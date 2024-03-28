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

    public override int SourceSongCount => Document.Descendants(InputNs + Track).Count();

    public bool VerifyHash()
    {
        XmlHashProvider hasher = new();
        string? expectedHash = Document.Descendants(InputNs + "identifier").FirstOrDefault()?.Value;
        return hasher.VerifyHash(Document.Descendants(InputNs + Track), expectedHash);
    }
}
