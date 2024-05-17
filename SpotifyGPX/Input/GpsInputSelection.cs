﻿// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Input;

public abstract partial class GpsInputSelection : FileInputBase
{
    public abstract List<GpsTrack> GetAllTracks();

    protected GpsInputSelection(string path) : base(path)
    {
    }

    public virtual List<GpsTrack> GetSelectedTracks()
    {
        List<GpsTrack> AllTracks = GetAllTracks();

        if (AllTracks.Count > 1)
        {
            // If the GPX contains more than one track, provide user parsing options:

            GpsTrack combinedTrack = CombineTracks(AllTracks); // Generate a combined track (cohesive of all included tracks)
            AllTracks = CalculateGaps(AllTracks); // Add gaps between tracks as track options
            AllTracks.Add(combinedTrack); // Add the combined track to the end of the list

            return HandleMultipleTracks(AllTracks);
        }

        return AllTracks;
    }

    /// <summary>
    /// Gets input from the user about which tracks to intake.
    /// </summary>
    /// <param name="allTracks">The entire list of tracks.</param>
    /// <returns>A list of <see cref="GpsTrack"/> objects based on user selection.</returns>
    private static List<GpsTrack> HandleMultipleTracks(List<GpsTrack> allTracks)
    {
        int selectedTrackIndex; // Holds the user track selection index        

        Console.WriteLine("[INP] Multiple GPS tracks found:");

        foreach (GpsTrack track in allTracks)
        {
            Console.WriteLine($"[INP] Index: {allTracks.IndexOf(track)} {track.ToString()}");
        }

        foreach (var filter in FilterDefinitions)
        {
            Console.WriteLine($"[INP] [{filter.Key}] {filter.Value}");
        }

        Console.Write("[INP] Please enter the index of the track you want to use: ");

        // Loop the user input request until a valid option is selected
        while (true)
        {
            string input = Console.ReadLine() ?? string.Empty;
            if (int.TryParse(input, out selectedTrackIndex) && IsValidTrackIndex(selectedTrackIndex, allTracks.Count))
            {
                break; // Return this selection below
            }

            if (MultiTrackFilters.TryGetValue(input, out var FilterFunc))
            {
                return FilterFunc(allTracks).ToList();
            }

            Console.WriteLine("Invalid input. Please enter a valid track number.");
        }

        // If the user selected a specific track index, return that
        List<GpsTrack> selectedTracks = new()
        {
            allTracks[selectedTrackIndex]
        };
        return selectedTracks;
    }

    /// <summary>
    /// Combine a list of tracks into a single track.
    /// </summary>
    /// <param name="allTracks">A list of <see cref="GpsTrack"/> objects.</param>
    /// <returns>A single <see cref="GpsTrack"/> object with data from each in the list.</returns>
    /// <exception cref="Exception">The list provided was null or contained no tracks.</exception>
    private static GpsTrack CombineTracks(List<GpsTrack> allTracks)
    {
        if (allTracks == null || allTracks.Count == 0)
        {
            throw new Exception("No tracks provided to combine!");
        }

        // Combine all points from all tracks
        var combinedPoints = allTracks.SelectMany(track => track.Points);

        // Create a new GpsTrack with combined points
        GpsTrack combinedTrack = new(allTracks.Count, CombinedOrGapTrackName(allTracks.First().Track, allTracks.Last().Track), TrackType.Combined, combinedPoints.ToList());

        return combinedTrack;
    }

    /// <summary>
    /// Calculate all the gaps between tracks.
    /// </summary>
    /// <param name="allTracks">A list of <see cref="GpsTrack"/> objects.</param>
    /// <returns>A list of <see cref="GpsTrack"/> objects, containing the original tracks as well as tracks created based on the gaps between each in the original list.</returns>
    private static List<GpsTrack> CalculateGaps(List<GpsTrack> allTracks)
    {
        return allTracks
            .SelectMany((gpsTrack, index) => // For each track and its index
            {
                if (index < allTracks.Count - 1)
                {
                    GpsTrack followingTrack = allTracks[index + 1]; // Get the track after the current track (next one)
                    IGpsPoint end = gpsTrack.Points.Last(); // Get the last point of the current track
                    IGpsPoint next = followingTrack.Points.First(); // Get the first point of the next track
                    string gapName = CombinedOrGapTrackName(gpsTrack.Track, followingTrack.Track); // Create a name for the gap track based on the name of the current track and next track

                    if (end.Time != next.Time)
                    {
                        // Create a gap track based on the index of this track, the name of the gap, and the two endpoints                        
                        GpsTrack gapTrack = new(index, gapName, TrackType.Gap, new List<IGpsPoint> { end, next });

                        // Return this track, and the gap between it and the next track
                        return new[] { gpsTrack, gapTrack };
                    }
                }

                return new[] { gpsTrack }; // If there's no gap, return the GPS track
            })
            .OrderBy(track => track.Track.Index) // Order all tracks by index
            .ToList();
    }

    /// <summary>
    /// Creates a friendly name for a bridge track (combination or gap track) between <see cref="GpsTrack"/> objects.
    /// </summary>
    /// <param name="track1">The track before the break.</param>
    /// <param name="track2">The track after the break.</param>
    /// <returns>A name combining the names of the two given tracks.</returns>
    private static string CombinedOrGapTrackName(TrackInfo track1, TrackInfo track2) => $"{track1.ToString()}-{track2.ToString()}";

    /// <summary>
    /// Determines whether a user-input track selection is valid.
    /// </summary>
    /// <param name="index">The user-provided index of a <see cref="GpsTrack"/>.</param>
    /// <param name="totalTracks">The total number of tracks available for selection.</param>
    /// <returns>True, if the user-provided index is an existing <see cref="GpsTrack"/>.</returns>
    private static bool IsValidTrackIndex(int index, int totalTracks) => index >= 0 && index < totalTracks;

}
