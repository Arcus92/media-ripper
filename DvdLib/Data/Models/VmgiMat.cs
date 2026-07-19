using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
/// Video Manager Information Management Table
/// </summary>
public class VmgiMat
{
    public uint VmgLastSector { get; private set; } = 0;
    public uint VmgiLastSector { get; private set; } = 0;
    public byte SpecificationVersion { get; private set; } = 0;
    public uint VmgCategory { get; private set; } = 0;
    public ushort VmgNrOfVolumes { get; private set; } = 0;
    public ushort VmgThisVolumeNr { get; private set; } = 0;
    public byte DiscSide { get; private set; } = 0;
    public ushort VmgNrOfTitleSets { get; private set; } = 0;
    public string ProviderIdentifier { get; private set; } = "";
    public ulong VmgPosCode { get; private set; } = 0;
    public uint VmgiLastByte { get; private set; } = 0;
    public uint FirstPlayPgc { get; private set; } = 0;
    
    public uint TtSrpt { get; private set; } = 0;
    
    public VideoAttributes VmgmVideo { get; private set; } = default;
    public AudioAttributes[] VmgmAudios { get; private set; } = [];
    public SubPictureAttributes[] VmgmSubPictures { get; private set; } = [];

    private void Read(BigEndianBinaryReader reader)
    {
        VmgLastSector = reader.ReadUInt32();
        reader.ReadZero(12);
        VmgiLastSector = reader.ReadUInt32();
        reader.ReadZero();
        SpecificationVersion = reader.ReadByte();
        VmgCategory = reader.ReadUInt32();
        VmgNrOfVolumes = reader.ReadUInt16();
        VmgThisVolumeNr = reader.ReadUInt16();
        DiscSide = reader.ReadByte();
        reader.ReadZero(19);
        VmgNrOfTitleSets = reader.ReadUInt16();
        ProviderIdentifier = reader.ReadString(32);
        VmgPosCode = reader.ReadUInt64();
        reader.ReadZero(24);
        VmgiLastByte = reader.ReadUInt32();
        FirstPlayPgc = reader.ReadUInt32();
        reader.ReadZero(56);
        
        // Sector
        var vmgmVobs = reader.ReadUInt32();
        TtSrpt = reader.ReadUInt32();
        var vmgmPgciUt = reader.ReadUInt32();
        var ptlMait = reader.ReadUInt32();
        var vtsAtrt = reader.ReadUInt32();
        var txtdtMgi = reader.ReadUInt32();
        var vmgmCAdt = reader.ReadUInt32();
        var vmgmVobuAdmap = reader.ReadUInt32();
        reader.ReadZero(32);
        
        VmgmVideo = VideoAttributes.FromReader(reader);
        reader.ReadZero();
        var nrOfVmgmAudioStreams = reader.ReadByte();
        var vmgmAudioAttr = AudioAttributes.FromReader(reader, 8);
        VmgmAudios = vmgmAudioAttr.AsSpan(0, nrOfVmgmAudioStreams).ToArray();
        reader.ReadZero(17);
        var nrOfVmgmSubpStreams = reader.ReadByte();
        var vmgmSubpAttr = SubPictureAttributes.FromReader(reader, 28);
        VmgmSubPictures = vmgmSubpAttr.AsSpan(0, nrOfVmgmSubpStreams).ToArray();
    }
    
    public static VmgiMat FromReader(BigEndianBinaryReader reader)
    {
        var data = new VmgiMat();
        data.Read(reader);
        return data;
    }
}