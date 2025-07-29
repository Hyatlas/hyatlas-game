using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using HyatlasGame.Core.Chunks;
using HyatlasGame.Core.Rendering;
using WorldModel = HyatlasGame.Core.World.World;   // Alias für World-Klasse

namespace HyatlasGame.Core.Rendering
{
    /// <summary>
    /// Zwischenspeicher für Vertex‑/Index‑Buffer aller aktuell benötigten Chunks.
    /// Nutzt MeshService für asynchrone Mesherstellung und bietet
    /// Methoden zum direkten Zeichnen aller aktuell berechneten Chunks.
    /// </summary>
    public sealed class ChunkRenderCache : IDisposable
    {
        private readonly GraphicsDevice _device;
        private readonly MeshService    _meshService;

        // Cache vorhandener Renderer
        private readonly Dictionary<(int X, int Z), ChunkRenderComponent> _cache
            = new Dictionary<(int, int), ChunkRenderComponent>();

        // Letzte Render-Komponenten dieses Frames
        private List<ChunkRenderComponent> _lastFrameRenderers = new List<ChunkRenderComponent>();

        public ChunkRenderCache(GraphicsDevice device, MeshService meshService)
        {
            _device      = device      ?? throw new ArgumentNullException(nameof(device));
            _meshService = meshService ?? throw new ArgumentNullException(nameof(meshService));
            Console.WriteLine("[ChunkRenderCache] Initialisiert");
        }

        /// <summary>
        /// Synchronisiert den Cache mit geladenen Chunks:
        /// - Enqueue neuer/dirty Chunks im MeshService
        /// - Holt fertige Meshes aus dem Service und baut Renderer auf
        /// - Nutzt existierende Renderer als Platzhalter
        /// Liefert die Liste der Renderer, die in diesem Frame gezeichnet werden sollen.
        /// </summary>
        public List<ChunkRenderComponent> SyncAndGetRenderers(
            IEnumerable<(int X, int Z)> loaded,
            WorldModel world)
        {
            var toDraw     = new List<ChunkRenderComponent>();
            var stillAlive = new HashSet<(int, int)>();

            foreach (var coord in loaded)
            {
                stillAlive.Add(coord);
                var chunk = world.LoadChunk(coord.X, coord.Z);

                // Neu oder dirty: in Warteschlange aufnehmen
                if (chunk.IsDirty || !_cache.ContainsKey(coord))
                {
                    Console.WriteLine($"[ChunkRenderCache] Enqueue Chunk {coord} (IsDirty={chunk.IsDirty})");
                    _meshService.Enqueue(chunk);

                    if (_cache.TryGetValue(coord, out var placeholder))
                    {
                        Console.WriteLine($"[ChunkRenderCache] Nutze Platzhalter-Renderer für Chunk {coord}");
                        toDraw.Add(placeholder);
                    }

                    continue;
                }

                // Fertiges Mesh?
                if (_meshService.TryGetMesh(coord.X, coord.Z, out var meshData))
                {
                    Console.WriteLine($"[ChunkRenderCache] Baue neuen Renderer für Chunk {coord}");
                    if (_cache.TryGetValue(coord, out var oldRc))
                        oldRc.Dispose();

                    var newRc = new ChunkRenderComponent(_device, meshData);
                    _cache[coord] = newRc;
                    chunk.IsDirty = false;
                    toDraw.Add(newRc);
                }
                else
                {
                    Console.WriteLine($"[ChunkRenderCache] Mesh noch nicht fertig, existierender Renderer für Chunk {coord}");
                    toDraw.Add(_cache[coord]);
                }
            }

            // Entferne Renderer für entladene Chunks
            var toRemove = new List<(int, int)>();
            foreach (var kv in _cache)
            {
                if (!stillAlive.Contains(kv.Key))
                {
                    Console.WriteLine($"[ChunkRenderCache] Entferne Renderer für entladenen Chunk {kv.Key}");
                    kv.Value.Dispose();
                    toRemove.Add(kv.Key);
                }
            }
            foreach (var key in toRemove)
                _cache.Remove(key);

            // Merke Liste für spätere DrawAll-Aufrufe
            _lastFrameRenderers = toDraw;
            return toDraw;
        }

        /// <summary>
        /// Zeichnet alle aktuell berechneten Chunks ohne Textur (VertexPositionTexture ignoriert UVs).
        /// </summary>
        public void DrawAll(GraphicsDevice device)
        {
            foreach (var rc in _lastFrameRenderers)
                rc.Draw(device);
        }

        /// <summary>
        /// Zeichnet alle aktuell berechneten Chunks mit Textur über den gegebenen BasicEffect.
        /// </summary>
        public void DrawAll(GraphicsDevice device, BasicEffect effect)
        {
            if (effect == null) throw new ArgumentNullException(nameof(effect));
            foreach (var rc in _lastFrameRenderers)
                rc.Draw(device, effect);
        }

        public void Dispose()
        {
            Console.WriteLine("[ChunkRenderCache] Dispose, räume alle Renderer auf");
            foreach (var rc in _cache.Values)
                rc.Dispose();
            _cache.Clear();
        }
    }
}
