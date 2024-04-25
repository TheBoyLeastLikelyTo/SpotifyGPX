﻿// SpotifyGPX by Simon Field

using SpotifyGPX.Api;
using SpotifyGPX.Input;
using SpotifyGPX.Output;
using System;
using System.Collections.Generic;
using System.IO;

namespace SpotifyGPX;

class Program
{
    static void Main(string[] args)
    {
        var (options, flags) = ArgumentParser.Parse(args);

        string? inputPairs = null;
        string? inputSpotify = null;
        string? inputGps = null;

        if (args.Length == 0 || flags.Contains("h"))
        {
            ArgumentParser.PrintHelp();
            return;
        }

        if (options.ContainsKey("spotify"))
        {
            inputSpotify = options["spotify"];
        }

        if (options.ContainsKey("gps"))
        {
            inputGps = options["gps"];
        }

        if (options.ContainsKey("pairs"))
        {
            inputPairs = options["pairs"];
        }

        bool exportCsv = flags.Contains("c");
        bool exportGpx = flags.Contains("g");
        bool exportJson = flags.Contains("j");
        bool exportPlist = flags.Contains("p");
        bool exportTxt = flags.Contains("t");
        bool exportJsonReport = flags.Contains("r");
        bool exportKml = flags.Contains("k");
        bool exportExcel = flags.Contains("e");
        bool grabApiData = flags.Contains("a");
        bool pointPredict = flags.Contains("pp");
        bool autoPredict = flags.Contains("pa");
        bool silent = flags.Contains("s");
        bool transform = flags.Contains("x");

        // Create a list of paired songs and points
        PairingsHandler pairedEntries;

        // Prefix for output files
        string prefix;

        try
        {
            InputHandler input;
            EntryMatcher matcher;

            if (inputPairs != null)
            {
                // Step 0: Get input handler based on file path
                input = new(inputPairs);
                prefix = inputPairs;

                // Step 1: Get list of pairs from the pairs file
                List<SongPoint> pairs = input.GetAllPairs();

                // Step 2: Create list of songs and points paired based on the given pairs file
                pairedEntries = new PairingsHandler(pairs);
            }
            else if (inputSpotify != null && inputGps != null)
            {
                // Step 0: Get input handler based on file paths
                input = new(inputSpotify, inputGps);
                prefix = inputGps;

                // Step 1: Get list of GPX tracks from the GPS file
                List<GpsTrack> tracks = input.GetSelectedTracks();

                // Step 2: Get list of songs played from the entries file
                List<ISongEntry> songs = input.GetFilteredSongs(tracks);
                //List<ISongEntry> songs = input.GetAllSongs(); // Unfiltered run

                if (grabApiData)
                {
                    // Step 2.5: Get metadata for each song
                    matcher = new(songs);
                    songs = matcher.MatchEntries();
                }

                // Step 3: Create list of songs and points paired as close as possible to one another
                pairedEntries = new PairingsHandler(songs, tracks, silent, pointPredict, autoPredict);
            }
            else
            {
                throw new Exception($"Neither song and GPS nor pairings files provided!");
            }

            // Step 4: Write the pairing job's pair counts and averages
            pairedEntries.WriteCounts();
            pairedEntries.WriteAverages();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return;
        }

        try
        {
            string startingPrefix = Path.GetFileNameWithoutExtension(prefix);

            if (exportCsv)
                pairedEntries.Save(Formats.Csv, startingPrefix, transform);

            if (exportGpx)
                pairedEntries.Save(Formats.Gpx, startingPrefix, transform);

            if (exportJson)
                pairedEntries.Save(Formats.Json, startingPrefix, transform);

            if (exportJsonReport)
                pairedEntries.Save(Formats.JsonReport, startingPrefix, transform);

            if (exportKml)
                pairedEntries.Save(Formats.Kml, startingPrefix, transform);

            if (exportTxt)
                pairedEntries.Save(Formats.Txt, startingPrefix, transform);

            if (exportExcel)
                pairedEntries.Save(Formats.Xlsx, startingPrefix, transform);

            if (exportPlist)
                pairedEntries.Save(Formats.Xspf, startingPrefix, transform);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return;
        }

        return;
    }
}

public class ArgumentParser
{
    public static (Dictionary<string, string> options, HashSet<string> flags) Parse(string[] args)
    {
        var options = new Dictionary<string, string>();
        var flags = new HashSet<string>();

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (arg.StartsWith("--"))
            {
                if (i + 1 < args.Length)
                {
                    string key = arg[2..];
                    string value = args[i + 1];
                    options[key] = value;
                    i++;
                }
                else
                {
                    throw new ArgumentException($"Expected value after {arg}");
                }
            }
            else if (arg.StartsWith('-'))
            {
                string flag = arg[1..];
                flags.Add(flag);
            }
        }

        return (options, flags);
    }

    public static void PrintHelp()
    {
        Console.WriteLine("Usage: SpotifyGPX [--spotify <spotify> --gps <gps>] [--pairs <pairs>] [-c] [-g] [-j] [-k] [-p] [-t] [-r] [-e] [-x] [-pp [-pa]] [-s] [-h]");
        Console.WriteLine("--spotify <spotify> --gps <gps> - Path to a Spotify playback history and GPS journey file");
        Console.WriteLine("--pairs <pairs> - Path to a pairs file");
        Console.WriteLine("-c - Export a CSV table of all the pairs");
        Console.WriteLine("-g - Export a GPX from the calculated points");
        Console.WriteLine("-j - Save off the relevant part of the Spotify json");
        Console.WriteLine("-k - Export a KML from the calculated points");
        Console.WriteLine("-p - Export a XSPF playlist of the songs");
        Console.WriteLine("-t - Export a plain text list of pairs");
        Console.WriteLine("-r - Export a JsonReport of all the data used to compile the resulting pairings");
        Console.WriteLine("-e - Export an Excel workbook of all pairings, grouped into worksheets for each track");
        Console.WriteLine("-x - Export an XML conversion of each file exported (combine this with other format export flags)");
        Console.WriteLine("-pp - Predict new positions for duplicate points (use with -pa for automatic prediction of all duplicate positions)");
        Console.WriteLine("-s - Do not print out each newly created Song-Point pairing upon creation");
        Console.WriteLine("-h - Print the help instructions");
    }
}
