﻿// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyGPX;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

#nullable enable

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        if (args.Length >= 2 && ".json" == Path.GetExtension(args[0]) && ".gpx" == Path.GetExtension(args[1]))
        {
            string inputJson = args[0];
            string inputGpx = args[1];
            bool noGpxExport = args.Length >= 3 && args.Contains("-n");
            bool exportJson = args.Length >= 3 && args.Contains("-j");
            bool exportPlist = args.Length >= 3 && args.Contains("-p");
            bool exportSpotifyURI = args.Length >= 3 && args.Contains("-s");
            bool predictPoints = args.Length >= 3 && args.Contains("-g");

            if (!File.Exists(inputJson))
            {
                // Ensures the specified JSON exists
                Console.WriteLine($"[INFO] Source {Path.GetExtension(inputJson)} file, '{Path.GetFileName(inputJson)}', does not exist!");
                return;
            }
            else if (!File.Exists(inputGpx))
            {
                // Ensures the specified GPX exists
                Console.WriteLine($"[INFO] Source {Path.GetExtension(inputGpx)} file, '{Path.GetFileName(inputGpx)}', does not exist!");
                return;
            }

            string outputGpx = GenerateOutputPath(inputGpx, "gpx");

            // Step 1: Create a list of all Spotify songs in the given JSON file
            List<SpotifyEntry> spotifyEntries;

            // Step 2: Create a list of all GPX points in the given GPX file
            List<GPXPoint> gpxPoints;

            // Step 3: Create a list of songs within the timeframe between the first and last GPX point
            List<SpotifyEntry> filteredEntries;

            // Step 4: Create a list of paired songs and points based on the closest time between each song and each GPX point
            List<(SpotifyEntry, GPXPoint, int)> correlatedEntries;

            try
            {
                // Step 1: Create list of songs contained in the JSON file, and get the JSON format
                spotifyEntries = JSON.ParseSpotifyJson(inputJson);

                // Step 2: Create list of GPX points from the GPX file
                gpxPoints = GPX.ParseGPXFile(inputGpx);

                // Step 3: Create list of songs played during the GPX tracking timeframe
                filteredEntries = JSON.FilterSpotifyJson(spotifyEntries, gpxPoints);

                // Step 4: Create list of songs and points paired as close as possible to one another
                correlatedEntries = GPX.CorrelateGpxPoints(filteredEntries, gpxPoints);
            }
            catch (Exception ex)
            {
                // Catch any errors found in the calculation process
                Console.WriteLine(ex);
                return;
            }

            Console.WriteLine($"[INFO] {filteredEntries.Count} Spotify entries filtered from {spotifyEntries.Count} total");
            Console.WriteLine($"[INFO] {correlatedEntries.Count} Spotify entries matched to set of {filteredEntries.Count}");

            if (noGpxExport == false)
            {
                XmlDocument document;

                List<(SpotifyEntry, GPXPoint, int)> PredictedPoints = null;

                try
                {
                    if (predictPoints == true)
                    {
                        PredictedPoints = GPX.CompleteGPX(correlatedEntries);
                    }

                    // Create a GPX document based on the list of songs and points
                    document = GPX.CreateGPXFile(predictPoints == true ? PredictedPoints : correlatedEntries, inputGpx);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating GPX: {ex}");
                    return;
                }

                // Write the contents of the GPX
                document.Save(outputGpx);

                Console.WriteLine($"[INFO] {Path.GetExtension(outputGpx)} file, '{Path.GetFileName(outputGpx)}', generated successfully!");
            }

            if (exportJson == true)
            {
                // Stage output path of output JSON
                string outputJson = GenerateOutputPath(inputGpx, "json");

                try
                {
                    // Write the contents of the JSON
                    File.WriteAllText(outputJson, JSON.ExportSpotifyJson(filteredEntries));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating JSON: {ex}");
                    return;
                }

                Console.WriteLine($"[INFO] {Path.GetExtension(outputJson)} file, '{Path.GetFileName(outputJson)}', generated successfully!");
            }

            if (exportPlist == true)
            {
                // Stage output path of output XSPF
                string outputPlist = GenerateOutputPath(inputGpx, "xspf");

                XmlDocument playlist;

                try
                {
                    // Create an XML document for the playlist
                    playlist = XSPF.CreatePlist(filteredEntries, outputPlist);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating playlist: {ex}");
                    return;
                }

                // Write the contents of the XSPF
                playlist.Save(outputPlist);

                Console.WriteLine($"[INFO] {Path.GetExtension(outputPlist)} file, {Path.GetFileName(outputPlist)}', generated successfully!");
            }

            if (exportSpotifyURI == true)
            {
                // Stage output path of output URI list
                string outputTxt = GenerateOutputPath(inputGpx, "txt");
                string clipboard;

                // Attempt to parse SpotifyEntries for URI
                try
                {
                    // Get the list of Spotify URIs as a string
                    clipboard = JSON.GenerateClipboardData(filteredEntries);
                }
                catch (Exception ex)
                {
                    // URI found to be null
                    Console.WriteLine($"Error generating clipboard data: {ex}");
                    return;
                }

                // Set the clipboard contents to the string
                Clipboard.SetText(clipboard);

                // Write the contents of the URI list
                File.WriteAllText(outputTxt, clipboard);

                Console.WriteLine($"[INFO] {Path.GetExtension(outputTxt)} file, '{Path.GetFileName(outputTxt)}', generated successfully!");

                Console.WriteLine("[INFO] Spotify URIs copied to clipboard, ready to paste into a Spotify playlist!");
            }
        }
        else
        {
            // None of these

            Console.WriteLine("[ERROR] Usage: SpotifyGPX <json> <gpx> [-j] [-p] [-n] [-s]");
            return;
        }

        // Exit the program
        return;
    }

    static string GenerateOutputPath(string inputFile, string format)
    {
        // Set up the output file path
        string outputFile = Path.Combine(Directory.GetParent(inputFile).ToString(), $"{Path.GetFileNameWithoutExtension(inputFile)}_Spotify.{format}");

        return outputFile;
    }
}

