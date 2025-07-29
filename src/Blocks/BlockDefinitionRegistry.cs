// File: src/Blocks/BlockDefinitionRegistry.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;              // für TitleContainer.OpenStream
using Microsoft.Xna.Framework.Graphics;    // für Texture2D
using HyatlasGame.Core.Shared.Utils;
using HyatlasGame.Core.Blocks.Enums;

namespace HyatlasGame.Core.Blocks
{
    /// <summary>
    /// Lädt Block-Definitionen aus einer JSON-Datei und stellt sie über ID bereit.
    /// </summary>
    public static class BlockDefinitionRegistry
    {
        private static readonly Dictionary<byte, BlockDefinition> _definitions
            = new Dictionary<byte, BlockDefinition>();

        /// <summary>
        /// Lädt alle Block-Definitionen aus der angegebenen JSON-Datei.
        /// Muss einmal beim Programmstart aufgerufen werden.
        /// </summary>
        /// <param name="jsonPath">Pfad zu blocks.json im Content-Ordner (z.B. "Content/blocks.json")</param>
        public static void LoadDefinitions(string jsonPath)
        {
            // Titelcontainer für XNA/MonoGame öffnet Dateien aus dem Content-Ordner
            using var stream  = TitleContainer.OpenStream(jsonPath);
            using var reader  = new StreamReader(stream);
            string    json    = reader.ReadToEnd();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters =
                {
                    // Damit Enum-Felder als Strings geparst werden (CamelCase möglich)
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true)
                }
            };

            var list = JsonSerializer.Deserialize<List<BlockDefinitionJson>>(json, options)
                       ?? throw new InvalidOperationException("Failed to parse block definitions.");

            _definitions.Clear();
            foreach (var dto in list)
            {
                // Wir erwarten, dass textureFolder etwa "Blocks/Earth" ist
                // und darin eine Datei "texture.png" liegt.
                string relTexPath = Path.Combine(dto.TextureFolder, "texture.png");

                // Erzeuge für jede Seite eine TextureRegion mit demselben Pfad
                var regions = new TextureRegion[6];
                for (int i = 0; i < 6; i++)
                    regions[i] = new TextureRegion(relTexPath);

                // Lade die Textur einmalig
                using var texStream = TitleContainer.OpenStream(Path.Combine("Content", relTexPath));
                var texture = Texture2D.FromStream(GraphicsDeviceHelper.GraphicsDevice, texStream);

                // Baue die Definition
                var def = new BlockDefinition(
                    dto.Id,
                    dto.Name,
                    dto.IsSolid,
                    dto.IsOpaque,
                    regions,
                    dto.CollisionShape,
                    dto.RotationFlags,
                    dto.ModelPath
                );

                // Merke die Textur im Cache
                BlockTextureCache.Register(dto.Id, texture);
                _definitions[def.ID] = def;
            }
        }

        /// <summary>
        /// Holt die Definition für die gegebene Block-ID.
        /// </summary>
        public static BlockDefinition GetDefinition(byte id)
        {
            if (_definitions.TryGetValue(id, out var def))
                return def;
            throw new KeyNotFoundException($"No BlockDefinition for ID={id}");
        }

        // DTO-Klasse für JSON-Deserialisierung:
        private class BlockDefinitionJson
        {
            public byte Id { get; set; }
            public string Name { get; set; } = null!;
            public bool IsSolid { get; set; }
            public bool IsOpaque { get; set; }
            public string TextureFolder { get; set; } = null!;
            public string? ModelPath { get; set; }
            public CollisionShape CollisionShape { get; set; }
            public RotationFlags RotationFlags { get; set; }
        }
    }

    /// <summary>
    /// Hilfsklasse zum Speichern/laden der Texturen pro Block-ID.
    /// </summary>
    internal static class BlockTextureCache
    {
        private static readonly Dictionary<byte, Texture2D> _textures = new();

        public static void Register(byte id, Texture2D tex) => _textures[id] = tex;
        public static Texture2D Get(byte id) =>
            _textures.TryGetValue(id, out var t)
            ? t
            : throw new KeyNotFoundException($"No texture for Block ID={id}");
    }

    /// <summary>
    /// Helfer, um in statischen Methoden an das GraphicsDevice zu kommen.
    /// Muss einmal in Game1.Initialize durch Aufruf von
    /// GraphicsDeviceHelper.Initialize(GraphicsDevice) initialisiert werden.
    /// </summary>
    internal static class GraphicsDeviceHelper
    {
        public static GraphicsDevice GraphicsDevice { get; private set; } = null!;
        public static void Initialize(GraphicsDevice device) =>
            GraphicsDevice = device ?? throw new ArgumentNullException(nameof(device));
    }
}
