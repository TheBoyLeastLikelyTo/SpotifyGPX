// SpotifyGPX by Simon Field

using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Output;

/// <summary>
/// Provides instructions for exporting pairing data to the JsonReport format.
/// </summary>
public partial class JsonReport : JsonSaveable
{
    protected override List<JObject> Document { get; }

    /// <summary>
    /// Creates a new output handler for handling files in the JsonReport format.
    /// </summary>
    /// <param name="pairs">A list of pairs to be exported.</param>
    public JsonReport(IEnumerable<SongPoint> pairs) => Document = GetDocument(pairs);

    /// <summary>
    /// Creates a JsonReport document (a list of JObjects) representing tracks and their pairs.
    /// </summary>
    /// <param name="Pairs">A list of pairs.</param>
    /// <returns>A list of JObjects, each representing a track containing pairs.</returns>
    private static List<JObject> GetDocument(IEnumerable<SongPoint> Pairs)
    {
        List<JObject> objects = Pairs
            .GroupBy(pair => pair.Origin) // Group the pairs by track (JsonReport supports multiTrack)
            .Select(track =>
            {
                return new JObject(
                    new JProperty("Count", track.Count()), // Include # of pairs in this track
                    new JProperty("TrackInfo", JToken.FromObject(track.Key)), // Include info about the GPX track
                    new JProperty(track.Key.ToString(), JToken.FromObject(track.SelectMany(pair => new JArray(JToken.FromObject(pair))))) // Create a json report for each pair
                );
            })
            .ToList();

        JsonHashProvider<IEnumerable<JObject>> hasher = new();
        string hash = hasher.ComputeHash(objects);

        JObject header = new()
        {
            //new JProperty("Created", DateTimeOffset.Now.ToUniversalTime()),
            new JProperty("Total", Pairs.Count()),
            new JProperty("SHA256Hash", hash)
        };

        objects.Insert(0, header);

        return objects;
    }

    /// <summary>
    /// The number of pairs within this JsonReport file, regardless of track.
    /// </summary>
    public override int Count
    {
        get
        {
            // For each document (JObject) in Document (List<JObject>),
            // Get that JObject's children
            // Select the last child (in this case, the pair list)
            // Get the count of pairs within the pair list
            // Get the sum of pairs in that JObject
            // Get the sum of pairs in all selected JObjects of List<JObject>

            return Document.Select(JObject => JObject.Children().Last().Select(pair => pair.Count()).Sum()).Sum();
        }
    }
}
