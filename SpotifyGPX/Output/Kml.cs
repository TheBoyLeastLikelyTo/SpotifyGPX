// SpotifyGPX by Simon Field

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Output;

/// <summary>
/// Provides instructions for exporting pairing data to the KMZ format.
/// </summary>
public partial class Kml : XmlSaveable
{
    protected override XDocument Document { get; }

    /// <summary>
    /// Creates a new output handler for handling files in the KMZ format.
    /// </summary>
    /// <param name="pairs">A list of pairs to be exported.</param>
    /// <param name="trackName">The name of the track representing the pairs.</param>
    public Kml(IEnumerable<SongPoint> pairs, string trackName)
    {
        Document = GetDocument(pairs, trackName);
    }

    /// <summary>
    /// Creates an XDocument containing each pair, in KML format.
    /// </summary>
    /// <param name="pairs">A list of pairs.</param>
    /// <param name="trackName">The name of the track representing the pairs.</param>
    /// <returns>An XDocument containing the contents of the created KML.</returns>
    private static XDocument GetDocument(IEnumerable<SongPoint> pairs, string trackName)
    {
        var kmlPairs = pairs.Select(pair =>
            new XElement(Namespace + Placemark,
                new XElement(Namespace + "name", pair.Song.ToString()),
                new XElement(Namespace + "description", pair.Description),
                new XElement(Namespace + "Point",
                    new XElement(Namespace + "coordinates", $"{pair.Point.Location.Longitude},{pair.Point.Location.Latitude}")
                ),
                new XElement(Namespace + "TimeStamp",
                    new XElement(Namespace + "when", pair.Point.Time.UtcDateTime.ToString(Options.ISO8601UTC))
                )
            )
        );

        XmlHashProvider hasher = new();
        string hash = hasher.ComputeHash(kmlPairs);

        return new XDocument(
            new XDeclaration("1.0", DocumentEncoding, null),
            new XElement(Namespace + "kml",
                new XAttribute("xmlns", Namespace),
                new XAttribute(XNamespace.Xmlns + "gx", Gx),
                new XElement(Namespace + "Document",
                    new XElement(Namespace + "name", trackName),
                    new XElement(Namespace + "description", hash),
                    kmlPairs
                )
            )
        );
    }

    /// <summary>
    /// The number of placemarks (pairs) within this KML file.
    /// </summary>
    public override int Count => Document.Descendants(Namespace + Placemark).Count();
}
