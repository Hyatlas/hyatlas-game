namespace HyatlasGame.Core.Blocks
{
    /// <summary>
    /// Repräsentiert einen Blocktyp in der Voxel-Welt.
    /// </summary>
    public interface IBlock
    {
        /// <summary>
        /// Einmalige ID des Blocktyps (z.B. 0 = Air, 1 = Earth).
        /// </summary>
        byte ID { get; }

        /// <summary>
        /// Anzeigename des Blocks.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gibt an, ob der Block fest (kollidierbar) ist.
        /// </summary>
        bool IsSolid { get; }
    }
}
