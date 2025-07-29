using HyatlasGame.Core.Shared.Constants;
using HyatlasGame.Core.Shared.Enums;
using HyatlasGame.Core.Shared.Utils;

namespace HyatlasGame.Core.Zones
{
    public class StratosphereZone : IZone
    {
        public WorldZone ZoneType => WorldZone.Stratosphere;
        public (int MinY, int MaxY) HeightRange => ZoneConstants.GetHeightRange(ZoneType);

        public byte GetBlockId(Vector3Int position)
        {
            // TODO: Stratosphäre–Logik (leichte Atmosphäre, Wolkenlöcher)
            return 0;
        }
    }
}
