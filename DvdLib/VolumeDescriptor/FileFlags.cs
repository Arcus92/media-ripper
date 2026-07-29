namespace DvdLib.VolumeDescriptor;

public enum FileFlags : byte
{
    Hidden = 1 << 0,
    Directory = 1 << 1,
    AssociatedFile = 1 << 2,
    HasFormat = 1 << 3,
    HasPermission = 1 << 4,
    Spanning = 1 << 7
}