using System;
using HyatlasGame.Core.Shared.Utils;

namespace HyatlasGame.Core.Networking.Packets
{
    /// <summary>
    /// Packet zum Synchronisieren von Blockänderungen im Multiplayer.
    /// </summary>
    [Serializable]
    public struct BlockUpdatePacket
    {
        /// <summary>
        /// Chunk-Koordinaten, in dem der Block steht.
        /// </summary>
        public int ChunkX;
        public int ChunkZ;

        /// <summary>
        /// Lokale Position innerhalb des Chunks.
        /// </summary>
        public Vector3Int LocalPosition;

        /// <summary>
        /// Neue Block-ID, die gesetzt wurde.
        /// </summary>
        public byte NewBlockId;

        public BlockUpdatePacket(int chunkX, int chunkZ, Vector3Int localPosition, byte newBlockId)
        {
            ChunkX       = chunkX;
            ChunkZ       = chunkZ;
            LocalPosition = localPosition;
            NewBlockId   = newBlockId;
        }
    }
}
