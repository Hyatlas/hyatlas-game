using System;
using System.Collections.Generic;
using HyatlasGame.Core.Shared.Enums;

namespace HyatlasGame.Core.Shared.Constants
{
    /// <summary>
    /// Enthält die Y-Höhenbereiche für jede WorldZone.
    /// Die Werte sind Platzhalter und können später angepasst werden.
    /// </summary>
    public static class ZoneConstants
    {
        /// <summary>
        /// Definiert für jede Zone das (MinY, MaxY)-Intervall.
        /// </summary>
        public static readonly IReadOnlyDictionary<WorldZone, (int MinY, int MaxY)> HeightRanges
            = new Dictionary<WorldZone, (int MinY, int MaxY)>
        {
            { WorldZone.Hell,         (MinY: -100, MaxY: -1)   },
            { WorldZone.Underground,  (MinY: 0,    MaxY:  15)  },
            { WorldZone.Surface,      (MinY: 16,   MaxY:  80)  },
            { WorldZone.Sky,          (MinY: 81,   MaxY: 128)  },
            { WorldZone.Stratosphere, (MinY: 129,  MaxY: 192)  },
            { WorldZone.Orbit,        (MinY: 193,  MaxY: 256)  },
        };

        /// <summary>
        /// Gibt für die angegebene Zone das (MinY, MaxY)-Intervall zurück.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Wenn die Zone nicht definiert ist.</exception>
        public static (int MinY, int MaxY) GetHeightRange(WorldZone zone)
        {
            if (!HeightRanges.TryGetValue(zone, out var range))
                throw new KeyNotFoundException($"Keine Höhenwerte für Zone {zone} definiert.");
            return range;
        }
    }
}
