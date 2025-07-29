using HyatlasGame.Core.Shared.Constants;
using HyatlasGame.Core.Shared.Enums;
using HyatlasGame.Core.Shared.Utils;

namespace HyatlasGame.Core.Zones
{
    public class OrbitZone : IZone
    {
        public WorldZone ZoneType => WorldZone.Orbit;
        public (int MinY, int MaxY) HeightRange => ZoneConstants.GetHeightRange(ZoneType);

        public byte GetBlockId(Vector3Int position)
        {
            // TODO: Orbit-spezifische Logik (Raumstationen, Vakuum, Sterne)
            return 0; // voreingestellt = Air
        }
    }
}
