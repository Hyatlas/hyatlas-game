using System;
using System.Collections.Generic;

namespace HyatlasGame.Core.Blocks
{
    /// <summary>
    /// Zentrale Registry für alle Blocktypen.
    /// </summary>
    public static class BlockRegistry
    {
        private static readonly Dictionary<byte, IBlock> blocksById = new Dictionary<byte, IBlock>();
        private static readonly Dictionary<string, IBlock> blocksByName = new Dictionary<string, IBlock>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Registriert einen neuen Blocktyp.
        /// Muss beim Start ausgeführt werden, bevor Blöcke abgefragt werden.
        /// </summary>
        public static void RegisterBlock(IBlock block)
        {
            if (block == null) throw new ArgumentNullException(nameof(block));
            if (blocksById.ContainsKey(block.ID))
                throw new ArgumentException($"Ein Block mit der ID {block.ID} ist bereits registriert.");
            if (blocksByName.ContainsKey(block.Name))
                throw new ArgumentException($"Ein Block mit dem Namen '{block.Name}' ist bereits registriert.");

            blocksById[block.ID] = block;
            blocksByName[block.Name] = block;
        }

        /// <summary>
        /// Holt einen registrierten Block anhand seiner ID.
        /// </summary>
        public static IBlock GetBlockById(byte id)
        {
            blocksById.TryGetValue(id, out var block);
            return block;
        }

        /// <summary>
        /// Holt einen registrierten Block anhand seines Namens.
        /// </summary>
        public static IBlock GetBlockByName(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            blocksByName.TryGetValue(name, out var block);
            return block;
        }

        /// <summary>
        /// Liste aller registrierten Blöcke.
        /// </summary>
        public static IEnumerable<IBlock> GetAllBlocks() => blocksById.Values;
    }
}
