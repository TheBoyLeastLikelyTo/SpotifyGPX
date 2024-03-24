// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Input;

/// <summary>
/// Provides instructions for parsing GPS data from the GPX format.
/// </summary>
public partial class Gpx : GpsInputBase
{
    private XDocument Document { get; } // Entire input GPX document
    protected override List<GPXTrack> Tracks { get; } // Parsed tracks from GPX document

    /// <summary>
    /// Creates a new input handler for handling files in the GPX format.
    /// </summary>
    /// <param name="path">The path to the GPX file.</param>
    /// <exception cref="Exception">No tracks and/or points were found in the given file.</exception>
    public Gpx(string path)
    {
        Document = XDocument.Load(path);

        Tracks = ParseTracks();
    }

    /// <summary>
    /// The total number of track elements in this GPX file.
    /// </summary>
    public override int SourceTrackCount => Document.Descendants(InputNs + Track).Count();

    /// <summary>
    /// The total number of point elements in this GPX file.
    /// </summary>
    public override int SourcePointCount => Document.Descendants(InputNs + TrackPoint).Count();

    /// <summary>
    /// Parses this GPX document into a readable list of tracks.
    /// </summary>
    /// <returns>A list of GPXTrack objects.</returns>
    /// <exception cref="Exception">An element (latitude, longitude, or time) of this point was null.</exception>
    private List<GPXTrack> ParseTracks()
    {
        return Document.Descendants(InputNs + Track)
            .Select((trk, index) => new GPXTrack( // For each track and its index, create a new GPXTrack
                index,
                trk.Element(InputNs + "name")?.Value,
                TrackType.GPX,
                trk.Descendants(InputNs + TrackPoint)
                    .Select((trkpt, pointIndex) => new GPXPoint( // For each point and its index, create a new GPXPoint
                        pointIndex,
                        new Coordinate( // Parse its coordinate
                            double.Parse(trkpt.Attribute("lat")?.Value ?? throw new Exception($"GPX 'lat' cannot be null, check your GPX")),
                            double.Parse(trkpt.Attribute("lon")?.Value ?? throw new Exception($"GPX 'lon' cannot be null, check your GPX"))
                        ),
                        DateTimeOffset.ParseExact(trkpt.Element(InputNs + "time")?.Value ?? throw new Exception($"GPX 'time' cannot be null, check your GPX"), TimeFormat, null, TimeStyle)
                    )).ToList() // Send all points to List<GPXPoint>
            ))
            .ToList(); // Send all tracks to List<GPXTrack>
    }
}
