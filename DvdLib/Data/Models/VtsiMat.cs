using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
///     Video Title Set Information Management Table
/// </summary>
public class VtsiMat : IBigEndianBinaryReadable
{
    public uint VtsLastSector { get; private set; }
    public uint VtsiLastSector { get; private set; }
    public byte SpecificationVersion { get; private set; }
    public uint VtsCategory { get; private set; }
    public uint VtsiLastByte { get; private set; }

    public uint VtsmVobs { get; private set; }
    public uint VtsTtVobs { get; private set; }
    public uint VtsPttSrpt { get; private set; }
    public uint VtsPgcit { get; private set; }

    public uint VtsmPgciUt { get; private set; }
    public uint VtsTmapt { get; private set; }
    public uint VtsmCAdt { get; private set; }
    public uint VtsmVobuAdmap { get; private set; }
    public uint VtsCAdt { get; private set; }
    public uint VtsVobuAdmap { get; private set; }

    public VideoAttributes VtsmVideo { get; private set; }
    public AudioAttributes[] VtsmAudios { get; private set; } = [];
    public SubPictureAttributes[] VtsmSubPictures { get; private set; } = [];

    public VideoAttributes VtsVideo { get; private set; }
    public AudioAttributes[] VtsAudios { get; private set; } = [];
    public SubPictureAttributes[] VtsSubPictures { get; private set; } = [];
    public MultiChannelAttributes[] VtsiMultiChannelAudios { get; private set; } = [];

    /// <inheritdoc />
    public void Read(BigEndianBinaryReader reader)
    {
        VtsLastSector = reader.ReadUInt32();
        reader.ReadZero(12);
        VtsiLastSector = reader.ReadUInt32();
        reader.ReadZero();
        SpecificationVersion = reader.ReadByte();
        VtsCategory = reader.ReadUInt32();
        reader.ReadZero(2);
        reader.ReadZero(2);
        reader.ReadZero();
        reader.ReadZero(19);
        reader.ReadZero(2);
        reader.ReadZero(32);
        reader.ReadZero(8);
        reader.ReadZero(24);
        VtsiLastByte = reader.ReadUInt32();
        reader.ReadZero(4);
        reader.ReadZero(56);
        VtsmVobs = reader.ReadUInt32();
        VtsTtVobs = reader.ReadUInt32();
        VtsPttSrpt = reader.ReadUInt32();
        VtsPgcit = reader.ReadUInt32();
        VtsmPgciUt = reader.ReadUInt32();
        VtsTmapt = reader.ReadUInt32();
        VtsmCAdt = reader.ReadUInt32();
        VtsmVobuAdmap = reader.ReadUInt32();
        VtsCAdt = reader.ReadUInt32();
        VtsVobuAdmap = reader.ReadUInt32();
        reader.ReadZero(24);

        VtsmVideo = reader.Read<VideoAttributes>();
        reader.ReadZero();
        var nrOfVtsmAudioStreams = reader.ReadByte();
        var vtsmAudioAttr = reader.Read<AudioAttributes>(8);
        VtsmAudios = vtsmAudioAttr.AsSpan(0, nrOfVtsmAudioStreams).ToArray();
        reader.ReadZero(17);
        var nrOfVtsmSubpStreams = reader.ReadByte();
        var vtsmSubpAttr = reader.Read<SubPictureAttributes>(28);
        VtsmSubPictures = vtsmSubpAttr.AsSpan(0, nrOfVtsmSubpStreams).ToArray();
        reader.ReadZero(2);

        VtsVideo = reader.Read<VideoAttributes>();
        reader.ReadZero();
        var nrOfVtsAudioAttr = reader.ReadByte();
        var vtsAudioAttr = reader.Read<AudioAttributes>(8);
        VtsAudios = vtsAudioAttr.AsSpan(0, nrOfVtsAudioAttr).ToArray();
        reader.Skip(17);
        var nrOfVtsSubPictureStreams = reader.ReadByte();
        var vtsSubPictureAttr = reader.Read<SubPictureAttributes>(32);
        VtsSubPictures = vtsSubPictureAttr.AsSpan(0, nrOfVtsSubPictureStreams).ToArray();
        reader.Skip(2);
        var vtsMuAudioAttr = reader.Read<MultiChannelAttributes>(8);
        VtsiMultiChannelAudios = vtsMuAudioAttr;
    }
}