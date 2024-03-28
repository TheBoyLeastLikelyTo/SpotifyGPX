// SpotifyGPX by Simon Field

namespace SpotifyGPX.Output;

/// <summary>
/// Interfaces with file output classes, unifying all formats that pairs can be written out as.
/// </summary>
public interface IFileOutput
{
    /// <summary>
    /// Save this file to the disk.
    /// </summary>
    /// <param name="path">The path on the disk where the file should be saved.</param>
    void Save(string path);
    
    /// <summary>
    /// The number of pairings in the outgoing document.
    /// </summary>
    int Count { get; } // Provides the number of pairings in the file
}
