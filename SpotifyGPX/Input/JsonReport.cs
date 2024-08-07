﻿// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using SpotifyGPX.PointEntry;
using SpotifyGPX.SongEntry;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;

namespace SpotifyGPX.Input;

public sealed partial class JsonReport : PairInputBase, IHashVerifier
{
    private List<JsonDocument> JsonObjects { get; }
    private JsonElement Header => JsonObjects.First().RootElement;
    private List<JsonDocument> JsonTracksOnly { get; }
    protected override string FormatName => nameof(JsonReport);
    public override List<SongPoint> ParsePairsMethod() => GetFromJObject();
    public override List<SongPoint> FilterPairsMethod() => FilterPairs();
    public override List<ISongEntry> FilterSongsMethod() => FilterSongs();
    public override List<GpsTrack> FilterTracksMethod() => FilterTracks();

    public JsonReport(string path, StringBroadcaster bcast) : base(path, bcast)
    {
        using JsonNetDeserializer deserializer = new(path, bcast);
        JsonObjects = deserializer.Deserialize<JsonDocument>(JsonOptions);
        JsonTracksOnly = JsonObjects.Skip(1).ToList();
    }

    private List<SongPoint> GetFromJObject()
    {
        int alreadyParsed = 0;

        // Get the header
        int expectedTotal = Header.TryGetProperty("Total", out JsonElement total) ? total.GetInt32() : throw new Exception($"Expected total required in header object of JsonReport!");

        List<SongPoint> allPairs = JsonTracksOnly
            .SelectMany((track, index) =>
            {
                JsonElement root = track.RootElement;

                // Get the Count item
                int expectedPairCount = JsonTools.ForceGetProperty("Count", root).GetInt32();

                // TrackInfo
                TrackInfo trackInfo = GetTrackInfo(JsonTools.ForceGetProperty("TrackInfo", root));

                // Get the pairs tree
                JsonElement pairsTreeElement = JsonTools.ForceGetProperty("Track", root);
                List<SongPoint> trackPairs = pairsTreeElement
                .EnumerateArray()
                .Select(pairElement =>
                {
                    // Index
                    int pairIndex = JsonTools.ForceGetProperty("Index", pairElement).GetInt32();

                    // Song
                    ISongEntry songEntry = GetSongEntry(JsonTools.ForceGetProperty("Song", pairElement));

                    // Point
                    IGpsPoint pointEntry = GetGpsPoint(JsonTools.ForceGetProperty("Point", pairElement));

                    // Origin (TrackInfo)
                    TrackInfo trackEntry = GetTrackInfo(JsonTools.ForceGetProperty("Origin", pairElement));

                    return new SongPoint(pairIndex, songEntry, pointEntry, trackEntry);
                })
                .Where((pair, index) => pair.Origin == trackInfo && pair.Index - alreadyParsed == index)
                .ToList();

                List<int> indexes = trackPairs.Select(pair => pair.Index).ToList();

                // Verify the quantity of pairs
                if (trackPairs.Count != expectedPairCount)
                {
                    VerifyQuantity(alreadyParsed, expectedPairCount, indexes, trackInfo.Index);
                }

                alreadyParsed += trackPairs.Count; // Add the number of pairs in this track

                return trackPairs;

            })
            .ToList();

        return allPairs;
    }

    private static TrackInfo GetTrackInfo(JsonElement origin)
    {
        int? originIndex = JsonTools.TryGetProperty("Index", origin)?.GetInt32();
        string? originName = JsonTools.TryGetProperty("Name", origin)?.GetString();
        int originType = JsonTools.ForceGetProperty("Type", origin).GetInt32();
        return new TrackInfo(originIndex, originName, (TrackType)originType);
    }

