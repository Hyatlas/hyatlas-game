using System;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace HyatlasGame.Core.Shared.Utils
{
    /// <summary>
    /// Ganzzahl‑Vektor für Block‑/Chunk‑Koordinaten.
    /// </summary>
    public readonly struct Vector3Int : IEquatable<Vector3Int>
    {
        /* ---------- Felder ---------- */
        public int X { get; init; }
        public int Y { get; init; }
        public int Z { get; init; }

        /* ---------- Konstruktor ---------- */
        public Vector3Int(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /* ---------- Hilfs‑Eigenschaften ---------- */
        public int   LengthSquared => X * X + Y * Y + Z * Z;
        public float Length        => MathF.Sqrt(LengthSquared);

        /* ---------- Konvertierungen ---------- */
        public Vector3 ToVector3() => new(X, Y, Z);

        public void Deconstruct(out int x, out int y, out int z)
        {
            x = X; y = Y; z = Z;
        }

        /* ---------- Equality ---------- */
        public bool Equals(Vector3Int other)
            => X == other.X && Y == other.Y && Z == other.Z;

        public override bool Equals(object? obj)
            => obj is Vector3Int other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(X, Y, Z);

        public static bool operator ==(Vector3Int a, Vector3Int b) => a.Equals(b);
        public static bool operator !=(Vector3Int a, Vector3Int b) => !a.Equals(b);

        /* ---------- Arithmetik ---------- */
        public static Vector3Int operator +(Vector3Int a, Vector3Int b)
            => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Vector3Int operator -(Vector3Int a, Vector3Int b)
            => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        /* ---------- Darstellung ---------- */
        public override string ToString()
            => string.Create(CultureInfo.InvariantCulture,
                $"{nameof(Vector3Int)}({X}, {Y}, {Z})");
    }
}
