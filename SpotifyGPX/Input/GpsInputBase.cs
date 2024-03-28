// SpotifyGPX by Simon Field

using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Input;

/// <summary>
/// The base class for all classes supporting the parsing of GPS journey files. All classes that handle GPS journey files must inherit this class.
/// </summary>
public abstract class GpsInputBase : IGpsInput
{
    protected abstract List<GPXTrack> Tracks { get; }
    public abstract int SourceTrackCount { get; }
    public int ParsedTrackCount => Tracks.Count;
    public abstract int SourcePointCount { get; }
    public int ParsedPointCount => Tracks.Select(track => track.Points.Count).Sum();
    public List<GPXTrack> GetAllTracks() => Tracks;
}
