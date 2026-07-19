using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

public struct UserOps
{
    public UserOps()
    {
    }
    
    
    
    private void Read(BigEndianBinaryReader reader)
    {
        var b = reader.ReadBits32();
        // TODO
    }

    public static UserOps FromReader(BigEndianBinaryReader reader)
    {
        var data = new UserOps();
        data.Read(reader);
        return data;
    }
}