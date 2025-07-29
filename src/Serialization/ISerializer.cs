namespace HyatlasGame.Core.Serialization
{
    /// <summary>
    /// Generische Schnittstelle für die Serialisierung und Deserialisierung von Objekten.
    /// </summary>
    /// <typeparam name="T">Typ, der serialisiert/deserialisiert wird.</typeparam>
    public interface ISerializer<T>
    {
        /// <summary>
        /// Serialisiert das Objekt in einen Bytestream.
        /// </summary>
        byte[] Serialize(T obj);

        /// <summary>
        /// Deserialisiert das Objekt aus einem Bytestream.
        /// </summary>
        T Deserialize(byte[] data);
    }
}
