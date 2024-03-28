// SpotifyGPX by Simon Field

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpotifyGPX.Input;

public partial class Json : SongInputBase
{
    private JsonDeserializer JsonDeserializer { get; }
    private List<JObject> AllEntries { get; }
    protected override List<SpotifyEntry> AllSongs { get; }

    public Json(string path)
    {
        JsonDeserializer = new JsonDeserializer(path, JsonSettings);
        AllEntries = JsonDeserializer.Deserialize();
        AllSongs = ParseSongs();
    }

    private List<SpotifyEntry> ParseSongs()
    {
        return AllEntries
            .Select((entry, index) => new SpotifyEntry(
                index,
                (DateTimeOffset?)entry["endTime"] ?? (DateTimeOffset?)entry["ts"] ?? throw new Exception($"'ts' timestamp missing from JSON entry {index}"),
                (string?)entry["username"],
                (string?)entry["platform"],
                (double?)entry["msPlayed"] ?? (double?)entry["ms_played"] ?? throw new Exception($"'msPlayed' duration missing from JSON entry {index}"),
                (string?)entry["conn_country"],
                (string?)entry["ip_addr_decrypted"],
                (string?)entry["user_agent_decrypted"],
                (string?)entry["trackName"] ?? (string?)entry["master_metadata_track_name"],
                (string?)entry["artistName"] ?? (string?)entry["master_metadata_album_artist_name"],
                (string?)entry["master_metadata_album_album_name"],
                (string?)entry["spotify_track_uri"],
                (string?)entry["episode_name"],
                (string?)entry["episode_show_name"],
                (string?)entry["spotify_episode_uri"],
                (string?)entry["reason_start"],
                (string?)entry["reason_end"],
                (bool?)entry["shuffle"],
                (bool?)entry["skipped"],
                (bool?)entry["offline"],
                (long?)entry["offline_timestamp"],
                (bool?)entry["incognito"]
                )
            ).ToList();
    }

    public override int SourceSongCount => AllEntries.Count;
}
