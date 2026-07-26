using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

public struct UserOps : IBigEndianBinaryReadable
{
    public UserOps()
    {
    }
    
    /// <inheritdoc />
    public void Read(BigEndianBinaryReader reader)
    {
        var b = reader.ReadBits32();
        // TODO
    }
}