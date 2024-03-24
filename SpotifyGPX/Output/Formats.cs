// SpotifyGPX by Simon Field

namespace SpotifyGPX.Output;

/// <summary>
/// A list of the supported export formats.
/// </summary>
public enum Formats
{
    /// <summary>
    /// A CSV file containing a pair per line.
    /// </summary>
    Csv,

    /// <summary>
    /// A GPX file containing song-point pairs as waypoints.
    /// </summary>
    Gpx,

    /// <summary>
    /// A JSON file containing only the original Spotify data records used for pairs.
    /// </summary>
    Json,

    /// <summary>
    /// A JsonReport file containing all pairing and track data.
    /// </summary>
    JsonReport,

    /// <summary>
    /// A KML file containing song-point pairs as placemarks.
    /// </summary>
    Kml,

    /// <summary>
    /// A plain text (TXT) file containing a string per pair.
    /// </summary>
    Txt,

    /// <summary>
    /// An XSLX (Excel) workbook containing a table of pairs.
    /// </summary>
    Xlsx,

    /// <summary>
    /// An XML playlist file compatible with audio playback software.
    /// </summary>
    Xspf
}
