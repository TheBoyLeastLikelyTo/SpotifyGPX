// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Output;

public partial class Xspf : XmlSaveable
{
    protected override XDocument Document { get; }

    public Xspf(IEnumerable<SongPoint> pairs, string trackName) => Document = GetDocument(pairs, trackName);

    private static XDocument GetDocument(IEnumerable<SongPoint> pairs, string trackName)
    {
        var xspfPairs = pairs.Select(pair =>
        new XElement(Namespace + Track,
            new XElement(Namespace + "identifier", pair.Song.Song_ID),
            new XElement(Namespace + "title", pair.Song.Song_Name),
            new XElement(Namespace + "creator", pair.Song.Song_Artist),
            new XElement(Namespace + "annotation", pair.Song.Time.UtcDateTime.ToString(Options.ISO8601UTC)),
            new XElement(Namespace + "album", pair.Song.Song_Album),
            new XElement(Namespace + "duration", pair.Song.TimePlayed.TotalMilliseconds),
            new XElement(Namespace + "link", pair.Song.Song_URI)
        ));

        XmlHashProvider hasher = new();
        string hash = hasher.ComputeHash(xspfPairs);

        return new XDocument(
            new XDeclaration("1.0", DocumentEncoding, null),
            new XElement(Namespace + "playlist",
                new XAttribute("version", "1.0"),
                new XAttribute("xmlns", Namespace),
                new XElement(Namespace + "title", trackName),
                new XElement(Namespace + "creator", "SpotifyGPX"),
                new XElement(Namespace + "annotation", ""),
                new XElement(Namespace + "identifier", hash),
                new XElement(Namespace + "date", DateTimeOffset.Now.UtcDateTime.ToString(Options.ISO8601UTC)),
                new XElement(Namespace + "trackList", xspfPairs) // All pairs inside <trackList>
            )
        );
    }

    public override int Count => Document.Descendants(Namespace + Track).Count(); // Number of track elements
}
