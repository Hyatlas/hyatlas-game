using HyatlasGame.Core.Blocks;

namespace HyatlasGame.Core.Blocks.Types
{
    /// <summary>
    /// Repräsentiert luftleeren Raum – unsichtbar und nicht kollidierbar.
    /// </summary>
    public class AirBlock : Block
    {
        public override byte ID => 0;
        public override string Name => "Air";
        public override bool IsSolid => false;
    }
}
