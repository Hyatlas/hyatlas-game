using HyatlasGame.Core.Shared.Enums;
using HyatlasGame.Core.Shared.Utils;

namespace HyatlasGame.Core.Zones
{
    /// <summary>
    /// Schnittstelle für eine vertikale Spielzone (z.B. Orbit, Surface, Hell).
    /// Definiert, welche Y-Höhen sie abdeckt und wie Blocks in dieser Zone generiert werden.
    /// </summary>
    public interface IZone
    {
        /// <summary>
        /// Der Typ dieser Zone.
        /// </summary>
        WorldZone ZoneType { get; }

        /// <summary>
        /// Der in ZoneConstants definierte Höhenbereich (MinY, MaxY).
        /// </summary>
        (int MinY, int MaxY) HeightRange { get; }

        /// <summary>
        /// Liefert die Block-ID, die an der gegebenen Position in dieser Zone stehen soll.
        /// </summary>
        /// <param name="position">Weltkoordinaten (x,y,z)</param>
        byte GetBlockId(Vector3Int position);
    }
}
