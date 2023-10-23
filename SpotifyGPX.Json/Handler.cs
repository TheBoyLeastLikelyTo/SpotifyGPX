﻿// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;
using SpotifyGPX.Options;

namespace SpotifyGPX.Json
{
    public class Json
    {
        public static List<SpotifyEntry> FilterSpotifyJson(List<SpotifyEntry> spotifyEntries, List<GPXPoint> gpxPoints)
        {
            // Find the start and end times in GPX
            DateTimeOffset gpxStartTime = gpxPoints.Min(point => point.Time);
            DateTimeOffset gpxEndTime = gpxPoints.Max(point => point.Time);

            // Create list of Spotify songs covering the tracked GPX path timeframe
            List<SpotifyEntry> spotifyEntryCandidates = new();

            try
            {
                // Attempt to filter Spotify entries within the GPX timeframe
                spotifyEntryCandidates = spotifyEntries
                .Where(entry => entry.Time_End >= gpxStartTime && entry.Time_End <= gpxEndTime)
                .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error finding points covering GPX timeframe: {ex.Message}");
            }

            return spotifyEntryCandidates;
        }

        public static string GenerateClipboardData(List<SpotifyEntry> tracks)
        {
            // Create string for final clipboard contents
            string clipboard = "";

            foreach (SpotifyEntry track in tracks)
            {
                // Ensures no null values return
                if (track.Song_URI != null)
                {
                    clipboard += $"{track.Song_URI}\n";
                }
                else
                {
                    // If null URI, throw exception
                    throw new Exception($"URI null for track '{track.Song_Name}'");
                }
            }

            // Return final clipboard contents
            return clipboard;
        }
    }
}