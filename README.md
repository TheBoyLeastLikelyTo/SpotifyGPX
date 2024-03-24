# SpotifyGPX

Great for road trips: match GPS positions to played Spotify songs.

SpotifyGPX allows you to recount where you listened to each song of a set based on a tracked journey, placing points as close as possible to the location of each song.

## Use cases and functions

SpotifyGPX allows you to create a map of the songs you listened to on Spotify during a journey. Imagine a map that shows not just where you went, but also the soundtrack of your trip! It does this by combining two kinds of data:

- Your Spotify listening history: It gets a list of the songs you played and when you played them from Spotify.
- A GPX file of your journey: A GPX file is like a digital map of your travels. It includes information about your location (latitude and longitude) at different points in time.

Here's how it works:

1. You provide SpotifyGPX with these two files: Your Spotify listening history JSON file and the GPX file of your trip.
2. It matches up the songs with the locations: It looks at the timestamps in both files to figure out which songs you were listening to at each point along your journey.

Cool things you can do after you return from your journey:

- Visualize your musical journeys: You can use mapping tools to see your route and the songs you listened to along the way, creating a visual memory of your trip.
- Share your musical adventures: You can share the GPX file (or Google MyMaps link) with friends or family so they can experience your trip through your music choices.
- Create playlists based on your travels: You can export a playlist of the songs you listened to during a particular journey, making it easy to relive those memories.

Think of it like a musical scrapbook of your travels!

## Privacy notice

SpotifyGPX is not endorsed by Spotify Technology SA. It exists only as a third-party tool that you can opt to use with the data Spotify freely provides to you. SpotifyGPX does not interact with Spotify itself in any way. It relies on user-submitted data alone.
SpotifyGPX does not exchange your data with any outside parties. In other words, the data you feed SpotifyGPX (including your original GPX tracks and Spotify data) is operated on by your computer alone.

SpotifyGPX does not modify the contents of the files you feed it. It will instead create new files representing its calculated data.

Why don't we just use the Spotify API instead of having users provide JSON? The Spotify API only provides us the [last 50 songs](https://developer.spotify.com/documentation/web-api/reference/get-recently-played) played by a user, which is rarely adequate for including their entire journey (if at all).

## Requirements

### Running SpotifyGPX
- [.NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) & C# version 11 installed
- A supported journey source file (see below), containing position data (such as GPX)
- A supported song playback source file (see below), containing played back songs (such as JSON)
- Dependencies of file format handlers (such as Newtonsoft.Json, see formats table for more info)  

## Supported Formats

### Native Format Support

SpotifyGPX's InputHandler and OutputHandler classes allow you to easily implement your own formats if desired.
See the below `Source` links for each format's input or output definition source file, and the below `Sample` links for samples of actual data in these formats.

