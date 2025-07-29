using System;
using HyatlasGame.Core.Networking.Packets;

namespace HyatlasGame.Core.Networking
{
    /// <summary>
    /// Schnittstelle für den Netzwerk-Client.
    /// Verantwortlich für Verbindungsaufbau zum Server und Senden/Empfangen von Paketen.
    /// </summary>
    public interface INetworkClient : IDisposable
    {
        /// <summary>
        /// Baut eine Verbindung zum angegebenen Server auf.
        /// </summary>
        /// <param name="host">Hostname oder IP des Servers.</param>
        /// <param name="port">Port des Servers.</param>
        void Connect(string host, int port);

        /// <summary>
        /// Sendet ein Block-Update-Paket an den Server.
        /// </summary>
        void Send(BlockUpdatePacket packet);

        /// <summary>
        /// Sendet ein NPC-Update-Paket an den Server.
        /// </summary>
        void Send(NPCUpdatePacket packet);

        /// <summary>
        /// Wird aufgerufen, wenn ein Block-Update vom Server empfangen wird.
        /// </summary>
        event Action<BlockUpdatePacket> OnBlockUpdateReceived;

        /// <summary>
        /// Wird aufgerufen, wenn ein NPC-Update vom Server empfangen wird.
        /// </summary>
        event Action<NPCUpdatePacket> OnNPCUpdateReceived;

        /// <summary>
        /// Trennt die Verbindung zum Server.
        /// </summary>
        void Disconnect();
    }
}
