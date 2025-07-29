using HyatlasGame.Core.Blocks.Types;

namespace HyatlasGame.Core.Blocks
{
    /// <summary>
    /// Initialisiert und registriert alle verfügbaren Blocktypen.
    /// Muss beim Programmstart aufgerufen werden.
    /// </summary>
    public static class BlockInitializer
    {
        public static void RegisterAll()
        {
            // Leerer Raum
            BlockRegistry.RegisterBlock(new AirBlock());

            // Erde / Oberfläche
            BlockRegistry.RegisterBlock(new EarthBlock());

            // Weitere Blöcke später hier hinzufügen, z.B. WaterBlock, StoneBlock, etc.
        }
    }
}