| Format | Extension | Input | Output | Type of Data | Hash Support |
| ------ | --------- | ----- | ------ | ------------ | ------------ |
| [JSON](https://en.wikipedia.org/wiki/JSON) | `.json` | [Source](SpotifyGPX/Input/Json.cs) / [Sample](Samples/sample.json) (With [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/)) | [Source](SpotifyGPX/Output/Json.cs) / [Sample](Samples/sample_20230709.json) (With [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/)) | Song Records |  |
| [XSPF](https://en.wikipedia.org/wiki/XML_Shareable_Playlist_Format) | `.xspf` | [Source](SpotifyGPX/Input/Xspf.cs) / [Sample](Samples/sample_20230709.xspf) | [Source](SpotifyGPX/Output/Xspf.cs) / [Sample](Samples/sample_20230709.xspf) | Song Records | X |
| [KML](https://en.wikipedia.org/wiki/Keyhole_Markup_Language) | `.kml` | Coming Soon | | GPS Journey |  |
| [GPX](https://en.wikipedia.org/wiki/GPS_Exchange_Format) | `.gpx` | [Source](SpotifyGPX/Input/Gpx.cs) / [Sample](Samples/sample.gpx) | | GPS Journey |  |
| [JsonReport](https://en.wikipedia.org/wiki/JSON) | `.jsonreport` | [Source](SpotifyGPX/Input/JsonReport.cs) / [Sample](Samples/sample_All.jsonreport) | [Source](SpotifyGPX/Output/JsonReport.cs) / [Sample](Samples/sample_All.jsonreport) | Pair Records | X |
| [GPX](https://en.wikipedia.org/wiki/GPS_Exchange_Format) | `.gpx` | | [Source](SpotifyGPX/Output/Gpx.cs) / [Sample](Samples/sample_20230709.gpx) | Pair Records | X |
| [KMZ](https://en.wikipedia.org/wiki/Keyhole_Markup_Language) | `.kmz` | | Coming Soon | Pair Records | X |
| [CSV](https://en.wikipedia.org/wiki/Comma-separated_values) | `.csv` | | [Source](SpotifyGPX/Output/Csv.cs) / [Sample](Samples/sample_20230709.csv) | Pair Records |  |
| [TXT](https://en.wikipedia.org/wiki/Text_file) | `.txt` | | [Source](SpotifyGPX/Output/Txt.cs) / [Sample](Samples/sample_20230709.txt) | Pair Records |  |
| [XLSX](https://en.wikipedia.org/wiki/Office_Open_XML) | `.xlsx` | | [Source](SpotifyGPX/Output/Xlsx.cs) / [Sample](Samples/sample_20230709.xlsx) (With [EPPlus](https://www.nuget.org/packages/EPPlus/)) | Pair Records |  |

### XML Conversion & Transformation

SpotifyGPX's OutputHandler class also supports conversion of certain data types to XML.

If `-x` is passed, and you have also passed flags for exporting in the below supported formats, an XML of the data will be saved in addition to the targeted format.

For example, if you pass `-r` and `-x`, the `<name>.jsonreport` will be saved in addition to a `<name>.jsonreport.xml` representing the JsonReport data in XML-form.

See the below table for the object types (and their corresponding formats) supporting conversion to XML.

| Data Source Type | Applicable Output Formats | Supports Conversion to XML |
| ---------------- | ------------------------- | ------------------- |
| `List<JObject>` | JSON, JsonReport | Yes |
| `XDocument` | GPX, KML, XSPF | Yes |
| `string?[]` | CSV, TXT | Yes |
| `byte[]` | XLSX | No |

Below are some pre-created XSLT stylesheets for transforming the converted XML of various output formats.

| Stylesheet | Transforms from | Transforms to | Description | Sample |
| ---------- | --------------- | ------------- | ----------- | ------ |
| [copy.xslt](SpotifyGPX/Xslt/copy.xslt) | Any XML | An Identical XML | Maintains the identical structure of the original XML in the transformed XML | |
| [csv.xslt](SpotifyGPX/Xslt/csv.xslt) | CSV XML | HTML | Creates a table representing the contents of the CSV table | [Sample](Samples/sample_20230709.csv.html) |
| [gpx.xslt](SpotifyGPX/Xslt/gpx.xslt) | GPX XML | HTML | Creates a table with each song-point pair represented as a row | [Sample](Samples/sample_20230709.gpx.html) |
| [json.xslt](SpotifyGPX/Xslt/json.xslt) | JSON XML | HTML | Creates a table of song playback records | [Sample](Samples/sample_20230709.json.html) |
| [jsonreport.xslt](SpotifyGPX/Xslt/jsonreport.xslt) | JsonReport XML | HTML | Creates a table for each track, containing the pairings of the corresponding track | [Sample](Samples/sample_All.jsonreport.html) |
| [txt.xslt](SpotifyGPX/Xslt/txt.xslt) | TXT XML | HTML | Creates a bulleted list, each bullet containing a line from the text file, with its contents hyperlinked | [Sample](Samples/sample_20230709.txt.html) |
| [txt_nolink.xslt](SpotifyGPX/Xslt/txt_nolink.xslt) | TXT XML | HTML | Creates a bulleted list, each bullet containing a line from the text file | |
| [xspf.xslt](SpotifyGPX/Xslt/xspf.xslt) | XSPF XML | HTML | Creates a table of song playback records | [Sample](Samples/sample_20230709.xspf.html) |  

Transforming XML with XSLT stylesheets using SpotifyGPX:

- if there is an `.xslt` file of the same name as a selected output format class in the running directory (ie. `jsonreport.xslt` for exporting JsonReport with `-r` flag), and,
- if the data type of said format is supported for conversion to XML (see above table), and,
- if `-x` is passed

Then, the XSLT stylesheet will be used in the creation of the converted XML.

If `-x` is passed and no stylesheet is present, the XML will be saved without transformation.

## Usage

>  `SpotifyGPX [--spotify <spotify> --gps <gps>] [--pairs <pairs>] [-n] [-c] [-j] [-p] [-t] [-r] [-e] [-x] [-pp [-pa]] [-s] [-h]`

| Argument | Description |
| ----- | ----- |
| `SpotifyGPX` | Path to the SpotifyGPX executable |
| `--spotify <spotify> --gps <gps>` | Path to a Spotify playback history and GPS journey file |
| `--pairs <pairs>` | Path to a pairs file |
| `-n` | Do not export a `GPX` from the calculated points |
| `-c` | Export a CSV table of all the pairs |
| `-j` | Export the relevant part of the Spotify `json` |
| `-p` | Export a XSPF playlist of the songs |
| `-t` | Export a plain text list of pairs |
| `-r` | Export a JsonReport of all the data used to compile the resulting pairings |
| `-e` | Export an Excel workbook of all pairings, grouped into worksheets for each track |
| `-x` | Export an XML conversion of each file exported (combine this with other format export flags) |
| `-pp` | Predict new positions for duplicate points (use with `-pa` for automatic prediction of all duplicate positions) |
| `-s` | Do not print out each newly created Song-Point pairing upon creation |
| `-h` | Print the help instructions |

## Preparing for a journey

Ensure you take the below steps to prepare before setting off:

1. Make sure you have access to the music playback application (ie. Spotify) along the journey
2. Use an app such as [GPSLogger (for Android)](https://github.com/mendhak/gpslogger) or [Open GPX Tracker for iOS + WatchOS](https://github.com/merlos/iOS-Open-GPX-Tracker) to track your position at different points in time along the route
3. Ensure a position is saved frequently enough for SpotifyGPX to create accurate pairings, between a song and a point (recommended: 1 point every 15-30 seconds)
4. Run a few test logging sessions at short distances. Make sure points are created by your tracker, and that their times have an identifiable time zone (UTC or UTC offset). Compare your test data to the confirmed [sample GPX log.](Samples/sample.gpx)
 
As long as there is song playback and GPS tracking running simultaneously, and that both datasets have times for each record, you will be able to use SpotifyGPX to pair the two sets of data.

## After your journey

To use SpotifyGPX, retrieve the data you tracked:

1. [Request and download](https://www.spotify.com/account/privacy/) your `Account data` or `Extended streaming history` data `json` (see below for a comparison between the two forms)
2. Copy the appropriate GPX files from the device you used for tracking
3. Run SpotifyGPX with the following two files:

| Sample | Use case |
| ------ | -------- |
| [GPX](Samples/sample.gpx) | The route you took and the times where you were in each place |
| [JSON](Samples/sample.json) | The songs you listened to and the times you listened to them |

SpotifyGPX will allow you to:

1. Use each of the coordinates (from the GPX) to identify (with closest possible precision) a position for a song
2. Use the exported formats below to parse the song-point positions:

| Sample | Use case |
| ------ | -------- |
| [CSV](Samples/sample_20230709.csv) | View a table of pairs for each track |
| [GPX](Samples/sample_20230709.gpx) | View a map of where each of your tracks was played using a tool like [kepler.gl](https://github.com/keplergl/kepler.gl) or [Google MyMaps](https://www.google.com/mymaps) |
| [JSON](Samples/sample_20230709.json) | Preserve the original Spotify data (from the original JSON) used for your journey |
| [JsonReport](Samples/sample_All.jsonreport) | Preserve all data SpotifyGPX used to create the pairings for your journey. Includes data from Json. |
| KMZ | View a map of where each of your tracks was played using a tool like [kepler.gl](https://github.com/keplergl/kepler.gl) or [Google MyMaps](https://www.google.com/mymaps) |
| [TXT](Samples/sample_20230709.txt) | Create a txt file of a Spotify URI (song link) list of the songs on your journey. [Paste](https://community.spotify.com/t5/Your-Library-how-to-paste-a-list-of-URL-s-into-a-playlist/m-p/5355978/highlight/true#M19851) into Spotify playlists on Desktop. |
| XLSX | Complete data analysis and filtration of pairing data \
| [XSPF](Samples/sample_20230709.xspf) | Use as a playlist to visualize your journey's songs in an app such as [VLC Media Player](https://www.videolan.org/) |

## Built-In Options

[Options.cs](SpotifyGPX/Options.cs) contains global and local options variables for various elements within the program.

### Multi-track selection prompt
The following parsing options are given to you when your journey contains multiple track `<trk>` elements:

| Option | Filters by track of type |
| ------ | ------------------------ |
| The index of an individual track (in the printed list) | That track only (GPX, Gap, or Combined) |
| `[A] GPX tracks` | GPX |
| `[B] GPX tracks, and gaps between them` | GPX, Gap |
| `[C] Gaps between GPX tracks only` | Gap |
| `[D] GPX tracks and Combined track` | GPX, Combined |
| `[E] Gap tracks and Combined track` | Gap, Combined |
| `[F] GPX, Gap, and Combined tracks (everything)` | GPX, Gap, Combined |

Here is an explanation of each track type:

A track generated from:
1. GPX - track in the original GPS file
2. Gap - gap between tracks in your file
3. Combined - all the points in your GPX combined into a single track (ignores track designations in provided GPX)

## Types of Data from Spotify
The differences between the types of data Spotify provides are detailed below.

`Extended streaming history` data takes longer for Spotify to send, but contains significantly more information than `Account data`.

By default, SpotifyGPX needs a song name, artist name, and end time of each song. An end time value is required at the bare minimum.

### Account data (5 days):

| JSON tag name | Description |
| ------------- | ----------- |
| `endTime` | Time the song ended |
| `trackName` | Song Name |
| `artistName` | Artist Name |
| `msPlayed` | Number of milliseconds of song playback |

### Extended streaming history (30 days):

| JSON tag name | Description |
| ------------- | ----------- |
| `ts` | This field is a timestamp indicating when the track stopped playing in UTC (Coordinated Universal Time). The order is year, month and day followed by a timestamp in military time |
| `username` | This field is your Spotify username. |
| `platform` | This field is the platform used when streaming the track (e.g. Android OS, Google Chromecast). |
| `ms_played` | This field is the number of milliseconds the stream was played. |
| `conn_country` | This field is the country code of the country where the stream was played (e.g. SE - Sweden). |
| `ip_addr_decrypted` | This field contains the IP address logged when streaming the track. |
| `user_agent_decrypted` | This field contains the user agent used when streaming the track (e.g. a browser, like Mozilla Firefox, or Safari) |
| `master_metadata_track_name` | This field is the name of the track. |
| `master_metadata_album_artist_name` | This field is the name of the artist, band or podcast. |
| `master_metadata_album_album_name` | This field is the name of the album of the track. |
| `spotify_track_uri` | A Spotify URI, uniquely identifying the track in the form of “spotify:track:<`base-62 string`>” |
| `episode_name` | This field contains the name of the episode of the podcast. |
| `episode_show_name` | This field contains the name of the show of the podcast. |
| `spotify_episode_uri` | A Spotify Episode URI, uniquely identifying the podcast episode in the form of “spotify:episode:<`base-62 string`>” |
| `reason_start` | This field is a value telling why the track started (e.g. “trackdone”) |
| `reason_end` | This field is a value telling why the track ended (e.g. “endplay”). |
| `shuffle` | This field has the value True or False depending on if shuffle mode was used when playing the track. |
| `skipped` | This field indicates if the user skipped to the next song |
| `offline` | This field indicates whether the track was played in offline mode (“True”) or not (“False”). |
| `offline_timestamp` | This field is a timestamp of when offline mode was used, if used. |
| `incognito` | This field indicates whether the track was played in incognito mode (“True”) or not (“False”). |