using System;
using HyatlasGame.Core.Networking.Packets;

namespace HyatlasGame.Core.Networking
{
    /// <summary>
    /// Schnittstelle für den Netzwerk-Server.
    /// Verantwortlich für Empfang von Client-Verbindungen und Senden/Empfangen von Paketen.
    /// </summary>
    public interface INetworkServer : IDisposable
    {
        /// <summary>
        /// Startet den Server und lauscht auf dem angegebenen Port.
        /// </summary>
        /// <param name="port">Port, auf dem der Server Verbindungen akzeptiert.</param>
        void Start(int port);

        /// <summary>
        /// Stoppt den Server und trennt alle Verbindungen.
        /// </summary>
        void Stop();

        /// <summary>
        /// Sendet ein Block-Update-Paket an alle verbundenen Clients.
        /// </summary>
        void Broadcast(BlockUpdatePacket packet);

        /// <summary>
        /// Sendet ein NPC-Update-Paket an alle verbundenen Clients.
        /// </summary>
        void Broadcast(NPCUpdatePacket packet);

        /// <summary>
        /// Event, wenn ein Client verbunden ist (liefert Client-ID oder Session-Info).
        /// </summary>
        event Action<Guid> OnClientConnected;

        /// <summary>
        /// Event, wenn ein Client die Verbindung trennt.
        /// </summary>
        event Action<Guid> OnClientDisconnected;

        /// <summary>
        /// Event, wenn ein Block-Update von einem Client empfangen wird.
        /// </summary>
        event Action<Guid, BlockUpdatePacket> OnBlockUpdateReceived;

        /// <summary>
        /// Event, wenn ein NPC-Update von einem Client empfangen wird.
        /// </summary>
        event Action<Guid, NPCUpdatePacket> OnNPCUpdateReceived;
    }
}
