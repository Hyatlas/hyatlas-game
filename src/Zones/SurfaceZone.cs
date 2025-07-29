using HyatlasGame.Core.Shared.Constants;
using HyatlasGame.Core.Shared.Enums;
using HyatlasGame.Core.Shared.Utils;

namespace HyatlasGame.Core.Zones
{
    public class SurfaceZone : IZone
    {
        public WorldZone ZoneType => WorldZone.Surface;
        public (int MinY, int MaxY) HeightRange => ZoneConstants.GetHeightRange(ZoneType);

        public byte GetBlockId(Vector3Int position)
        {
            // TODO: Oberflächen–Logik (Biomzuweisung, flache Erde)
            return position.Y <= HeightRange.MinY ? (byte)1 : (byte)0;
        }
    }
}
