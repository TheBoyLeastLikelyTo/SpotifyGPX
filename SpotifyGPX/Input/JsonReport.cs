// SpotifyGPX by Simon Field

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Input;

/// <summary>
/// Provides instructions for parsing song playback data and GPS data from the JsonReport format.
/// </summary>
public partial class JsonReport : PairInputBase, IJsonDeserializer, IHashVerifier
{
    private JsonDeserializer JsonDeserializer { get; } // Deserializer for handling Json deserialization for hashing
    private List<JObject> JsonObjects { get; } // Everything in the JsonReport file
    private List<JObject> JsonTracksOnly { get; } // Only the tracks portion of the JsonReport file (excluding header, hash)
    protected override List<SongPoint> AllPairs { get; } // Pairs parsed from the JsonTracksOnly portion of the file

    /// <summary>
    /// Creates a JsonReport importer that allows job creation from existing job reports.
    /// </summary>
    /// <param name="path">The path to a JsonReport file.</param>
    public JsonReport(string path)
    {
        JsonDeserializer = new JsonDeserializer(path, JsonSettings);
        JsonObjects = Deserialize();
        JsonTracksOnly = JsonObjects.Skip(1).ToList();
        List<SongPoint> pairs = GetFromJObject();
        AllPairs = pairs;
    }

    public List<JObject> Deserialize()
    {
        return JsonDeserializer.Deserialize();
    }

    /// <summary>
    /// Parses a JsonReport to a list of tracks and songs.
    /// </summary>
    /// <returns>A list of tracks and a list of songs representing the included data.</returns>
    /// <exception cref="Exception"></exception>
    public List<SongPoint> GetFromJObject()
    {
        int alreadyParsed = 0; // The number of points already parsed

        // Get the header
        JObject header = JsonObjects.First();
        int expectedTotal = header.Value<int>("Total");

        List<SongPoint> allPairs = JsonTracksOnly
            .SelectMany((track, index) =>
            {
                // Get the Count item
                int expectedPairCount = track.Value<int>("Count");

                // Get the TrackInfo item
                JObject trackInfo = track.Value<JObject>("TrackInfo") ?? throw new Exception($"No TrackInfo object for track {index}");
                TrackInfo tInfo = trackInfo.ToObject<TrackInfo>();

                // Get the track pairs
                JArray pairs = track.Value<JArray>(tInfo.Name) ?? throw new Exception($"No pairs tree found with track name '{tInfo.Name}'");

                List<SongPoint> trackPairs = pairs
                    .Select(pair => pair.ToObject<SongPoint>())
                    .Where((pair, index) => pair.Origin == tInfo && pair.Index - alreadyParsed == index)
                    .ToList();

                List<int> indexes = trackPairs.Select(pair => pair.Index).ToList();

                // Verify the quantity of pairs
                if (trackPairs.Count != expectedPairCount)
                {
                    VerifyQuantity(alreadyParsed, expectedPairCount, indexes, tInfo.Index);
                }

                alreadyParsed += trackPairs.Count; // Add the number of pairs in this track

                return trackPairs;
            })
            .ToList();

        return allPairs;
    }

    /// <summary>
    /// Verifies per-track pair quantities match JSON metadata fields.
    /// </summary>
    /// <param name="start">The index of the first pair in this track.</param>
    /// <param name="expectedCount">The expected number of pairs in this track.</param>
    /// <param name="indexes">The list of pair indexes represented in the JsonReport.</param>
    /// <param name="trackIndex">The index of this track in the entire JsonReport file.</param>
    /// <exception cref="Exception"></exception>
    private static void VerifyQuantity(int start, int expectedCount, List<int> indexes, int trackIndex)
    {
        // Find missing indexes
        List<int> expectedIndexes = Enumerable.Range(start, expectedCount).ToList();
        List<int> missingIndexes = expectedIndexes.Where(index => !indexes.Contains(index)).ToList();

        // Output missing indexes
        string missing = string.Join(", ", missingIndexes.Select(index => index.ToString()));
        Console.WriteLine($"Missing indexes: {missing}");
        throw new Exception($"Track {trackIndex} in JsonReport expected to have {expectedCount} pairs, but had {indexes.Count}");
    }

    /// <summary>
    /// The total number of songs in the JsonReport file.
    /// </summary>
    public override int SourceSongCount => JsonTracksOnly.Select(JObject => JObject.Value<int>("Count")).Sum();

    /// <summary>
    /// The total number of points in the source JsonReport file
    /// </summary>
    public override int SourcePointCount => JsonTracksOnly.Select(JObject => JObject.Value<int>("Count")).Sum();

    /// <summary>
    /// The total number of tracks in the source JsonReport file.
    /// </summary>
    public override int SourceTrackCount => JsonTracksOnly.Count;

    /// <summary>
    /// The total number of pairs in the source JsonReport file.
    /// </summary>
    public override int SourcePairCount => JsonTracksOnly.Select(JObject => JObject.Value<int>("Count")).Sum();

    /// <summary>
    /// Verifies the hash included in the file with the contents of the file.
    /// </summary>
    /// <returns>True, if the hashes match. Otherwise, false.</returns>
    public bool VerifyHash()
    {
        JsonHashProvider<IEnumerable<JObject>> hasher = new();
        string? expectedHash = JsonObjects.First().Value<string>("SHA256Hash");
        return hasher.VerifyHash(JsonTracksOnly, expectedHash);
    }
}
