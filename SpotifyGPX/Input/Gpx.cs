﻿// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SpotifyGPX.Input;

/// <summary>
/// Provides instructions for parsing GPS data from the GPX format.
/// </summary>
public class Gpx : IGpsInput
{
    private static XNamespace InputNs => "http://www.topografix.com/GPX/1/0"; // Namespace of input GPX
    private static string Track => "trk"; // Name of a track element
    private static string TrackPoint => "trkpt"; // Name of a track point object, children of tracks
    private XDocument Document { get; } // Entire input GPX document
    private List<GPXTrack> Tracks { get; } // Parsed tracks from GPX document

    /// <summary>
    /// Creates a new input handler for handling files in the GPX format.
    /// </summary>
    /// <param name="path">The path to the GPX file.</param>
    /// <exception cref="Exception">No tracks and/or points were found in the given file.</exception>
    public Gpx(string path)
    {
        Document = XDocument.Load(path);

        if (TrackCount == 0)
        {
            // If there are no tracks in the GPX, throw error
            throw new Exception($"No track elements found in '{Path.GetFileName(path)}'!");
        }

        if (PointCount == 0)
        {
            // If there are no points the GPX, throw error
            throw new Exception($"No points found in '{Path.GetFileName(path)}'!");
        }

        Tracks = ParseTracks();
    }

    /// <summary>
    /// The total number of track elements in this GPX file.
    /// </summary>
    public int TrackCount => Document.Descendants(InputNs + Track).Count();

    /// <summary>
    /// The total number of point elements in this GPX file.
    /// </summary>
    public int PointCount => Document.Descendants(InputNs + TrackPoint).Count();

    /// <summary>
    /// Gets all the tracks in this GPX file.
    /// </summary>
    /// <returns>A list of GPXTrack objects.</returns>
    public List<GPXTrack> GetAllTracks()
    {
        return Tracks;
    }

    /// <summary>
    /// Parses this GPX document into a readable list of tracks.
    /// </summary>
    /// <returns>A list of GPXTrack objects.</returns>
    /// <exception cref="Exception">An element (latitude, longitude, or time) of this point was null.</exception>
    private List<GPXTrack> ParseTracks()
    {
        List<GPXTrack> allTracks = Document.Descendants(InputNs + Track)
            .Select((trk, index) => new GPXTrack( // For each track and its index, create a new GPXTrack
                index,
                trk.Element(InputNs + "name")?.Value,
                TrackType.GPX,
                trk.Descendants(InputNs + TrackPoint)
                    .Select((trkpt, pointIndex) => new GPXPoint( // For each point and its index, create a new GPXPoint
                        pointIndex,
                        new Coordinate( // Parse its coordinate
                            double.Parse(trkpt.Attribute("lat")?.Value ?? throw new Exception($"GPX 'lat' cannot be null, check your GPX")),
                            double.Parse(trkpt.Attribute("lon")?.Value ?? throw new Exception($"GPX 'lon' cannot be null, check your GPX"))
                        ),
                        trkpt.Element(InputNs + "time")?.Value ?? throw new Exception($"GPX 'time' cannot be null, check your GPX")
                    )).ToList() // Send all points to List<GPXPoint>
            ))
            .ToList(); // Send all tracks to List<GPXTrack>

        if (allTracks.Count > 1)
        {
            // If the GPX contains more than one track, provide user parsing options:

            GPXTrack combinedTrack = CombineTracks(allTracks); // Generate a combined track (cohesive of all included tracks)
            allTracks = CalculateGaps(allTracks); // Add gaps between tracks as track options
            allTracks.Add(combinedTrack); // Add the combined track to the end of the list

            // Take user input for track/filtration selection
            return HandleMultipleTracks(allTracks);
        }

        return allTracks; // Return final track list
    }

    /// <summary>
    /// Gets input from the user about which tracks to intake.
    /// </summary>
    /// <param name="allTracks">The entire list of tracks.</param>
    /// <returns>A list of GPXTrack objects based on user selection.</returns>
    private static List<GPXTrack> HandleMultipleTracks(List<GPXTrack> allTracks)
    {
        // Display all the tracks to the user
        DisplayTrackOptions(allTracks);

        int selectedTrackIndex; // Holds the user track selection index

        // Loop the user input request until a valid option is selected
        while (true)
        {
            string input = Console.ReadLine() ?? string.Empty;
            if (int.TryParse(input, out selectedTrackIndex) && IsValidTrackIndex(selectedTrackIndex, allTracks.Count))
            {
                break; // Return this selection below
            }
            switch (input)
            {
                case "A":
                    return allTracks.Where(track => track.Track.Type == TrackType.GPX).ToList(); // GPX only
                case "B":
                    return allTracks.Where(track => track.Track.Type != TrackType.Combined).ToList(); // GPX and gap tracks
                case "C":
                    return allTracks.Where(track => track.Track.Type == TrackType.Gap).ToList(); // Gaps only
                case "D":
                    return allTracks.Where(track => track.Track.Type != TrackType.Gap).ToList(); // GPX and combined
                case "E":
                    return allTracks.Where(track => track.Track.Type != TrackType.GPX).ToList(); // Gaps and combined
                case "F":
                    return allTracks; // Combined, GPX, and gaps
            }
            Console.WriteLine("Invalid input. Please enter a valid track number.");
        }

        // If the user selected a specific track index, return that
        List<GPXTrack> selectedTracks = new()
        {
            allTracks[selectedTrackIndex]
        };
        return selectedTracks;
    }

