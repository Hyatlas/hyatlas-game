using HyatlasGame.Core.Shared.Constants;
using HyatlasGame.Core.Shared.Enums;
using HyatlasGame.Core.Shared.Utils;

namespace HyatlasGame.Core.World
{
    /// <summary>
    /// Einfacher, flacher Weltgenerator:
    /// Gibt Erde bis zu einer bestimmten Höhe, darüber Luft zurück.
    /// </summary>
    public class FlatWorldGenerator : IWorldGenerator
    {
        private readonly int _groundLevel;

        /// <summary>
        /// Externer Zugriff auf die definierte Bodenhöhe.
        /// </summary>
        public int GroundLevel => _groundLevel;

        /// <summary>
        /// Erstellt einen FlatWorldGenerator mit einer definierbaren Bodenhöhe.
        /// Standardmäßig liegt der Boden am unteren Ende der Surface-Zone.
        /// </summary>
        /// <param name="groundLevel">
        /// Die Y-Höhe, bis zu der Erde generiert wird (inklusive).
        /// </param>
        public FlatWorldGenerator(int? groundLevel = null)
        {
            if (groundLevel.HasValue)
            {
                _groundLevel = groundLevel.Value;
            }
            else
            {
                // Standard: oberstes Minimum der Surface-Zone
                var range = ZoneConstants.GetHeightRange(WorldZone.Surface);
                _groundLevel = range.MinY;
            }
        }

        /// <inheritdoc/>
        public byte GetBlockId(Vector3Int position)
        {
            // Ist die aktuelle Höhe unter oder gleich Boden-Level?
            if (position.Y <= _groundLevel)
            {
                // Erde
                return 1; // EarthBlock.ID
            }
            else
            {
                // Luft
                return 0; // AirBlock.ID
            }
        }
    }
}
