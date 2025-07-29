using System;

namespace HyatlasGame.Core.Blocks
{
    /// <summary>
    /// Abstrakte Basisklasse für alle Blocktypen.
    /// </summary>
    public abstract class Block : IBlock
    {
        /// <inheritdoc/>
        public abstract byte ID { get; }

        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public abstract bool IsSolid { get; }

        /// <summary>
        /// Optionale Hilfsmethode zum Debuggen.
        /// </summary>
        public override string ToString()
        {
            return $"{Name} (ID={ID}, Solid={IsSolid})";
        }
    }
}
