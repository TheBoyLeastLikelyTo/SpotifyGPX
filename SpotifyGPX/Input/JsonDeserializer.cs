// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace SpotifyGPX.Input;

/// <summary>
/// Deserialize a JSON file to a list of JObject objects.
/// </summary>
public class JsonDeserializer
{
    private string JsonPath { get; }
    private JsonSerializerSettings? SerializerSettings { get; }

    public JsonDeserializer(string path, JsonSerializerSettings? settings)
    {
        JsonPath = path;
        SerializerSettings = settings;
    }

    public List<JObject> Deserialize()
    {
        List<JObject> tracks = new();

        JsonSerializer serializer = JsonSerializer.Create(SerializerSettings);

        using (var fileStream = File.OpenRead(JsonPath))
        using (var streamReader = new StreamReader(fileStream))
        using (var jsonReader = new JsonTextReader(streamReader))
        {
            while (jsonReader.Read())
            {
                if (jsonReader.TokenType == JsonToken.StartObject)
                {
                    var json = serializer.Deserialize<JObject>(jsonReader);

                    if (json != null)
                    {
                        tracks.Add(json);
                    }
                    else
                    {
                        throw new Exception($"Input file contains null JSON entries on top level entry.");
                    }
                }
            }
        }

        return tracks;
    }
}
