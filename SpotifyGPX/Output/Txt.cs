// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Output;

public partial class Txt : TxtSaveable
{
    protected override string?[] Document { get; }

    public Txt(IEnumerable<SongPoint> pairs) => Document = GetDocument(pairs);

    private static string?[] GetDocument(IEnumerable<SongPoint> Pairs)
    {
        // Below are some examples of arrays
        string?[] strings = GetVerbatim(Pairs, pair => pair.ToString()); // Full pair strings
        string?[] Titles = GetVerbatim(Pairs, pair => pair.Song.ToString()); // Song names
        string?[] URIs = GetVerbatim(Pairs, pair => pair.Song.Song_URI); // Song URIs (paste into Spotify desktop in a new playlist)
        string?[] IPs = GetWithoutDuplicates(Pairs, pair => pair.Song.Spotify_IP); // IP Addresses
        string?[] Platforms = GetWithoutDuplicates(Pairs, pair => pair.Song.Spotify_Platform); // Device platforms
        string?[] Countries = GetWithoutDuplicates(Pairs, pair => pair.Song.Spotify_Country); // Countries
        string?[] Accuracies = GetWithoutDuplicates(Pairs, pair => pair.Accuracy.ToString()); // Accuracies
        string?[] StartReasons = GetWithoutDuplicates(Pairs, pair => pair.Song.Song_StartReason); // Start reasons
        string?[] EndReasons = GetWithoutDuplicates(Pairs, pair => pair.Song.Song_EndReason); // End reasons

        return URIs; // Currently returns URI list, but can be changed to your specification
    }

    /// <summary>
    /// Get a variable (selector) out of each pair, sending each to an array.
    /// </summary>
    /// <typeparam name="T">The type of array to return.</typeparam>
    /// <param name="pairs">A list of pairs.</param>
    /// <param name="selector">The variable (of a pair) each element of the array will contain.</param>
    /// <returns>An array (without duplicates) of the specified type, each element of which contains a selected variable of a pair.</returns>
    private static T[] GetWithoutDuplicates<T>(IEnumerable<SongPoint> pairs, Func<SongPoint, T> selector)
    {
        // Pass .Distinct() to ensure no duplicate values in returned array
        return pairs.Select(selector).Distinct().ToArray();
    }

    /// <summary>
    /// Get a variable (selector) out of each pair, sending each to an array.
    /// </summary>
    /// <typeparam name="T">The type of array to return.</typeparam>
    /// <param name="pairs">A list of pairs.</param>
    /// <param name="selector">The variable (of a pair) each element of the array will contain.</param>
    /// <returns>An array of the specified type, each element of which contains a selected variable of a pair.</returns>
    private static T[] GetVerbatim<T>(IEnumerable<SongPoint> pairs, Func<SongPoint, T> selector)
    {
        // Return all pairs' selected (selector) object as an array
        return pairs.Select(selector).ToArray();
    }

    public override int Count => Document.Length;
}
