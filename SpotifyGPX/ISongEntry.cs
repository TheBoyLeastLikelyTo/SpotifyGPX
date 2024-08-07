﻿// SpotifyGPX by Simon Field

using SpotifyGPX.Input;
using SpotifyGPX.SongInterfaces;
using System;

namespace SpotifyGPX;

/// <summary>
/// Interfaces with structs designated for song playback records.
/// All structs encapsulating song playback records must implement this interface.
/// </summary>
public interface ISongEntry : IInterfaceFront<ISongEntry>
{
    /// <summary>
    /// The description of this song, as printed to description fields.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// The index of this song in a list of songs.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// The time of the song as provided by the given <see cref="ISongInput"/> file.
    /// </summary>
    public DateTimeOffset FriendlyTime { get; set; }

    /// <summary>
    /// The official time of the song, used to pair it with a GPS point.
    /// </summary>
    public DateTimeOffset Time
    {
        get
        {
            if (this is IEstimatableSong estimate)
            {
                return CurrentUsage switch
                {
                    TimeUsage.Start => estimate.TimeStarted,
                    TimeUsage.End => estimate.TimeEnded,
                    _ => throw new InvalidOperationException("Time usage not set.")
                };
            }

            return FriendlyTime;
        }
    }

    /// <summary>
    /// The artist of the song.
    /// </summary>
    public string? Song_Artist { get; set; }

    /// <summary>
    /// The name of the song.
    /// </summary>
    public string? Song_Name { get; set; }

    /// <summary>
    /// Determines whether the start or end time of the song should be used for the official <see cref="Time"/> time.
    /// </summary>
    public TimeUsage CurrentUsage { get; }

    /// <summary>
    /// Determines whether <see cref="FriendlyTime"/> was interpreted as the start or end time of the song.
    /// </summary>
    public TimeInterpretation CurrentInterpretation { get; set; }

    /// <summary>
    /// Provides the name of the song action time.
    /// </summary>
    public string TimeName => $"{TimeAction}{(IsEstimated ? " (est)" : string.Empty)}";

    private string TimeAction => CurrentUsage switch
    {
        TimeUsage.Start => "started",
        TimeUsage.End => "ended",
        _ => "started OR ended"
    };

    /// <summary>
    /// Determines whether this <see cref="ISongEntry"/>'s <see cref="Time"/> was estimated or exact.
    /// </summary>
    public bool IsEstimated => (int)CurrentUsage != (int)CurrentInterpretation;

    /// <summary>
    /// The name of this song, as printed to name fields. Also includes artist.
    /// </summary>
    /// <returns>The song name as provided by the song file format.</returns>
    public string ToString();

    /// <summary>
    /// Determines whether this song falls within a provided time frame.
    /// </summary>
    /// <param name="Start">The start of the time frame.</param>
    /// <param name="End">The end of the time frame.</param>
    /// <returns>True, if this song is between the given start and end times. False, if this song is outside the provided time frame.</returns>
    public bool WithinTimeFrame(DateTimeOffset Start, DateTimeOffset End) => (Time >= Start) && (Time <= End);
}
