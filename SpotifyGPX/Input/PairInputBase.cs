// SpotifyGPX by Simon Field

using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Input;

/// <summary>
/// The base class for all classes supporting the parsing of song-point pairing files. All classes that handle song-point pairing files must inherit this class.
/// </summary>
public abstract class PairInputBase : ISongInput, IGpsInput, IPairInput
{
    protected abstract List<SongPoint> AllPairs { get; } // All pairs in the file
    public abstract int SourcePairCount { get; }
    public int ParsedPairCount => AllPairs.Count;
    public abstract int SourceTrackCount { get; }
    public int ParsedTrackCount => AllPairs.GroupBy(pair => pair.Origin).Count();
    public abstract int SourcePointCount { get; }
    public int ParsedPointCount => AllPairs.Select(pair => pair.Point).Count();
    public abstract int SourceSongCount { get; }
    public int ParsedSongCount => AllPairs.Select(pair => pair.Song).Count();
    public List<SongPoint> GetAllPairs() => AllPairs;
    public List<SpotifyEntry> GetAllSongs() => AllPairs.Select(pair => pair.Song).ToList();
    public List<GPXTrack> GetAllTracks() => AllPairs.GroupBy(pair => pair.Origin).Select(type => new GPXTrack(type.Key.Index, type.Key.Name, type.Key.Type, type.Select(pair => pair.Point).ToList())).ToList();
}
