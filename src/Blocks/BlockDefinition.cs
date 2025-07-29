// File: src/Blocks/BlockDefinition.cs
using System;
using Microsoft.Xna.Framework;
using HyatlasGame.Core.Blocks.Enums;    // Für CollisionShape und RotationFlags
using HyatlasGame.Core.Shared.Utils;

namespace HyatlasGame.Core.Blocks
{
    /// <summary>
    /// Enthält alle Metadaten eines Blocktyps: Eigenschaften, Texturen, Modellpfad und Kollisionsform.
    /// </summary>
    public class BlockDefinition
    {
        /// <summary>Einmalige ID des Blocktyps (z.B. 0 = Air, 1 = Earth).</summary>
        public byte ID { get; }

        /// <summary>Anzeigename des Blocks.</summary>
        public string Name { get; }

        /// <summary>Gibt an, ob der Block fest (kollidierbar) ist.</summary>
        public bool IsSolid { get; }

        /// <summary>Gibt an, ob der Block undurchsichtig ist (Sichtbehinderung).</summary>
        public bool IsOpaque { get; }

        /// <summary>Texturregionen für jede Seite (0:+X,1:-X,2:+Y,3:-Y,4:+Z,5:-Z).</summary>
        public TextureRegion[] SideTextures { get; }

        /// <summary>Optionaler Pfad zu einem externen 3D-Modell (OBJ/GLTF).</summary>
        public string? ModelPath { get; }

        /// <summary>Definition der Kollisionsform: Würfel, Zylinder oder Mesh.</summary>
        public CollisionShape Collision { get; }

        /// <summary>Rotationsoptionen beim Platzieren (z.B. nur Vierfach-Rotation um Y).</summary>
        public RotationFlags RotationFlags { get; }

        public BlockDefinition(
            byte id,
            string name,
            bool isSolid,
            bool isOpaque,
            TextureRegion[] sideTextures,
            CollisionShape collision,
            RotationFlags rotationFlags,
            string? modelPath = null)
        {
            if (sideTextures == null || sideTextures.Length != 6)
                throw new ArgumentException("Es müssen genau 6 TextureRegions angegeben werden.", nameof(sideTextures));

            ID = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsSolid = isSolid;
            IsOpaque = isOpaque;
            SideTextures = sideTextures;
            Collision = collision;
            RotationFlags = rotationFlags;
            ModelPath = modelPath;
        }
    }
}
