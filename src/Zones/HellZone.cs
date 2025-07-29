using HyatlasGame.Core.Shared.Constants;
using HyatlasGame.Core.Shared.Enums;
using HyatlasGame.Core.Shared.Utils;

namespace HyatlasGame.Core.Zones
{
    public class HellZone : IZone
    {
        public WorldZone ZoneType => WorldZone.Hell;
        public (int MinY, int MaxY) HeightRange => ZoneConstants.GetHeightRange(ZoneType);

        public byte GetBlockId(Vector3Int position)
        {
            // TODO: Hölle–Logik (Lava, Dämonenblöcke)
            return 1; // voreingestellt = Earth (später vielleicht Lava)
        }
    }
}
