// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Output;

public partial class Csv : TxtSaveable
{
    protected override string?[] Document { get; }

    public Csv(IEnumerable<SongPoint> pairs) => Document = GetDocument(pairs);

    private static string[] GetDocument(IEnumerable<SongPoint> pairs)
    {
        string[] header = // The header line for the CSV file
        {
            "Title",
            "Artist",
            "Latitude",
            "Longitude",
            "Song Time",
            "Point Time"
        };

        // Convert the header to a CSV line
        string headerString = string.Join(Delimiter, header.Select(heading => $"\"{heading}\""));

        string[] csv = pairs.Select(pair => // Convert each pair to a CSV line
        {
            string?[] columns =
            {
                pair.Song.Song_Name,
                pair.Song.Song_Artist,
                pair.Point.Location.Latitude.ToString(),
                pair.Point.Location.Longitude.ToString(),
                pair.Song.Time.ToString(),
                pair.Point.Time.ToString(),
            };

            return string.Join(Delimiter, columns.Select(column => $"\"{column}\""));
        }).ToArray();

        string[] csvWithHeader = new string[csv.Length + 1]; // Add one for the header line
        csvWithHeader[0] = headerString; // Add the header line as the first element
        Array.Copy(csv, 0, csvWithHeader, 1, csv.Length); // Copy the existing CSV lines after the header

        return csvWithHeader;
    }

    public override int Count => Document.Length - 1; // Subtract one for the header line
}
