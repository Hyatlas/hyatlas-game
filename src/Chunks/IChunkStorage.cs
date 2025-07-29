using System;
using System.IO;
using HyatlasGame.Core.World;

namespace HyatlasGame.Core.Chunks
{
    /// <summary>
    /// Schnittstelle für das Speichern und Laden von Chunks.
    /// </summary>
    public interface IChunkStorage
    {
        /// <summary>
        /// Speichert den gegebenen Chunk persistierend (z. B. auf Festplatte).
        /// </summary>
        void SaveChunk(Chunk chunk);

        /// <summary>
        /// Lädt einen Chunk anhand seiner Chunk-Koordinaten.
        /// </summary>
        /// <param name="chunkX">X-Koordinate des Chunks.</param>
        /// <param name="chunkZ">Z-Koordinate des Chunks.</param>
        /// <returns>Der geladene Chunk oder null, falls nicht vorhanden.</returns>
        Chunk? LoadChunk(int chunkX, int chunkZ);

        /// <summary>
        /// Prüft, ob für die angegebenen Chunk-Koordinaten bereits ein gespeicherter Chunk existiert.
        /// </summary>
        bool HasChunk(int chunkX, int chunkZ);
    }

    /// <summary>
    /// Speichert Chunks jeweils in einer eigenen Datei unter einem Basisverzeichnis.
    /// </summary>
    public class FileChunkStorage : IChunkStorage
    {
        private readonly string _baseDirectory;
        private readonly IWorldGenerator _generator;

        /// <summary>
        /// Legt einen Dateispeicher unter dem angegebenen Verzeichnis an.
        /// </summary>
        /// <param name="baseDirectory">Das Verzeichnis, in dem die Chunk‑Dateien liegen.</param>
        /// <param name="generator">Der Generator, den Deserialize benötigt.</param>
        public FileChunkStorage(string baseDirectory, IWorldGenerator generator)
        {
            if (string.IsNullOrWhiteSpace(baseDirectory))
                throw new ArgumentException("baseDirectory darf nicht leer sein", nameof(baseDirectory));
            _generator     = generator ?? throw new ArgumentNullException(nameof(generator));
            _baseDirectory = baseDirectory;
            Directory.CreateDirectory(_baseDirectory);
        }

        /// <inheritdoc />
        public void SaveChunk(Chunk chunk)
        {
            if (chunk == null) throw new ArgumentNullException(nameof(chunk));
            var path = GetChunkPath(chunk.ChunkX, chunk.ChunkZ);
            var data = ChunkSerializer.Serialize(chunk);
            File.WriteAllBytes(path, data);
        }

        /// <inheritdoc />
        public Chunk? LoadChunk(int chunkX, int chunkZ)
        {
            var path = GetChunkPath(chunkX, chunkZ);
            if (!File.Exists(path))
                return null;
            var data = File.ReadAllBytes(path);
            return ChunkSerializer.Deserialize(data, _generator);
        }

        /// <inheritdoc />
        public bool HasChunk(int chunkX, int chunkZ)
            => File.Exists(GetChunkPath(chunkX, chunkZ));

        private string GetChunkPath(int x, int z)
            => Path.Combine(_baseDirectory, $"chunk_{x}_{z}.dat");
    }
}
