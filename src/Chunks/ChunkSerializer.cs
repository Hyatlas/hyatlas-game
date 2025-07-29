using System;
using System.IO;
using System.IO.Compression;
using HyatlasGame.Core.Shared.Utils;
using HyatlasGame.Core.World;

namespace HyatlasGame.Core.Chunks
{
    /// <summary>
    /// Serialisiert und deserialisiert Chunk-Daten (inkl. Kompression und Versionierung).
    /// </summary>
    public static class ChunkSerializer
    {
        private const int CurrentVersion = 1;

        /// <summary>
        /// Serialisiert die Roh-Blockdaten eines Chunks in einen komprimierten Byte-Stream.
        /// </summary>
        public static byte[] Serialize(Chunk chunk)
        {
            if (chunk == null) throw new ArgumentNullException(nameof(chunk));

            using var ms = new MemoryStream();
            // Kompression über GZip
            using (var gzip = new GZipStream(ms, CompressionLevel.Optimal, leaveOpen: true))
            using (var writer = new BinaryWriter(gzip))
            {
                // Format-Version
                writer.Write(CurrentVersion);

                // Chunk-Metadaten
                writer.Write(chunk.ChunkX);
                writer.Write(chunk.ChunkZ);
                writer.Write(chunk.Width);
                writer.Write(chunk.Height);
                writer.Write(chunk.Depth);

                // Blockdaten
                for (int x = 0; x < chunk.Width; x++)
                for (int y = 0; y < chunk.Height; y++)
                for (int z = 0; z < chunk.Depth; z++)
                {
                    byte id = chunk.GetBlock(new Vector3Int(x, y, z));
                    writer.Write(id);
                }

                writer.Flush();
            }
            // ms enthält jetzt den kompletten komprimierten Stream
            return ms.ToArray();
        }

        /// <summary>
        /// Deserialisiert einen Chunk aus einem komprimierten Byte-Stream.
        /// </summary>
        public static Chunk Deserialize(byte[] data, IWorldGenerator generator)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (generator == null) throw new ArgumentNullException(nameof(generator));

            using var ms = new MemoryStream(data);
            using var gzip = new GZipStream(ms, CompressionMode.Decompress, leaveOpen: true);
            using var reader = new BinaryReader(gzip);

            // Format-Version prüfen
            int version = reader.ReadInt32();
            if (version != CurrentVersion)
                throw new InvalidDataException($"Unsupported chunk version: {version}");

            // Chunk-Metadaten
            int chunkX = reader.ReadInt32();
            int chunkZ = reader.ReadInt32();
            int width  = reader.ReadInt32();
            int height = reader.ReadInt32();
            int depth  = reader.ReadInt32();

            // Chunk erstellen (füllt sich selbst initial)
            var chunk = new Chunk(chunkX, chunkZ, width, height, depth, generator);

            // Blockdaten einlesen und überschreiben
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            for (int z = 0; z < depth; z++)
            {
                byte id = reader.ReadByte();
                chunk.SetBlock(new Vector3Int(x, y, z), id);
            }

            return chunk;
        }
    }
}
