// SpotifyGPX by Simon Field

using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Input;

/// <summary>
/// The base class for all classes supporting the parsing of GPS journey files. All classes that handle GPS journey files must inherit this class.
/// </summary>
public abstract class GpsInputBase : IGpsInput
{
    protected abstract List<GPXTrack> Tracks { get; } // All GPS tracks in the file
    public abstract int SourceTrackCount { get; } // Number of tracks in the source file
    public int ParsedTrackCount => Tracks.Count; // Number of GPS tracks parsed from the file
    public abstract int SourcePointCount { get; } // Number of points in the source file
    public int ParsedPointCount => Tracks.Select(track => track.Points.Count).Sum(); // Number of points parsed from the file
    public List<GPXTrack> GetAllTracks() => Tracks;
}
