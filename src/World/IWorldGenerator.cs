using HyatlasGame.Core.Shared.Utils;

namespace HyatlasGame.Core.World
{
    /// <summary>
    /// Liefert für jede Weltkoordinate die Block‑ID (0 = Luft).
    /// </summary>
    public interface IWorldGenerator
    {
        byte GetBlockId(Vector3Int worldPosition);
    }
}
