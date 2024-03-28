// SpotifyGPX by Simon Field

using System.Collections.Generic;

namespace SpotifyGPX.Input;

/// <summary>
/// The base class for all classes supporting the parsing song playback records files. All classes that handle song playback records files must inherit this class.
/// </summary>
public abstract class SongInputBase : ISongInput
{
    protected abstract List<SpotifyEntry> AllSongs { get; }
    public abstract int SourceSongCount { get; }
    public int ParsedSongCount => AllSongs.Count;
    public List<SpotifyEntry> GetAllSongs() => AllSongs;
}
