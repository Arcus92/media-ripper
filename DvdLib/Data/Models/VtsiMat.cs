using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
/// Video Title Set Information Management Table
/// </summary>
public class VtsiMat
{
    public uint VtsLastSector { get; private set; } = 0;
    public uint VtsiLastSector { get; private set; } = 0;
    public byte SpecificationVersion { get; private set; } = 0;
    public uint VtsCategory { get; private set; } = 0;
    public uint VtsiLastByte { get; private set; } = 0;
    
    public uint VtsPttSrpt { get; private set; } = 0;
    public uint VtsPgcit { get; private set; } = 0;

    public VideoAttributes VtsmVideo { get; private set; }
    public AudioAttributes[] VtsmAudios { get; private set; } = [];
    public SubPictureAttributes[] VtsmSubPictures { get; private set; } = [];
    
    public VideoAttributes VtsVideo { get; private set; }
    public AudioAttributes[] VtsAudios { get; private set; } = [];
    public SubPictureAttributes[] VtsSubPictures { get; private set; } = [];
    public MultiChannelAttributes[] VtsiMultiChannelAudios { get; private set; } = [];

    
    private void Read(BigEndianBinaryReader reader)
    {
        VtsLastSector = reader.ReadUInt32();
        reader.ReadZero(12);
        VtsiLastSector = reader.ReadUInt32();
        reader.ReadZero();
        SpecificationVersion = reader.ReadByte();
        VtsCategory = reader.ReadUInt32();
        reader.ReadZero(2);
        reader.ReadZero(2);
        reader.ReadZero(1);
        reader.ReadZero(19);
        reader.ReadZero(2);
        reader.ReadZero(32);
        reader.ReadZero(8);
        reader.ReadZero(24);
        VtsiLastByte = reader.ReadUInt32();
        reader.ReadZero(4);
        reader.ReadZero(56);
        var vtsmVobs = reader.ReadUInt32();
        var vtsttVobs = reader.ReadUInt32();
        VtsPttSrpt = reader.ReadUInt32();
        VtsPgcit = reader.ReadUInt32();
        var vtsmPgciUt = reader.ReadUInt32();
        var vtsTmapt = reader.ReadUInt32();
        var vtsmCAdt = reader.ReadUInt32();
        var vtsmVobuAdmap = reader.ReadUInt32();
        var vtsCAdt = reader.ReadUInt32();
        var vtsVobuAdmap = reader.ReadUInt32();
        reader.ReadZero(24);
        
        VtsmVideo = VideoAttributes.FromReader(reader);
        reader.ReadZero();
        var nrOfVtsmAudioStreams = reader.ReadByte();
        var vtsmAudioAttr = AudioAttributes.FromReader(reader, 8);
        VtsmAudios = vtsmAudioAttr.AsSpan(0, nrOfVtsmAudioStreams).ToArray();
        reader.ReadZero(17);
        var nrOfVtsmSubpStreams = reader.ReadByte();
        var vtsmSubpAttr = SubPictureAttributes.FromReader(reader, 28);
        VtsmSubPictures = vtsmSubpAttr.AsSpan(0, nrOfVtsmSubpStreams).ToArray();
        reader.ReadZero(2);
        
        VtsVideo = VideoAttributes.FromReader(reader);
        reader.ReadZero();
        var nrOfVtsAudioAttr = reader.ReadByte();
        var vtsAudioAttr = AudioAttributes.FromReader(reader, 8);
        VtsAudios = vtsAudioAttr.AsSpan(0, nrOfVtsAudioAttr).ToArray();
        reader.Skip(17);
        var nrOfVtsSubPictureStreams = reader.ReadByte();
        var vtsSubPictureAttr = SubPictureAttributes.FromReader(reader, 32);
        VtsSubPictures = vtsSubPictureAttr.AsSpan(0, nrOfVtsSubPictureStreams).ToArray();
        reader.Skip(2);
        var vtsMuAudioAttr = MultiChannelAttributes.FromReader(reader, 8);
        VtsiMultiChannelAudios = vtsMuAudioAttr;
    }
    
    public static VtsiMat FromReader(BigEndianBinaryReader reader)
    {
        var data = new VtsiMat();
        data.Read(reader);
        return data;
    }
}