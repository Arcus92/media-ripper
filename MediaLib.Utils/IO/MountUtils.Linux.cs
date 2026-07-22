using System.Text;

namespace MediaLib.Utils.IO;

public static partial class MountUtils
{
    /// <inheritdoc cref="GetMountSourceAsync" />
    private static async Task<string> GetMountSourceLinuxAsync(string path)
    {
        path = Path.GetFullPath(path);

        await foreach (var (source, target) in GetMountPointsLinuxAsync().ConfigureAwait(false))
        {
            if (target == path) return source;
        }
        
        return path;
    }
    
    
    /// <summary>
    /// Returns all mount points on this Linux system as source-target-pair.
    /// </summary>
    /// <returns>Returns all mount points as source-target-pair.</returns>
    private static async IAsyncEnumerable<KeyValuePair<string, string>> GetMountPointsLinuxAsync()
    {
        if (!OperatingSystem.IsLinux())
        {
            yield break;
        }
        
        const string procMountsPath = "/proc/mounts";
        foreach (var line in await File.ReadAllLinesAsync(procMountsPath).ConfigureAwait(false))
        {
            var sourceEnd = line.IndexOf(' ');
            if (sourceEnd < 0) continue;
            
            var sourceOctal = line.AsSpan(0, sourceEnd);
            
            var targetEnd = line.IndexOf(' ', sourceEnd + 1);
            if (targetEnd < 0) continue;
            
            var targetOctal = line.AsSpan(sourceEnd + 1, targetEnd - sourceEnd - 1);
            
            var source = UnescapeOctal(sourceOctal);
            var target = UnescapeOctal(targetOctal);
            
            yield return new KeyValuePair<string, string>(source, target);
        }
    }

    private static string UnescapeOctal(ReadOnlySpan<char> text)
    {
        if (!text.Contains('\\'))
        {
            return text.ToString();
        }
        
        var builder = new StringBuilder(text.Length);

        // Escape \OOO
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (c == '\\' && i + 3 <  text.Length)
            {
                var octalText = text.Slice(i + 1, 3).ToString();
                var charCode = Convert.ToInt32(octalText, 8);
                builder.Append((char)charCode);
                i += 3;
            }
            
            builder.Append(c);
        }
        
        return builder.ToString();
    }
}