    /// <summary>
    /// Write each track to the console, and display user options for intake selection.
    /// </summary>
    /// <param name="allTracks">The entire list of tracks.</param>
    private static void DisplayTrackOptions(List<GPXTrack> allTracks)
    {
        Console.WriteLine("[INP] Multiple GPX tracks found:");

        foreach (GPXTrack track in allTracks)
        {
            Console.WriteLine($"[INP] Index: {allTracks.IndexOf(track)} {track.ToString()}");
        }

        Console.WriteLine("[INP] [A] GPX tracks");
        Console.WriteLine("[INP] [B] GPX tracks, and gaps between them");
        Console.WriteLine("[INP] [C] Gaps between GPX tracks only");
        Console.WriteLine("[INP] [D] GPX tracks and Combined track");
        Console.WriteLine("[INP] [E] Gap tracks and Combined track");
        Console.WriteLine("[INP] [F] GPX, Gap, and Combined tracks (everything)");
        Console.Write("[INP] Please enter the index of the track you want to use: ");
    }

    /// <summary>
    /// Combine a list of tracks into a single track.
    /// </summary>
    /// <param name="allTracks">A list of GPXTrack objects.</param>
    /// <returns>A single GPXTrack with data from each in the list.</returns>
    /// <exception cref="Exception">The list provided was null or contained no tracks.</exception>
    private static GPXTrack CombineTracks(List<GPXTrack> allTracks)
    {
        if (allTracks == null || allTracks.Count == 0)
        {
            throw new Exception("No tracks provided to combine!");
        }

        // Combine all points from all tracks
        var combinedPoints = allTracks.SelectMany(track => track.Points);

        // Create a new GPXTrack with combined points
        GPXTrack combinedTrack = new(allTracks.Count, CombinedOrGapTrackName(allTracks.First().Track, allTracks.Last().Track), TrackType.Combined, combinedPoints.ToList());

        return combinedTrack;
    }

    /// <summary>
    /// Calculate all the gaps between tracks.
    /// </summary>
    /// <param name="allTracks">A list of GPXTrack objects.</param>
    /// <returns>A list of GPXTrack objects, containing the original tracks as well as tracks created based on the gaps between each in the original list.</returns>
    private static List<GPXTrack> CalculateGaps(List<GPXTrack> allTracks)
    {
        return allTracks
            .SelectMany((gpxTrack, index) => // For each track and its index
            {
                if (index < allTracks.Count - 1)
                {
                    GPXTrack followingTrack = allTracks[index + 1]; // Get the track after the current track (next one)
                    GPXPoint end = gpxTrack.Points.Last(); // Get the last point of the current track
                    GPXPoint next = followingTrack.Points.First(); // Get the first point of the next track
                    string gapName = CombinedOrGapTrackName(gpxTrack.Track, followingTrack.Track); // Create a name for the gap track based on the name of the current track and next track

                    if (end.Time != next.Time)
                    {
                        // Create a gap track based on the index of this track, the name of the gap, and the two endpoints                        
                        GPXTrack gapTrack = new(index, gapName, TrackType.Gap, new List<GPXPoint> { end, next });

                        // Return this track, and the gap between it and the next track
                        return new[] { gpxTrack, gapTrack };
                    }
                }

                return new[] { gpxTrack }; // If there's no gap, return the GPX track
            })
            .OrderBy(track => track.Track.Index) // Order all tracks by index
            .ToList();
    }

    /// <summary>
    /// Creates a friendly name for a bridge track (combination or gap track) between GPXTrack objects.
    /// </summary>
    /// <param name="track1">The track before the break.</param>
    /// <param name="track2">The track after the break.</param>
    /// <returns>A name combining the names of the two given tracks.</returns>
    private static string CombinedOrGapTrackName(TrackInfo track1, TrackInfo track2) => $"{track1.ToString()}-{track2.ToString()}";

    /// <summary>
    /// Determines whether a user-input track selection is valid.
    /// </summary>
    /// <param name="index">The user-provided index of a GPXTrack.</param>
    /// <param name="totalTracks">The total number of tracks available for selection.</param>
    /// <returns>True, if the user-provided index is an existing GPXTrack.</returns>
    private static bool IsValidTrackIndex(int index, int totalTracks) => index >= 0 && index < totalTracks;
}
