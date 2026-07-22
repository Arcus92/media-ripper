namespace MediaLib.FFmpeg;

public readonly struct StreamId : IEquatable<StreamId>
{
    /// <summary>
    /// Gets the id value.
    /// </summary>
    public int Id { get; }
    
    /// <summary>
    /// Gets the id type.
    /// </summary>
    public StreamIdType Type { get; }
    
    /// <summary>
    /// Creates a stream identifier.
    /// </summary>
    /// <param name="id">The id value.</param>
    /// <param name="type">The id type.</param>
    public StreamId(int id, StreamIdType type)
    {
        Id = id;
        Type = type;
    }

    /// <summary>
    /// Returns a stream id from an index.
    /// </summary>
    /// <param name="index">The stream index.</param>
    /// <returns>Returns the stream id.</returns>
    public static StreamId Index(int index) => new(index, StreamIdType.Index);
    
    /// <summary>
    /// Returns a stream id from a pid.
    /// </summary>
    /// <param name="pid">The stream pid.</param>
    /// <returns>Returns the stream id.</returns>
    public static StreamId Pid(int pid) => new(pid, StreamIdType.Pid);

    /// <inheritdoc />
    public override string ToString()
    {
        if (Type == StreamIdType.Pid)
        {
            return $"0x{Id:X}";
        }
        
        return Id.ToString();
    }
    
    /// <inheritdoc />
    public bool Equals(StreamId other)
    {
        return Id == other.Id && Type == other.Type;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is StreamId other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, (int)Type);
    }

    public static bool operator ==(StreamId left, StreamId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(StreamId left, StreamId right)
    {
        return !left.Equals(right);
    }
    
    public static implicit operator StreamId(int id)
    {
        return Index(id);
    }
}