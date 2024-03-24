// SpotifyGPX by Simon Field

using System.Collections.Generic;

namespace SpotifyGPX.Input;

/// <summary>
/// The base class for all classes supporting the parsing song playback records files. All classes that handle song playback records files must inherit this class.
/// </summary>
public abstract class SongInputBase : ISongInput
{
    protected abstract List<SpotifyEntry> AllSongs { get; } // All songs in the file
    public abstract int SourceSongCount { get; } // Number of songs in the source file
    public int ParsedSongCount => AllSongs.Count; // Number of songs parsed from the file
    public List<SpotifyEntry> GetAllSongs() => AllSongs;
}
