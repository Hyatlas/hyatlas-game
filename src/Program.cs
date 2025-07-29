using System;
using HyatlasGame.Core.Shared.Utils;
using HyatlasGame.Core.Shared.Constants;
using HyatlasGame.Core.Shared.Enums;
using HyatlasGame.Core.World;
using HyatlasGame.Core.Chunks;
using HyatlasGame.Core.Blocks;

// Alias, damit wir nicht mit dem gleichnamigen Namespace kollidieren
using GameWorld = HyatlasGame.Core.World.World;

namespace HyatlasGame.Core
{
    /// <summary>
    /// Headless‑Testprogramm für den Core (Server / CLI‑Singleplayer).
    /// </summary>
    internal static class Program
    {
        private static void Main()
        {
            Console.WriteLine("=== HyatlasGame Core (CLI) ===");

            // 1) Generator
            var generator = new FlatWorldGenerator();

            // 2) Persistentes Storage
            var storage = new FileChunkStorage("chunks", generator);

            // 3) WorldManager
            var manager = new WorldManager(generator, storage);

            // 4) Spieler‑Start (Surface‑Zone)
            var sRange    = ZoneConstants.GetHeightRange(WorldZone.Surface);
            var playerPos = new Vector3Int(0, sRange.MinY + 1, 0);

            Console.WriteLine($"Spawn‑Position: {playerPos}");
            Console.WriteLine("Befehle: move x y z | get | set id | save | load | exit");

            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var cmd   = parts[0].ToLowerInvariant();

                try
                {
                    switch (cmd)
                    {
                        case "move" when parts.Length == 4:
                            playerPos = new Vector3Int(
                                int.Parse(parts[1]),
                                int.Parse(parts[2]),
                                int.Parse(parts[3]));
                            Console.WriteLine($"Player → {playerPos}");
                            break;

                        case "get":
                            var id    = manager.World.GetBlockAt(playerPos);
                            var block = BlockRegistry.GetBlockById(id);
                            Console.WriteLine($"Block @ {playerPos}: ID={id}, Name={block?.Name ?? "?"}");
                            break;

                        case "set" when parts.Length == 2:
                            var newId  = byte.Parse(parts[1]);
                            manager.World.SetBlockAt(playerPos, newId);
                            var nb = BlockRegistry.GetBlockById(newId);
                            Console.WriteLine($"Block @ {playerPos} = {newId} ({nb?.Name ?? "?"})");
                            break;

                        case "save":
                        {
                            var (cx, cz) = GameWorld.GetChunkCoords(playerPos);
                            storage.SaveChunk(manager.World.LoadChunk(cx, cz));
                            Console.WriteLine($"Chunk ({cx},{cz}) gespeichert.");
                            break;
                        }

                        case "load":
                        {
                            var (cx, cz) = GameWorld.GetChunkCoords(playerPos);
                            if (!storage.HasChunk(cx, cz))
                            {
                                Console.WriteLine($"Kein gespeicherter Chunk ({cx},{cz}).");
                                break;
                            }

                            var chunk = storage.LoadChunk(cx, cz)!;
                            manager.World.RemoveChunk(cx, cz);
                            manager.World.LoadChunk(cx, cz);   // neu einhängen
                            Console.WriteLine($"Chunk ({cx},{cz}) neu geladen.");
                            break;
                        }

                        case "exit":
                            return;

                        default:
                            Console.WriteLine("Unbekannt – cmds: move/get/set/save/load/exit");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler: {ex.Message}");
                }
            }
        }
    }
}
