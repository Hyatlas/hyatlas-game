using HyatlasGame.Core.Shared.Constants;
using HyatlasGame.Core.Shared.Enums;
using HyatlasGame.Core.Shared.Utils;

namespace HyatlasGame.Core.Zones
{
    public class SkyZone : IZone
    {
        public WorldZone ZoneType => WorldZone.Sky;
        public (int MinY, int MaxY) HeightRange => ZoneConstants.GetHeightRange(ZoneType);

        public byte GetBlockId(Vector3Int position)
        {
            // TODO: Himmel–Logik (z.B. Wolken, fliegende Inseln)
            return 0;
        }
    }
}
