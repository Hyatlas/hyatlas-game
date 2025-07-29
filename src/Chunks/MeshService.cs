using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using HyatlasGame.Core.Chunks;

namespace HyatlasGame.Core.Chunks
{
    /// <summary>
    /// Dienst, der Chunks im Hintergrund mesht und fertige MeshData puffert.
    /// </summary>
    public class MeshService : IDisposable
    {
        private readonly ChunkMeshGenerator _generator;
        private readonly ConcurrentQueue<Chunk> _queue = new();
        private readonly ConcurrentDictionary<(int X, int Z), ChunkMeshGenerator.MeshData> _meshes
            = new();
        private readonly CancellationTokenSource _cts = new();

        /// <summary>
        /// Startet den Hintergrund-Task mit dem gegebenen Generator.
        /// </summary>
        public MeshService(ChunkMeshGenerator generator)
        {
            _generator = generator ?? throw new ArgumentNullException(nameof(generator));
            Console.WriteLine("[MeshService] Starte WorkerLoop");
            Task.Run(WorkerLoop, _cts.Token);
        }

        /// <summary>
        /// Fügt einen Chunk zur asynchronen Mesh-Erzeugung hinzu.
        /// </summary>
        public void Enqueue(Chunk chunk)
        {
            var coord = (chunk.ChunkX, chunk.ChunkZ);
            if (!_meshes.ContainsKey(coord))
            {
                _queue.Enqueue(chunk);
                Console.WriteLine($"[MeshService] Enqueued Chunk {coord} (IsDirty={chunk.IsDirty})");
            }
        }

        /// <summary>
        /// Versucht, ein fertig generiertes Mesh zurückzugeben.
        /// </summary>
        public bool TryGetMesh(int chunkX, int chunkZ, out ChunkMeshGenerator.MeshData mesh)
        {
            var coord = (chunkX, chunkZ);
            if (_meshes.TryRemove(coord, out mesh))
            {
                Console.WriteLine($"[MeshService] Mesh bereit für Chunk {coord}");
                return true;
            }
            return false;
        }

        private async Task WorkerLoop()
        {
            while (!_cts.IsCancellationRequested)
            {
                if (_queue.TryDequeue(out var chunk))
                {
                    var coord = (chunk.ChunkX, chunk.ChunkZ);
                    Console.WriteLine($"[MeshService] Bearbeite Chunk {coord}");
                    try
                    {
                        var mesh = _generator.GenerateMesh(chunk);
                        _meshes[coord] = mesh;
                        chunk.IsDirty = false;
                        Console.WriteLine($"[MeshService] Fertig Mesh für Chunk {coord}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[MeshService] Fehler beim Meshen {coord}: {ex.Message}");
                    }
                }
                else
                {
                    await Task.Delay(10, _cts.Token).ConfigureAwait(false);
                }
            }
        }

        public void Dispose()
        {
            Console.WriteLine("[MeshService] Dispose, stoppe WorkerLoop");
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}