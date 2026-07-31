namespace MediaLib.Utils.IO;

/// <summary>
///     A readable instance of <see cref="BigEndianBinaryReader" />.
/// </summary>
public interface IBigEndianBinaryReadable
{
    /// <summary>
    ///     Reads the data with a <see cref="BigEndianBinaryReader" />.
    /// </summary>
    /// <param name="reader">The reader.</param>
    void Read(BigEndianBinaryReader reader);
}