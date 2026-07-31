namespace DvdLib.Decrypt;

[Flags]
public enum DvdCssReadFlags
{
    None = 0,
    Decrypt = 1 << 0
}