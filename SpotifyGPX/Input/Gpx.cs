// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Input;

public partial class Gpx : GpsInputBase
{
    private XDocument Document { get; }
    protected override List<GPXTrack> Tracks { get; }

    public Gpx(string path)
    {
        Document = XDocument.Load(path);

        Tracks = ParseTracks();
    }

    public override int SourceTrackCount => Document.Descendants(InputNs + Track).Count();

    public override int SourcePointCount => Document.Descendants(InputNs + TrackPoint).Count();

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
