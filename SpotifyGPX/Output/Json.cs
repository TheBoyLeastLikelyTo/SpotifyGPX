// SpotifyGPX by Simon Field

using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Output;

public partial class Json : JsonSaveable
{
    protected override List<JObject> Document { get; }

    public Json(IEnumerable<SongPoint> pairs) => Document = GetDocument(pairs);

    private static List<JObject> GetDocument(IEnumerable<SongPoint> Pairs)
    {
        return Pairs.Select(pair =>
        {
            JObject songJson = JObject.FromObject(pair.Song);

            // Uncomment below to return unchanged:
            //return songJson;

            // SpotifyGPX properties' leading name:
            string prefix = "SGPX_";

            // Remove non-Spotify properties
            songJson.Properties() // For all the properties in the JObject,
                .Where(property => property.Name.StartsWith(prefix)) // If the name indicates it is non-Spotify:
                .ToList() // Send non-Spotify properties to a list
                .ForEach(removeThis => removeThis.Remove()); // Remove non-Spotify properties.

            return songJson;

        }).ToList();
    }

    public override int Count => Document.Count; // Number of JObjects in list
}
