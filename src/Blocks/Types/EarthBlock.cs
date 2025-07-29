using HyatlasGame.Core.Blocks;

namespace HyatlasGame.Core.Blocks.Types
{
    /// <summary>
    /// Standard-Erde–Block, fest und kollidierbar.
    /// </summary>
    public class EarthBlock : Block
    {
        public override byte ID => 1;
        public override string Name => "Earth";
        public override bool IsSolid => true;
    }
}
