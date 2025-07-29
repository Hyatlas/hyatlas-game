using System;
using System.Collections.Generic;
using System.Collections.Generic;
using HyatlasGame.Core.Shared.Enums;
using HyatlasGame.Core.Shared.Constants;
using HyatlasGame.Core.Shared.Utils;
using HyatlasGame.Core.Zones;

namespace HyatlasGame.Core.World
{
    /// <summary>
    /// Leitet anhand der Höhe an die passenden IZone-Instanzen weiter.
    /// </summary>
    public class CompositeWorldGenerator : IWorldGenerator
    {
        private readonly Dictionary<WorldZone, IZone> _zones;

        public CompositeWorldGenerator(IEnumerable<IZone> zones)
        {
            _zones = new Dictionary<WorldZone, IZone>();
            foreach (var z in zones)
            {
                _zones[z.ZoneType] = z;
            }
        }

        public byte GetBlockId(Vector3Int position)
        {
            // Bestimme anhand der Y-Koordinate die Zone
            var zoneType = DetermineZone(position.Y);
            if (_zones.TryGetValue(zoneType, out var zone))
            {
                return zone.GetBlockId(position);
            }
            // Fallback: Luft
            return 0;
        }

        private WorldZone DetermineZone(int y)
        {
            foreach (var kv in ZoneConstants.HeightRanges)
            {
                var z = kv.Key;
                var (min, max) = kv.Value;
                if (y >= min && y <= max)
                    return z;
            }
            // darüber oder darunter: Orbit bzw. Hell
            if (y > ZoneConstants.HeightRanges[WorldZone.Orbit].MaxY)
                return WorldZone.Orbit;
            return WorldZone.Hell;
        }
    }
}
