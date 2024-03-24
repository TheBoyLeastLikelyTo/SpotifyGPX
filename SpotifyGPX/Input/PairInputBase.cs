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
    public abstract int SourcePairCount { get; } // Number of pairs in the source file
    public int ParsedPairCount => AllPairs.Count; // Number of pairs parsed from the file
    public abstract int SourceTrackCount { get; } // Number of tracks in the source file
    public int ParsedTrackCount => AllPairs.GroupBy(pair => pair.Origin).Count(); // Number of tracks parsed from the file
    public abstract int SourcePointCount { get; } // Number of points in the source file
    public int ParsedPointCount => AllPairs.Select(pair => pair.Point).Count(); // Number of points parsed from the file
    public abstract int SourceSongCount { get; } // Number of songs in the source file
    public int ParsedSongCount => AllPairs.Select(pair => pair.Song).Count(); // Number of songs parsed from the file
    public List<SongPoint> GetAllPairs() => AllPairs;
    public List<SpotifyEntry> GetAllSongs() => AllPairs.Select(pair => pair.Song).ToList();
    public List<GPXTrack> GetAllTracks() => AllPairs.GroupBy(pair => pair.Origin).Select(type => new GPXTrack(type.Key.Index, type.Key.Name, type.Key.Type, type.Select(pair => pair.Point).ToList())).ToList();
}
