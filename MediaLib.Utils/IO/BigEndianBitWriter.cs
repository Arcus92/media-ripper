using System.Numerics;

namespace MediaLib.Utils.IO;

/// <summary>
///     A big-endian bit writer. Starts at the left bit.
/// </summary>
/// <typeparam name="T">The base integer value.</typeparam>
public unsafe ref struct BigEndianBitWriter<T> where T : unmanaged, IBinaryInteger<T>
{
    /// <summary>
    ///     The binary value.
    /// </summary>
    private T _value;

    /// <summary>
    ///     The current bit position. Runs backwards for big endian.
    /// </summary>
    private int _position = Size - 1;

    public BigEndianBitWriter()
    {
        _value = default;
    }

    /// <summary>
    ///     Gets the number of total bits of <see cref="T" />.
    /// </summary>
    private static readonly int Size = sizeof(T) * 8;

    /// <summary>
    ///     Gets the number of available bits.
    /// </summary>
    public int Available => _position;

    /// <summary>
    ///     Writes the next bit.
    /// </summary>
    public void WriteBit(bool value)
    {
        if (value) _value |= T.One << _position;

        _position--;
    }

    /// <summary>
    ///     Writes the next bits.
    /// </summary>
    /// <param name="count">The number of bits to write.</param>
    /// <param name="value">The value to set.</param>
    public void ReadBits(int count, T value)
    {
        value = (value << (Size - count)) >> (Size - _position - 1);
        _position -= count;
        _value |= value;
    }

    /// <summary>
    ///     Skips the number of bits.
    /// </summary>
    /// <param name="count">The number of bits to skip.</param>
    public void Skip(int count)
    {
        _position -= count;
    }
}