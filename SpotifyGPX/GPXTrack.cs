﻿// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX;

/// <summary>
/// A journey track, with a name, type, and series of points representing the path on Earth of the journey. 
/// </summary>
public readonly struct GPXTrack
{
    /// <summary>
    /// Creates a GPXTrack, holding a series of points.
    /// </summary>
    /// <param name="index">The index of this track (in a series of tracks)</param>
    /// <param name="name">The friendly name of this track.</param>
    /// <param name="type">The type of this track (GPX, Gap, or Combined).</param>
    /// <param name="points">A list of the points comprising this track.</param>
    public GPXTrack(int? index, string? name, TrackType type, List<GPXPoint> points)
    {
        Track = new TrackInfo(index, name, type);
        Points = points;

        Start = Points.Select(point => point.Time).Min(); // Earliest point's time
        End = Points.Select(point => point.Time).Max(); // Latest point's time

        // Either above or below start/end parsing works, your choice

        // Start = Points.Select(point => point.Time).First(); // First point's time
        // End = Points.Select(point => point.Time).Last(); // Last point's time
    }

    /// <summary>
    /// Information about this track (such as its name, index in a list, and type).
    /// </summary>
    public readonly TrackInfo Track { get; } // Metadata for this track, including its name and index in a list

    /// <summary>
    /// A series of points that comprise this track (journey).
    /// </summary>
    public readonly List<GPXPoint> Points { get; } // Where and when were all the points in this track taken?

    /// <summary>
    /// The time and date at which this track's earliest point was taken.
    /// </summary>
    public readonly DateTimeOffset Start { get; } // What time was the earliest point logged?

    /// <summary>
    /// The time and date at which this track's latest point was taken.
    /// </summary>
    public readonly DateTimeOffset End { get; } // What time was the latest point logged?

    /// <summary>
    /// Converts this GPXTrack object to a string.
    /// </summary>
    /// <returns>A string, containing the name, number of points, start and end times, and type of the track.</returns>
    public override string ToString() // Display format for this track
    {
        StringBuilder builder = new();

        builder.Append("   Name: {0}", Track.ToString());
        builder.Append("   Points: {0}", Points.Count);
        builder.Append("   Starts: {0}", Start.ToString(Options.ISO8601Offset));
        builder.Append("   Ends: {0}", End.ToString(Options.ISO8601Offset));
        builder.Append("   Type: {0}", Track.Type);

        return builder.ToString();
    }
}
