using System;
using HyatlasGame.Core.Shared.Utils;
using HyatlasGame.Core.World;

namespace HyatlasGame.Core.Chunks
{
    /// <summary>
    /// Repräsentiert einen Chunk und hält einen gecachten Mesh‑Datensatz
    /// (<see cref="ChunkMeshGenerator.MeshData"/>), der nur dann
    /// neu berechnet wird, wenn <see cref="IsDirty"/> == <c>true</c>.
    /// </summary>
    public class Chunk
    {
        public int ChunkX { get; }
        public int ChunkZ { get; }
        public int Width  { get; }
        public int Height { get; }
        public int Depth  { get; }

        private readonly IWorldGenerator _generator;
        private readonly byte[,,]        _blocks;

        /// <summary>Wird auf <c>true</c> gesetzt, sobald ein Voxel geändert wurde.</summary>
        public bool IsDirty { get; set; } = true;

        /// <summary>Zuletzt erzeugte Geometriedaten für den Renderer.</summary>
        private ChunkMeshGenerator.MeshData? _meshCache;

        public event Action<Chunk>? OnBlockChanged;

        public Chunk(int chunkX, int chunkZ, int width, int height, int depth, IWorldGenerator generator)
        {
            ChunkX     = chunkX;
            ChunkZ     = chunkZ;
            Width      = width;
            Height     = height;
            Depth      = depth;
            _generator = generator ?? throw new ArgumentNullException(nameof(generator));

            _blocks = new byte[Width, Height, Depth];
            Generate();
        }

        /* ---------- Block‑Zugriff ---------- */

        public byte GetBlock(Vector3Int local)
        {
            if (!IsInBounds(local))
                throw new ArgumentOutOfRangeException(nameof(local), "Position liegt außerhalb des Chunks.");

            return _blocks[local.X, local.Y, local.Z];
        }

        public void SetBlock(Vector3Int local, byte id)
        {
            if (!IsInBounds(local))
                throw new ArgumentOutOfRangeException(nameof(local));

            _blocks[local.X, local.Y, local.Z] = id;
            IsDirty = true;
            OnBlockChanged?.Invoke(this);
        }

        private bool IsInBounds(in Vector3Int p) =>
               p.X >= 0 && p.X < Width
            && p.Y >= 0 && p.Y < Height
            && p.Z >= 0 && p.Z < Depth;

        /* ---------- Mesh‑Handling ---------- */

        /// <summary>
        /// Liefert einen gültigen <see cref="ChunkMeshGenerator.MeshData"/>‑Cache.
        /// </summary>
        public ChunkMeshGenerator.MeshData GetMesh(ChunkMeshGenerator gen)
        {
            if (_meshCache == null || IsDirty)
            {
                _meshCache = gen.GenerateMesh(this);
                IsDirty    = false;
            }

            return _meshCache.Value;
        }

        /* ---------- Initial‑Füllung ---------- */

        private void Generate()
        {
            for (int x = 0; x < Width;  x++)
            for (int y = 0; y < Height; y++)
            for (int z = 0; z < Depth;  z++)
            {
                int worldX = ChunkX * Width  + x;
                int worldY = y;
                int worldZ = ChunkZ * Depth  + z;

                _blocks[x, y, z] = _generator.GetBlockId(new Vector3Int(worldX, worldY, worldZ));
            }

            IsDirty = true;
        }
    }
}
