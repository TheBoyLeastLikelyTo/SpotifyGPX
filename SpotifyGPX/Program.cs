﻿// SpotifyGPX by Simon Field

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpotifyGPX;

class Program
{
    static void Main(string[] args)
    {
        string inputJson = string.Empty; // get JSON path
        string inputGpx = string.Empty; // get GPX path
        bool noGpxExport = false;
        bool exportWaypoints = false;
        bool exportJson = false;
        bool exportPlist = false;
        bool exportSpotifyURI = false;
        bool exportJsonReport = false;

        switch (args.Length)
        {
            case 1:
                switch (args[0])
                {
                    case "-n":
                        Console.WriteLine("[HELP] Pass -n to complete a song-point pairing without sending the pairs to a GPX file");
                        break;
                    case "-w":
                        Console.WriteLine("[HELP] Pass -w to write a GPX of waypoints for each source track");
                        break;
                    case "-j":
                        Console.WriteLine("[HELP] Pass -j to export a JSON of the songs covering your journey");
                        break;
                    case "-p":
                        Console.WriteLine("[HELP] Pass -p to export a XSPF playlist of the songs covering your journey");
                        break;
                    case "-s":
                        Console.WriteLine("[HELP] Pass -s to export and copy to clipboard a TXT list of Spotify tracks you can paste into a playlist on Spotify");
                        break;
                    case "-r":
                        Console.WriteLine("[HELP] Press -r to export a verbose JSON report of all created pairings");
                        break;
                    default:
                        Console.WriteLine("[HELP] Usage: SpotifyGPX <json> <gpx> [-n] [-w] [-j] [-p] [-s] [-r]");
                        break;
                }
                return;
            case >= 2:
                inputJson = args[0];
                inputGpx = args[1];
                noGpxExport = args.Length >= 3 && args.Contains("-n");
                exportWaypoints = args.Length >= 3 && args.Contains("-w");
                exportJson = args.Length >= 3 && args.Contains("-j");
                exportPlist = args.Length >= 3 && args.Contains("-p");
                exportSpotifyURI = args.Length >= 3 && args.Contains("-s");
                exportJsonReport = args.Length >= 3 && args.Contains("-r");
                break;
            default:
                Console.WriteLine("[HELP] Usage: SpotifyGPX <json> <gpx> [-n] [-w] [-j] [-p] [-s] [-r]");
                return;
        }

        // Check the JSON file
        if (Path.GetExtension(inputJson) != ".json")
        {
            // Ensure it has a JSON extension
            Console.WriteLine($"[ERROR] Provided file, '{inputJson}', is not a JSON file!");
            return;
        }
        else if (!File.Exists(inputJson))
        {
            // Ensure it exists
            Console.WriteLine($"[ERROR] Provided file, '{inputJson}', does not exist!");
            return;
        }

        // Check the GPX file
        if (Path.GetExtension(inputGpx) != ".gpx")
        {
            // Ensure it has a GPX extension
            Console.WriteLine($"[ERROR] Provided file, '{inputGpx}', is not a GPX file!");
            return;
        }
        else if (!File.Exists(inputGpx))
        {
            // Ensure it exists
            Console.WriteLine($"[ERROR] Provided file, '{inputGpx}', does not exist!");
            return;
        }

        // Create a list of paired songs and points
        Pairings pairedEntries;

        try
        {
            // Step 1: Create list of GPX points from the GPX file
            List<GPXTrack> tracks = new GpxFile(inputGpx).ParseGpxTracks();

            // Step 2: Create list of songs played, and filter it to songs played during the GPX tracking timeframe
            List<SpotifyEntry> filteredEntries = new JsonFile(inputJson).FilterSpotifyJson(tracks);
            // Use above to filter based on filtration options defined in SpotifyGPX.Options. To run unfiltered, use below
            //List<SpotifyEntry> filteredEntries = new JsonFile(inputJson).AllSongs;

            // Step 3: Create list of songs and points paired as close as possible to one another
            pairedEntries = new Pairings(filteredEntries, tracks);

            Console.WriteLine(pairedEntries.ToString());
        }
        catch (Exception ex)
        {
            // Catch any errors found in the calculation process
            Console.WriteLine(ex);
            return;
        }

        if (noGpxExport == false)
        {
            string path = Path.Combine(Directory.GetParent(inputGpx).ToString(), $"{Path.GetFileNameWithoutExtension(inputGpx)}_Tracks.gpx");
            pairedEntries.SaveCohesive(Pairings.CohesiveFormat.GPX, path);
            
        }

        if (exportWaypoints == true)
        {
            pairedEntries.SaveTracks(Pairings.TrackFormat.GPX, Path.GetFileNameWithoutExtension(inputGpx), Directory.GetParent(inputGpx).ToString(), "Waypoints");
        }

        if (exportJson == true)
        {
            pairedEntries.SaveTracks(Pairings.TrackFormat.JSON, Path.GetFileNameWithoutExtension(inputGpx), Directory.GetParent(inputGpx).ToString(), null);
        }

        if (exportPlist == true)
        {
            pairedEntries.SaveTracks(Pairings.TrackFormat.XSPF, Path.GetFileNameWithoutExtension(inputGpx), Directory.GetParent(inputGpx).ToString(), null);
        }

        if (exportSpotifyURI == true)
        {
            pairedEntries.SaveTracks(Pairings.TrackFormat.TXT, Path.GetFileNameWithoutExtension(inputGpx), Directory.GetParent(inputGpx).ToString(), null);
        }

        if (exportJsonReport == true)
        {
            string path = Path.Combine(Directory.GetParent(inputGpx).ToString(), $"{Path.GetFileNameWithoutExtension(inputGpx)}_Report.json");
            pairedEntries.SaveCohesive(Pairings.CohesiveFormat.JsonReport, path);
        }

        return; // Exit the program
    }
}
