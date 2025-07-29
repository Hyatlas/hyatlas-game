using System.Text.Json.Serialization;

namespace HyatlasGame.Core.Shared.Utils
{
    /// <summary>
    /// Repräsentiert die Textur eines Block-Seitenelements.
    /// </summary>
    public class TextureRegion
    {
        /// <summary>
        /// Pfad zur Bilddatei (relativ zum Content-Ordner), z.B. "Blocks/Earth/top.png".
        /// </summary>
        public string TexturePath { get; set; } = null!;

        /// <summary>
        /// Konstruktor für JSON-Deserialisierung.
        /// </summary>
        public TextureRegion() { }

        /// <summary>
        /// Erzeugt eine neue Region mit dem gegebenen Pfad.
        /// </summary>
        public TextureRegion(string texturePath)
        {
            TexturePath = texturePath;
        }
    }
}