class JSON
{
    public static List<SpotifyEntry> ParseSpotifyJson(string jsonFile)
    {
        // Create list of JSON objects
        List<JObject>? sourceJson;

        try
        {
            // Attempt to deserialize JSON file to list
            sourceJson = JsonConvert.DeserializeObject<List<JObject>>(File.ReadAllText(jsonFile));
            if (sourceJson == null)
            {
                throw new Exception("Deserializing results in null return! Check your JSON!");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error deserializing given JSON file: {ex}");
        }

        // Define time string formats:
        string verboseTimeFormat = "MM/dd/yyyy HH:mm:ss";
        string minifiedTimeFormat = "yyyy-MM-dd HH:mm";

        // Create list to store the parsed Spotify songs
        List<SpotifyEntry> spotifyEntries = sourceJson.Select(track =>
        {
            DateTimeOffset parsedTime = DateTimeOffset.TryParseExact((string?)track["endTime"] ?? (string?)track["ts"], (string?)track["ts"] == null ? minifiedTimeFormat : verboseTimeFormat, null, DateTimeStyles.AssumeUniversal, out var parsed) ? parsed : throw new Exception($"Error parsing DateTimeOffset from song end timestamp: \n{track}");

            try
            {
                return new SpotifyEntry
                {
                    Time_End = parsedTime,
                    Spotify_Username = (string?)track["username"],
                    Spotify_Platform = (string?)track["platform"],
                    Time_Played = (string?)track["msPlayed"] ?? (string?)track["ms_played"],
                    Spotify_Country = (string?)track["conn_country"],
                    Spotify_IP = (string?)track["ip_addr_decrypted"],
                    Spotify_UA = (string?)track["user_agent_decrypted"],
                    Song_Name = (string?)track["trackName"] ?? (string?)track["master_metadata_track_name"],
                    Song_Artist = (string?)track["artistName"] ?? (string?)track["master_metadata_album_artist_name"],
                    Song_Album = (string?)track["master_metadata_album_album_name"],
                    Song_URI = (string?)track["spotify_track_uri"],
                    Episode_Name = (string?)track["episode_name"],
                    Episode_Show = (string?)track["episode_show_name"],
                    Episode_URI = (string?)track["spotify_episode_uri"],
                    Song_StartReason = (string?)track["reason_start"],
                    Song_EndReason = (string?)track["reason_end"],
                    Song_Shuffle = (bool?)track["shuffle"],
                    Song_Skipped = (bool?)track["skipped"],
                    Spotify_Offline = (bool?)track["offline"],
                    Spotify_OfflineTS = (string?)track["offline_timestamp"],
                    Spotify_Incognito = (bool?)track["incognito"]
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing contents of JSON tag:\n{track} to a valid song entry:\n{ex}");
            }
        }).ToList();

        return spotifyEntries;
    }

    public static List<SpotifyEntry> FilterSpotifyJson(List<SpotifyEntry> spotifyEntries, List<GPXPoint> gpxPoints)
    {
        // Find the start and end times in GPX
        DateTimeOffset gpxStartTime = gpxPoints.Min(point => point.Time);
        DateTimeOffset gpxEndTime = gpxPoints.Max(point => point.Time);

        // Create list of Spotify songs covering the tracked GPX path timeframe
        List<SpotifyEntry> spotifyEntryCandidates = new();

        try
        {
            // Attempt to filter Spotify entries within the GPX timeframe
            spotifyEntryCandidates = spotifyEntries
            .Where(entry => entry.Time_End >= gpxStartTime && entry.Time_End <= gpxEndTime)
            .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error finding points covering GPX timeframe: {ex}");
        }

        return spotifyEntryCandidates;
    }

    public static string ExportSpotifyJson(List<SpotifyEntry> tracks)
    {
        // Create a list of JSON objects
        List<JObject> json = new();

        foreach (SpotifyEntry entry in tracks)
        {
            // Attempt to parse each SpotifyEntry to a JSON object
            try
            {
                // Create a JSON object containing each element of a SpotifyEntry
                JObject songEntry = new()
                {
                    ["ts"] = entry.Time_End.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                    ["username"] = entry.Spotify_Username,
                    ["platform"] = entry.Spotify_Platform,
                    ["ms_played"] = entry.Time_Played,
                    ["conn_country"] = entry.Spotify_Country,
                    ["ip_addr_decrypted"] = entry.Spotify_IP,
                    ["user_agent_decrypted"] = entry.Spotify_UA,
                    ["master_metadata_track_name"] = entry.Song_Name,
                    ["master_metadata_album_artist_name"] = entry.Song_Artist,
                    ["master_metadata_album_album_name"] = entry.Song_Album,
                    ["spotify_track_uri"] = entry.Song_URI,
                    ["episode_name"] = entry.Episode_Name,
                    ["episode_show_name"] = entry.Episode_Show,
                    ["spotify_episode_uri"] = entry.Episode_URI,
                    ["reason_start"] = entry.Song_StartReason,
                    ["reason_end"] = entry.Song_EndReason,
                    ["shuffle"] = entry.Song_Shuffle,
                    ["skipped"] = entry.Song_Skipped,
                    ["offline"] = entry.Spotify_Offline,
                    ["offline_timestamp"] = entry.Spotify_OfflineTS,
                    ["incognito"] = entry.Spotify_Incognito
                };
            
                // Add the SpotifyEntry JObject to the list
                json.Add(songEntry);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error sending track, '{entry.Song_Name}', to JSON: {ex}");
            }
        }

        // Create a JSON document based on the list of songs within range
        string document = JsonConvert.SerializeObject(json, Newtonsoft.Json.Formatting.Indented);

        return document;
    }

    public static string GenerateClipboardData(List<SpotifyEntry> tracks)
    {
        // Create string for final clipboard contents
        string clipboard = "";

        foreach (SpotifyEntry track in tracks)
        {
            // Ensures no null values return
            if (track.Song_URI != null)
            {
                clipboard += $"{track.Song_URI}\n";
            }
            else
            {
                // If null URI, throw exception
                throw new Exception($"URI null for track '{track.Song_Name}'");
            }
        }

        // Return final clipboard contents
        return clipboard;
    }
}

class GPX
{
    public static List<GPXPoint> ParseGPXFile(string gpxFile)
    {
        // Create a new XML document
        XDocument document = new();
        XNamespace ns = "http://www.topografix.com/GPX/1/0";

        // Create a list of interpreted GPX points
        List<GPXPoint> gpxPoints = new();

        try
        {
            // Attempt to load the contents of the specified file into the XML
            document = XDocument.Load(gpxFile);
        }
        catch (Exception ex)
        {
            // If the specified XML is invalid, throw an error
            throw new Exception($"The defined GPX file is incorrectly formatted: {ex}");
        }

        if (!document.Descendants(ns + "trkpt").Any())
        {
            // If there are no <trkpt> point elements in the GPX, throw an error
            throw new Exception($"No points found in '{Path.GetFileName(gpxFile)}'!");
        }

        try
        {
            // Attempt to add all GPX <trkpt> latitudes, longitudes, and times to the gpxPoints list
            gpxPoints = document.Descendants(ns + "trkpt")
            .Select(trkpt => new GPXPoint
            {
                Time = DateTimeOffset.ParseExact(trkpt.Element(ns + "time").Value, Options.gpxPointTimeInp, null),
                Latitude = double.Parse(trkpt.Attribute("lat").Value),
                Longitude = double.Parse(trkpt.Attribute("lon").Value)
            })
            .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"The GPX parameter cannot be parsed:\n{ex}");
        }

        // Return the list of points from the GPX
        return gpxPoints;
    }

    public static List<(SpotifyEntry, GPXPoint, int)> CorrelateGpxPoints(List<SpotifyEntry> filteredEntries, List<GPXPoint> gpxPoints)
    {
        // Correlate Spotify entries with the nearest GPX points
        List<(SpotifyEntry, GPXPoint, int)> correlatedEntries = new();

        // Create a list of correlation accuracies, one for each song
        List<double> correlationAccuracy = new();

        int count = 0;

        foreach (SpotifyEntry spotifyEntry in filteredEntries)
        {
            // Create variable to hold the calculated nearest GPX point and its accuracy (absolute value in comparison to each song)
            var nearestPoint = gpxPoints
            .Select(point => new
            {
                Point = point,
                Accuracy = Math.Abs((point.Time - spotifyEntry.Time_End).TotalSeconds)
            })
            .OrderBy(item => item.Accuracy)
            .First();

            // Add correlation accuracy (seconds) to the correlation accuracies list
            correlationAccuracy.Add(nearestPoint.Accuracy);

            // Add both the current Spotify entry and calculated nearest point to the correlated entries list
            correlatedEntries.Add((spotifyEntry, nearestPoint.Point, count));

            Console.WriteLine($"[SONG] [{count}] [{spotifyEntry.Time_End.ToUniversalTime().ToString(Options.consoleReadoutFormat)} ~ {nearestPoint.Point.Time.ToUniversalTime().ToString(Options.consoleReadoutFormat)}] [~{Math.Round(nearestPoint.Accuracy)}s] {Options.GpxTitle(spotifyEntry)}");

            count++;
        }

        // Calculate and print the average correlation accuracy in seconds
        Console.WriteLine($"[INFO] Song-Point Correlation Accuracy (avg sec): {Math.Round(Queryable.Average(correlationAccuracy.AsQueryable()))}");

        // Return the correlated entries list (including each Spotify song and its corresponding point), and the list of accuracies
        return correlatedEntries;
    }

    public static XmlDocument CreateGPXFile(List<(SpotifyEntry, GPXPoint, int)> finalPoints, string gpxFile)
    {
        // Create a new XML document
        XmlDocument document = new();

        // Create the XML header
        XmlNode header = document.CreateXmlDeclaration("1.0", "utf-8", null);
        document.AppendChild(header);

        // Create the GPX header
        XmlElement GPX = document.CreateElement("gpx");
        document.AppendChild(GPX);

        // Add GPX header attributes
        GPX.SetAttribute("version", "1.0");
        GPX.SetAttribute("creator", "SpotifyGPX");
        GPX.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
        GPX.SetAttribute("xmlns", "http://www.topografix.com/GPX/1/0");
        GPX.SetAttribute("xsi:schemaLocation", "http://www.topografix.com/GPX/1/0 http://www.topografix.com/GPX/1/0/gpx.xsd");

        // Add name of GPX file, based on input GPX name
        XmlElement gpxname = document.CreateElement("name");
        gpxname.InnerText = Path.GetFileName(gpxFile);
        GPX.AppendChild(gpxname);

        double pointCount = 0;

        foreach ((SpotifyEntry song, GPXPoint point, _) in finalPoints)
        {
            // Create waypoint for each song
            XmlElement waypoint = document.CreateElement("wpt");
            GPX.AppendChild(waypoint);

            // Set the lat and lon of the waypoing to the original point
            waypoint.SetAttribute("lat", point.Latitude.ToString());
            waypoint.SetAttribute("lon", point.Longitude.ToString());

            // Set the name of the GPX point to the name of the song
            XmlElement name = document.CreateElement("name");
            name.InnerText = Options.GpxTitle(song);
            waypoint.AppendChild(name);

            // Set the time of the GPX point to the original time
            XmlElement time = document.CreateElement("time");
            time.InnerText = point.Time.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            waypoint.AppendChild(time);

            // Set the description of the point 
            XmlElement description = document.CreateElement("desc");
            description.InnerText = Options.GpxDescription(song, point.Time.Offset, point.Predicted == true ? "Point Predicted" : null);
            waypoint.AppendChild(description);
            pointCount++;
        }

        Console.WriteLine($"[INFO] {pointCount} points found in '{Path.GetFileNameWithoutExtension(gpxFile)}' added to GPX");

        return document;
    }

    public static List<(SpotifyEntry, GPXPoint, int)> CompleteGPX(List<(SpotifyEntry, GPXPoint, int)> finalPoints)
    {
        List<(SpotifyEntry, GPXPoint, int)> indexedPoints = finalPoints
        .Select((item) => (item.Item1, item.Item2, item.Item3))
        .ToList();

        var groupedDuplicates = indexedPoints
        .GroupBy(p => (p.Item2.Latitude, p.Item2.Longitude));

        foreach (var group in groupedDuplicates)
        {
            // For every duplicate cluster:

            if (group.ToList().Count < 2)
            {
                // Skip this group if it does not constitute a duplicate of two or more songs
                continue;
            }

            Console.WriteLine($"[DUPE] {string.Join(", ", group.Select(s => $"{s.Item1.Song_Name} ({s.Item3})"))}");

        }

        int startIndex = 0;
        int endIndex = 0;

        try
        {
            Console.Write("Index of the Start of your dupe: ");
            startIndex = int.Parse(Console.ReadLine());
            Console.Write("Index of the End of your dupe: ");
            endIndex = int.Parse(Console.ReadLine());
        }
        catch(FormatException)
        {
            throw new FormatException($"You must enter a number!");
        }

        (double, double) startPoint = (indexedPoints[startIndex].Item2.Latitude, indexedPoints[startIndex].Item2.Longitude);
        (double, double) endPoint = (indexedPoints[endIndex].Item2.Latitude, indexedPoints[endIndex].Item2.Longitude);
        int dupes = endIndex - startIndex;


        List<GPXPoint> intermediates = GenerateIntermediatePoints(startPoint, endPoint, dupes)
            .Select(point => new GPXPoint
            {
                Latitude = point.Item1,
                Longitude = point.Item2
            })
            .ToList();

        for (int index = 0; index < dupes; index++)
        {
            int layer = startIndex + index;
            var (song, point, _) = indexedPoints[layer];

            // Create a new GPXPoint with updated latitude and longitude
            GPXPoint updatedPoint = new()
            {
                Predicted = true,
                Latitude = intermediates[index].Latitude,
                Longitude = intermediates[index].Longitude
            };

            // Update the indexedPoints list with the new GPXPoint
            indexedPoints[layer] = (song, updatedPoint, layer);

            Console.WriteLine($"[DUPE] [{layer}] {(updatedPoint.Latitude, updatedPoint.Longitude)} {song.Song_Name}");
        }


        return indexedPoints;
    }

    public static (double, double)[] GenerateIntermediatePoints((double, double) start, (double, double) end, int n)
    {
        if (n < 2)
        {
            //return end;
        }

        (double startLat, double startLng) = start;
        (double endLat, double endLng) = end;

        var intermediatePoints = new (double, double)[n];
        for (int i = 0; i < n; i++)
        {
            double t = (double)i / (n - 1);
            double intermediateLat = startLat + t * (endLat - startLat);
            double intermediateLng = startLng + t * (endLng - startLng);
            intermediatePoints[i] = (intermediateLat, intermediateLng);
        }

        return intermediatePoints;
    }
}

class XSPF
{
    public static XmlDocument CreatePlist(List<SpotifyEntry> tracks, string plistFile)
    {
        // Create a new XML document
        XmlDocument document = new();

        // Create the XML header
        XmlNode header = document.CreateXmlDeclaration("1.0", "utf-8", null);
        document.AppendChild(header);

        // Create the XSPF header
        XmlElement XSPF = document.CreateElement("playlist");
        document.AppendChild(XSPF);

        // Add XSPF header attributes
        XSPF.SetAttribute("version", "1.0");
        XSPF.SetAttribute("xmlns", "http://xspf.org/ns/0/");

        // Set the name of the XSPF playlist to the name of the file
        XmlElement name = document.CreateElement("name");
        name.InnerText = Path.GetFileNameWithoutExtension(plistFile);
        XSPF.AppendChild(name);

        // Set the title of the XSPF playlist to the name of the file
        XmlElement creator = document.CreateElement("creator");
        creator.InnerText = "SpotifyGPX";
        XSPF.AppendChild(creator);

        // Create the trackList header
        XmlElement trackList = document.CreateElement("trackList");
        XSPF.AppendChild(trackList);

        foreach (SpotifyEntry entry in tracks)
        {
            // Create track for each song
            XmlElement track = document.CreateElement("track");
            trackList.AppendChild(track);

            // Set the creator of the track to the song artist
            XmlElement artist = document.CreateElement("creator");
            artist.InnerText = entry.Song_Artist;
            track.AppendChild(artist);

            // Set the title of the track to the song name
            XmlElement title = document.CreateElement("title");
            title.InnerText = entry.Song_Name;
            track.AppendChild(title);

            // Set the annotation of the song to the end time
            XmlElement annotation = document.CreateElement("annotation");
            annotation.InnerText = entry.Time_End.ToString(Options.gpxPointTimeInp);
            track.AppendChild(annotation);

            // Set the duration of the song to the amount of time it was listened to
            XmlElement duration = document.CreateElement("duration");
            duration.InnerText = entry.Time_Played;
            track.AppendChild(duration);
        }

        return document;
    }
}