namespace MediaLib.Utils.IO;

/// <summary>
/// Utility class to get mounting information.
/// </summary>
public static partial class MountUtils
{
    /// <summary>
    /// Gets the source of a mounted path.
    /// On Linux this will return the device pipe in /dev from a mounted CD / DVD / Blu-ray drive.
    /// </summary>
    /// <param name="path">The mounted path.</param>
    /// <returns>Returns the mounted source or <paramref name="path"/> if no mounted source was found.</returns>
    public static string GetMountSource(string path)
    {
        return GetMountSourceAsync(path).GetAwaiter().GetResult();
    }
    
    /// <summary>
    /// Gets the source of a mounted path.
    /// On Linux this will return the device pipe in /dev from a mounted CD / DVD / Blu-ray drive.
    /// </summary>
    /// <param name="path">The mounted path.</param>
    /// <returns>Returns the mounted source or <paramref name="path"/> if no mounted source was found.</returns>
    public static async Task<string> GetMountSourceAsync(string path)
    {
        if (OperatingSystem.IsLinux())
        {
            return await GetMountSourceLinuxAsync(path).ConfigureAwait(false);
        }
        
        return path;
    }
}