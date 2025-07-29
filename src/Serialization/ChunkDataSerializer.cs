using HyatlasGame.Core.Chunks;
using HyatlasGame.Core.World;

namespace HyatlasGame.Core.Serialization
{
    /// <summary>
    /// Serializer für Chunk-Daten. Verpackt und entpackt Chunks mit dem zugrundeliegenden WorldGenerator.
    /// </summary>
    public class ChunkDataSerializer : ISerializer<Chunk>
    {
        private readonly IWorldGenerator _generator;

        public ChunkDataSerializer(IWorldGenerator generator)
        {
            _generator = generator;
        }

        public byte[] Serialize(Chunk chunk)
        {
            // nutzt den statischen ChunkSerializer für rohe Byte-Daten
            return Chunks.ChunkSerializer.Serialize(chunk);
        }

        public Chunk Deserialize(byte[] data)
        {
            // baut den Chunk mit demselben Generator wieder auf
            return Chunks.ChunkSerializer.Deserialize(data, _generator);
        }
    }
}
