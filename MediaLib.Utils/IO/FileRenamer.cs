namespace MediaLib.Utils.IO;

/// <summary>
/// A helper class to rename multiple files and avoiding collisions.
/// </summary>
public static class FileRenamer
{
    private record struct RenameStrategyEntry(string Filename, string NewFilename);

    private class RenameStrategy : List<RenameStrategyEntry>;

    /// <summary>
    /// Tries to rename the given files while resolving file name collisions.
    /// </summary>
    /// <param name="renameMap">The map of files to rename.</param>
    /// <returns>Returns true, if the files could be renamed.</returns>
    public static bool TryRename(Dictionary<string, string> renameMap)
    {
        if (!TryGetRenameStrategy(renameMap, out var strategy))
        {
            return false;
        }

        foreach (var entry in strategy)
        {
            File.Move(entry.Filename, entry.NewFilename);
        }

        return true;
    }
    
    /// <summary>
    /// Rename the given files while resolving file name collisions.
    /// </summary>
    /// <param name="renameMap">The map of files to rename.</param>
    /// <exception cref="IOException">Throws an exception if the files couldn't be renamed.</exception>
    public static void Rename(Dictionary<string, string> renameMap)
    {
        if (!TryRename(renameMap))
        {
            throw new IOException("Unable to rename files: Invalid rename strategy.");
        }
    }
    
    private static bool TryGetRenameStrategy(Dictionary<string, string> input, out RenameStrategy strategy)
    {
        // Normalize filenames
        var normalizedInput = new Dictionary<string, string>();
        foreach (var pair in input)
        {
            var filename = Path.GetFullPath(pair.Key);
            var newFilename = Path.GetFullPath(pair.Value);
            if (filename == newFilename) continue;
            normalizedInput.Add(filename, newFilename);
        }

        
        strategy = [];
        var cleanupStrategy = new RenameStrategy();
        foreach (var (filename, newFilename) in normalizedInput)
        {
            // Check for no conflicts.
            if (!File.Exists(newFilename))
            {
                strategy.Add(new RenameStrategyEntry(filename, newFilename));   
                continue;
            }
            
            // Allow it, if the output filename is also renamed.
            if (!normalizedInput.ContainsKey(newFilename))
            {
                return false;
            }
            
            // Create a temporary filename
            var name = Path.GetFileName(newFilename);
            var directory = Path.GetDirectoryName(newFilename) ?? "";
            var tempName = $"{name}_{Guid.NewGuid().ToString()}";
            var tempFilename = Path.Combine(directory, tempName);
            
            // First: Rename to temp file
            strategy.Add(new RenameStrategyEntry(filename, tempFilename));
            
            // Then: Rename to final name
            cleanupStrategy.Add(new RenameStrategyEntry(tempFilename, newFilename));
        }

        // Add the cleanup step to rename all temp files.
        strategy.AddRange(cleanupStrategy);
        
        return true;
    }
}