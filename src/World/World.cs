using System;
using System.Collections.Generic;
using HyatlasGame.Core.Chunks;
using HyatlasGame.Core.Shared.Utils;

namespace HyatlasGame.Core.World
{
    /// <summary>
    /// Repräsentiert die gesamte Spielwelt aus Chunks.
    /// </summary>
    public class World
    {
        private readonly IWorldGenerator _generator;
        private readonly Dictionary<(int ChunkX, int ChunkZ), Chunk> _chunks
            = new Dictionary<(int, int), Chunk>();

        public const int ChunkWidth  = 16;
        public const int ChunkHeight = 256;
        public const int ChunkDepth  = 16;

        public World(IWorldGenerator generator)
        {
            _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        }

        /// <summary>
        /// Gibt die Block-ID an der angegebenen Weltposition zurück.
        /// </summary>
        public byte GetBlockAt(Vector3Int position)
        {
            var (cx, cz) = GetChunkCoords(position);
            var chunk = LoadChunk(cx, cz);
            var local = new Vector3Int(
                Mod(position.X, ChunkWidth),
                position.Y,
                Mod(position.Z, ChunkDepth));
            return chunk.GetBlock(local);
        }

        /// <summary>
        /// Setzt an der angegebenen Weltposition den Block mit der gegebenen ID.
        /// </summary>
        public void SetBlockAt(Vector3Int position, byte blockId)
        {
            var (cx, cz) = GetChunkCoords(position);
            var chunk = LoadChunk(cx, cz);
            var local = new Vector3Int(
                Mod(position.X, ChunkWidth),
                position.Y,
                Mod(position.Z, ChunkDepth));
            chunk.SetBlock(local, blockId);
        }

        /// <summary>
        /// Lädt oder erzeugt einen Chunk an den angegebenen Chunk-Koordinaten.
        /// </summary>
        public Chunk LoadChunk(int chunkX, int chunkZ)
        {
            var key = (chunkX, chunkZ);
            if (!_chunks.TryGetValue(key, out var chunk))
            {
                chunk = new Chunk(chunkX, chunkZ, ChunkWidth, ChunkHeight, ChunkDepth, _generator);
                _chunks[key] = chunk;
            }
            return chunk;
        }

        /// <summary>
        /// Entfernt einen geladenen Chunk aus dem Speicher.
        /// </summary>
        public void RemoveChunk(int chunkX, int chunkZ)
        {
            _chunks.Remove((chunkX, chunkZ));
        }

        /// <summary>
        /// Ermittelt die Chunk-Koordinaten für eine Weltposition.
        /// </summary>
        public static (int ChunkX, int ChunkZ) GetChunkCoords(Vector3Int pos)
        {
            int cx = FloorDiv(pos.X, ChunkWidth);
            int cz = FloorDiv(pos.Z, ChunkDepth);
            return (cx, cz);
        }

        private static int FloorDiv(int a, int b)
        {
            // korrektes Abrunden bei negativen Koordinaten
            return (int)Math.Floor((double)a / b);
        }

        private static int Mod(int a, int b)
        {
            var m = a % b;
            return m < 0 ? m + b : m;
        }
    }
}