    private static ISongEntry GetSongEntry(JsonElement song)
    {
        string songDescription = JsonTools.ForceGetProperty("Description", song).GetString() ?? string.Empty;
        int songIndex = JsonTools.ForceGetProperty("Index", song).GetInt32();

        DateTimeOffset interpretedTime = JsonTools.ForceGetProperty("FriendlyTime", song).GetDateTimeOffset();
        int songInterpretation = JsonTools.ForceGetProperty("CurrentInterpretation", song).GetInt32();

        DateTimeOffset songTime = JsonTools.ForceGetProperty("Time", song).GetDateTimeOffset();
        int songCurrentUsage = JsonTools.ForceGetProperty("CurrentUsage", song).GetInt32();

        string? songArtist = JsonTools.ForceGetProperty("Song_Artist", song).GetString();
        string? songName = JsonTools.ForceGetProperty("Song_Name", song).GetString();

        DateTimeOffset time = TimeSource switch
        {
            UsageSource.FriendlyTime => interpretedTime,
            UsageSource.Time => songTime,
            _ => throw new Exception("Invalid UsageSource")
        };

        int finalInterpretation = TimeSource switch
        {
            UsageSource.FriendlyTime => songInterpretation,
            UsageSource.Time => songCurrentUsage,
            _ => throw new Exception("Invalid UsageSource")
        };

        ISongEntry songEntry = new GenericEntry()
        {
            Description = songDescription,
            Index = songIndex,
            FriendlyTime = time,
            Song_Artist = songArtist,
            Song_Name = songName,
            CurrentUsage = (TimeUsage)finalInterpretation,
            CurrentInterpretation = (TimeInterpretation)finalInterpretation
        };

        return songEntry;
    }

    private static IGpsPoint GetGpsPoint(JsonElement point)
    {
        int pointIndex = JsonTools.ForceGetProperty("Index", point).GetInt32();
        JsonElement pointLocation = JsonTools.ForceGetProperty("Location", point);
        double pointLocationLatitude = JsonTools.ForceGetProperty("Latitude", pointLocation).GetDouble();
        double pointLocationLongitude = JsonTools.ForceGetProperty("Longitude", pointLocation).GetDouble();
        DateTimeOffset pointTime = JsonTools.ForceGetProperty("Time", point).GetDateTimeOffset();

        IGpsPoint pointEntry = new GenericPoint()
        {
            Index = pointIndex,
            Location = new Coordinate(pointLocationLatitude, pointLocationLongitude),
            Time = pointTime
        };

        return pointEntry;
    }

    private List<ISongEntry> FilterSongs()
    {
        return AllPairs.Select(pair => pair.Song).Where(song => songFilter(song) == true).ToList();
    }

    private List<GpsTrack> FilterTracks()
    {
        return AllTracks.Where(track => track.OfType<GenericPoint>().All(point => pointFilter(point))).ToList();
    }

    private List<SongPoint> FilterPairs()
    {
        return AllPairs.Where(pair => pairFilter(pair) == true).ToList();
    }

    private void VerifyQuantity(int start, int expectedCount, List<int> indexes, int trackIndex)
    {
        // Find missing indexes
        List<int> expectedIndexes = Enumerable.Range(start, expectedCount).ToList();
        List<int> missingIndexes = expectedIndexes.Where(index => !indexes.Contains(index)).ToList();

        // Output missing indexes
        string missing = string.Join(", ", missingIndexes.Select(index => index.ToString()));
        BCaster.BroadcastError(new Exception($"Missing indexes: {missing}"));
        throw new Exception($"Track {trackIndex} in JsonReport expected to have {expectedCount} pairs, but had {indexes.Count}");
    }

    public override int SourceSongCount => JsonTracksOnly.Select(track => track.RootElement.GetProperty("Track").EnumerateArray().Select(pair => JsonTools.ForceGetProperty("Song", pair)).Count()).Sum();

    public override int SourcePointCount => JsonTracksOnly.Select(track => track.RootElement.GetProperty("Track").EnumerateArray().Select(pair => JsonTools.ForceGetProperty("Point", pair)).Count()).Sum();

    public override int SourceTrackCount => JsonTracksOnly.Select(track => track).Count();

    public override int SourcePairCount => JsonTracksOnly.Select(track => track.RootElement.GetProperty("Track").EnumerateArray().Count()).Sum();

    protected override void DisposeDocument()
    {
        JsonObjects.ForEach(obj => obj.Dispose());
        JsonTracksOnly.ForEach(obj => obj.Dispose());
    }

    public bool VerifyHash()
    {
        JsonHashProvider hasher = new();
        string? expectedHash = JsonTools.TryGetProperty("SHA256Hash", Header)?.GetString();
        return hasher.VerifyHash(JsonTracksOnly, expectedHash);
    }

    private enum UsageSource
    {
        /// <summary>
        /// Get the JsonReport's time from the FriendlyTime field.
        /// </summary>
        FriendlyTime,

        /// <summary>
        /// Get the JsonReport's time from the Time field.
        /// </summary>
        Time
    }
}
