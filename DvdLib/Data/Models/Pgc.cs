using MediaLib.Utils.IO;

namespace DvdLib.Data.Models;

/// <summary>
/// Program Chain Information
/// </summary>
public class Pgc
{
    public DvdTime PlaybackTime { get; private set; } = default;
    public UserOps ProhibitedOps { get; private set; } = default;
    public ushort[] AudioControl { get; private set; } = [];
    public uint[] SubpControl { get; private set; } = [];
    public ushort NextPgcNr { get; private set; } = 0;
    public ushort PrevPgcNr { get; private set; } = 0;
    public ushort GroupPgcNr { get; private set; } = 0;
    public byte PgPlaybackMode { get; private set; } = 0;
    public byte StillTime { get; private set; } = 0;
    public uint[] Palette { get; private set; } = [];
    public ushort CommandTblOffset { get; private set; } = 0;
    public ushort ProgramMapOffset { get; private set; } = 0;
    public ushort CellPlaybackOffset { get; private set; } = 0;
    public ushort CellPositionOffset { get; private set; } = 0;

    public CellPlayback[] CellPlayback { get; private set; } = [];

    private void Read(BigEndianBinaryReader reader)
    {
        var start = reader.Position;
        
        reader.Skip(2);
        var nrOfPrograms = reader.ReadByte();
        var nrOfCells = reader.ReadByte();
        PlaybackTime = DvdTime.FromReader(reader);
        ProhibitedOps = UserOps.FromReader(reader);
        AudioControl = reader.ReadUInt16Array(8);
        SubpControl = reader.ReadUInt32Array(32);
        NextPgcNr = reader.ReadUInt16();
        PrevPgcNr = reader.ReadUInt16();
        GroupPgcNr = reader.ReadUInt16();
        PgPlaybackMode = reader.ReadByte();
        StillTime = reader.ReadByte();
        Palette = reader.ReadUInt32Array(16);
        CommandTblOffset = reader.ReadUInt16();
        ProgramMapOffset = reader.ReadUInt16();
        CellPlaybackOffset = reader.ReadUInt16();
        CellPositionOffset = reader.ReadUInt16();

        if (CellPlaybackOffset != 0 && nrOfCells > 0)
        {
            reader.SeekTo(start + CellPlaybackOffset);
            CellPlayback = Models.CellPlayback.FromReader(reader, nrOfCells);
        }
    }

    public static Pgc FromReader(BigEndianBinaryReader reader)
    {
        var data = new Pgc();
        data.Read(reader);
        return data;
    }
}