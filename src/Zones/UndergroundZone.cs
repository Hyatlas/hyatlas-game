using HyatlasGame.Core.Shared.Constants;
using HyatlasGame.Core.Shared.Enums;
using HyatlasGame.Core.Shared.Utils;

namespace HyatlasGame.Core.Zones
{
    public class UndergroundZone : IZone
    {
        public WorldZone ZoneType => WorldZone.Underground;
        public (int MinY, int MaxY) HeightRange => ZoneConstants.GetHeightRange(ZoneType);

        public byte GetBlockId(Vector3Int position)
        {
            // TODO: Untergrund–Logik (Höhlen, Erzadern)
            return 1; // voreingestellt = Earth
        }
    }
}
