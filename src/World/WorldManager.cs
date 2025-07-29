using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using HyatlasGame.Core.Blocks;
using HyatlasGame.Core.Chunks;
using HyatlasGame.Core.Shared.Utils;
using HyatlasGame.Core.Shared.Enums;
using HyatlasGame.Core.Shared.Constants;

namespace HyatlasGame.Core.World
{
    /// <summary>
    /// Verwaltet die Spielwelt – Erzeugung, Laden/Entladen von Chunks, Spawn‑Positionen und Zugriff.
    /// Mit LRU‑Caching und persistentem Storage.
    /// </summary>
    public class WorldManager
    {
        private const int MaxLoadedChunks = 256;

        /// <summary>
        /// Die zentrale World-Instanz.
        /// </summary>
        public World World { get; }

        /// <summary>
        /// Der eingesetzte Weltgenerator.
        /// </summary>
        public IWorldGenerator Generator { get; }

        /// <summary>
        /// Das Storage-Backend für persistente Chunks.
        /// </summary>
        private readonly IChunkStorage _storage;

        // Aktuelle geladene Chunks mit LRU‑Reihenfolge (älteste vorne)
        private readonly Dictionary<(int X, int Z), Chunk> _loadedChunks = new();
        private readonly LinkedList<(int X, int Z)> _lruList = new();

        /// <summary>
        /// Gibt alle aktuell geladenen Chunk-Koordinaten zurück.
        /// </summary>
        public IEnumerable<(int X, int Z)> LoadedChunks => _lruList;

        /// <summary>
        /// Erzeugt einen neuen WorldManager mit dem angegebenen IWorldGenerator und Storage.
        /// </summary>
        public WorldManager(IWorldGenerator generator, IChunkStorage storage)
        {
            Generator = generator ?? throw new ArgumentNullException(nameof(generator));
            _storage  = storage   ?? throw new ArgumentNullException(nameof(storage));

            BlockInitializer.RegisterAll();
            World = new World(Generator);
        }

        /// <summary>
        /// Führe einen Frame-Update aus (z. B. NPC‑Logik, Physics).
        /// </summary>
        public void Update(float deltaTime)
        {
            // TODO: NPCs, Physics, Events etc.
        }

        /// <summary>
        /// Prüft, ob an der Position ein festes Block‑Objekt existiert.
        /// </summary>
        public bool IsSolidBlockAt(Vector3Int worldPos)
        {
            var id    = World.GetBlockAt(worldPos);
            var block = BlockRegistry.GetBlockById(id);
            return block != null && block.IsSolid;
        }

        /// <summary>
        /// Liefert eine Spawn‑Position oberhalb der obersten Erde in der Surface‑Zone.
        /// </summary>
        public Vector3Int GetSurfaceSpawnPosition(int worldX, int worldZ)
        {
            var (minY, maxY) = ZoneConstants.GetHeightRange(WorldZone.Surface);
            for (int y = maxY; y >= minY; y--)
            {
                if (World.GetBlockAt(new Vector3Int(worldX, y, worldZ)) != 0)
                    return new Vector3Int(worldX, y + 1, worldZ);
            }
            return new Vector3Int(worldX, minY + 1, worldZ);
        }

        /// <summary>
        /// Lädt und entlädt Chunks basierend auf der Spielerposition in Block‑Radien.
        /// Nutzt LRU‑Caching und persistentes Storage.
        /// </summary>
        public void UpdateLoadedChunks(Vector3 playerPos, int surfaceBlockRadius, int undergroundBlockRadius)
        {
            int chunkRadiusSurface     = (int)Math.Ceiling(surfaceBlockRadius     / (double)World.ChunkWidth);
            int chunkRadiusUnderground = (int)Math.Ceiling(undergroundBlockRadius / (double)World.ChunkWidth);

            int centerX = (int)Math.Floor(playerPos.X / World.ChunkWidth);
            int centerZ = (int)Math.Floor(playerPos.Z / World.ChunkDepth);

            var needed = new HashSet<(int X, int Z)>();

            // 1) Oberfläche: Kreis‑Approximation
            for (int dx = -chunkRadiusSurface; dx <= chunkRadiusSurface; dx++)
            for (int dz = -chunkRadiusSurface; dz <= chunkRadiusSurface; dz++)
                if (dx*dx + dz*dz <= chunkRadiusSurface*chunkRadiusSurface)
                    needed.Add((centerX + dx, centerZ + dz));

            // 2) Laden/neuladen
            foreach (var coord in needed)
            {
                if (!_loadedChunks.ContainsKey(coord))
                {
                    // versuchen: aus Storage laden
                    Chunk chunk = _storage.HasChunk(coord.X, coord.Z)
                        ? _storage.LoadChunk(coord.X, coord.Z)!
                        : new Chunk(coord.X, coord.Z, World.ChunkWidth, World.ChunkHeight, World.ChunkDepth, Generator);

                    // registrieren
                    _loadedChunks[coord] = chunk;
                    _lruList.AddLast(coord);
                }
                else
                {
                    // LRU‑Update
                    _lruList.Remove(coord);
                    _lruList.AddLast(coord);
                }
            }

            // 3) Unnötige oder überflüssige Chunks evicten
            var toRemove = new List<(int X, int Z)>();
            foreach (var coord in _loadedChunks.Keys)
                if (!needed.Contains(coord))
                    toRemove.Add(coord);

            while (_lruList.Count > MaxLoadedChunks)
                toRemove.Add(_lruList.First.Value);

            foreach (var coord in toRemove)
            {
                if (_loadedChunks.TryGetValue(coord, out var chunk))
                {
                    // persistieren
                    _storage.SaveChunk(chunk);
                    // aus Speicher entfernen
                    _loadedChunks.Remove(coord);
                    _lruList.Remove(coord);
                }
            }
        }
    }
}
