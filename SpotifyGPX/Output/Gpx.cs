// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Output;

public partial class Gpx : XmlSaveable
{
    protected override XDocument Document { get; }

    public Gpx(IEnumerable<SongPoint> pairs, string trackName) => Document = GetDocument(pairs, trackName);

    private static XDocument GetDocument(IEnumerable<SongPoint> pairs, string trackName)
    {
        var gpxPairs = pairs.Select(pair =>
            new XElement(Namespace + Waypoint,
                new XAttribute("lat", pair.Point.Location.Latitude),
                new XAttribute("lon", pair.Point.Location.Longitude),
                new XElement(Namespace + "name", pair.Song.ToString()),
                new XElement(Namespace + "time", pair.Point.Time.UtcDateTime.ToString(Options.ISO8601UTC)),
                new XElement(Namespace + "desc", pair.Description)
            )
        );

        XmlHashProvider hasher = new();
        string hash = hasher.ComputeHash(gpxPairs);

        return new XDocument(
            new XDeclaration("1.0", DocumentEncoding, null),
            new XElement(Namespace + "gpx",
                new XAttribute("version", "1.0"),
                new XAttribute("creator", "SpotifyGPX"),
                new XAttribute(XNamespace.Xmlns + "xsi", Xsi),
                new XAttribute("xmlns", Namespace),
                new XAttribute(Xsi + "schemaLocation", Schema),
                new XElement(Namespace + "name", trackName),
                new XElement(Namespace + "desc", hash),
                new XElement(Namespace + "time", DateTimeOffset.Now.UtcDateTime.ToString(Options.ISO8601UTC)),
                gpxPairs
            )
        );
    }

    public override int Count => Document.Descendants(Namespace + Waypoint).Count();
}
