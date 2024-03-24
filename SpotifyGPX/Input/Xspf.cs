// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Input;

public partial class Xspf : SongInputBase, IHashVerifier
{
    private XDocument Document { get; }
    protected override List<SpotifyEntry> AllSongs { get; }

    /// <summary>
    /// Creates a new input handler for handling files in the XSPF format.
    /// </summary>
    /// <param name="path">The path to the XSPF file.</param>
    /// <exception cref="Exception">The file contains no valid elements.</exception>
    public Xspf(string path)
    {
        Document = XDocument.Load(path);

        if (SourceSongCount == 0)
        {
            // If there are no tracks in the GPX, throw error
            throw new Exception($"No track elements found in '{Path.GetFileName(path)}'!");
        }

        AllSongs = ParseSongs();
    }

    /// <summary>
    /// Parse the contents of the XSPF file to a list of songs.
    /// </summary>
    /// <returns>A list of SpotifyEntry objects, each representing a song record.</returns>
    private List<SpotifyEntry> ParseSongs()
    {
        return Document.Descendants(InputNs + Track).Select((element, index) => new SpotifyEntry(
            index,
            DateTimeOffset.ParseExact(element.Element(InputNs + "annotation")?.Value, Options.ISO8601UTC, null, TimeStyle),
            null,
            null,
            double.Parse(element.Element(InputNs + "duration")?.Value),
            null,
            null,
            null,
            element.Element(InputNs + "title")?.Value,
            element.Element(InputNs + "creator")?.Value,
            null,
            element.Element(InputNs + "link")?.Value,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null)
            ).ToList();
    }

    /// <summary>
    /// The total number of songs in the XSPF file.
    /// </summary>
    public override int SourceSongCount => Document.Descendants(InputNs + Track).Count();

    /// <summary>
    /// Verifies the hash included in the file with the contents of the file.
    /// </summary>
    /// <returns>True, if the hashes match. Otherwise, false.</returns>
    public bool VerifyHash()
    {
        XmlHashProvider hasher = new();
        string? expectedHash = Document.Descendants(InputNs + "identifier").FirstOrDefault()?.Value;
        return hasher.VerifyHash(Document.Descendants(InputNs + Track), expectedHash);
    }
}
