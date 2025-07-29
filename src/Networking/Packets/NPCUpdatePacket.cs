using System;
using HyatlasGame.Core.Shared.Utils;

namespace HyatlasGame.Core.Networking.Packets
{
    /// <summary>
    /// Packet zum Synchronisieren von NPC-Updates im Multiplayer.
    /// </summary>
    [Serializable]
    public struct NPCUpdatePacket
    {
        /// <summary>
        /// Eindeutige ID des NPCs.
        /// </summary>
        public Guid NPCId;

        /// <summary>
        /// Aktuelle Weltposition des NPCs.
        /// </summary>
        public Vector3Int Position;

        /// <summary>
        /// Aktueller Zustand (z.B. Idle, Wander, Attack).
        /// </summary>
        public string State;

        public NPCUpdatePacket(Guid npcId, Vector3Int position, string state)
        {
            NPCId    = npcId;
            Position = position;
            State    = state;
        }
    }
}
