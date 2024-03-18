// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SpotifyGPX
{
    /// <summary>
    /// Holds constant values representing user options accessible globally.
    /// </summary>
    public class Options
    {
        private const bool IsMiniSpotify = false; // (true = "Account data", false = "Extended streaming history data")

        public const string SpotifyTimeFormat = IsMiniSpotify ? DateTimeOnly : ISO8601UTC;

        /// <summary>
        /// A time in HH:mm:ss format with no date or offset.
        /// </summary>
        public const string TimeOnly = @"HH\:mm\:ss";

        /// <summary>
        /// A date in yyyy-MM-dd and time in HH:mm format with no offset.
        /// </summary>
        public const string DateTimeOnly = @"yyyy-MM-dd HH\:mm";

        /// <summary>
        /// An ISO8601 time in UTC.
        /// DateTimeOffset must be converted to UTC before converting to string.
        /// </summary>
        public const string ISO8601UTC = @"yyyy-MM-ddTHH\:mm\:ssZ";

        /// <summary>
        /// An ISO8601 time including an offset.
        /// Can be any UTC offset.
        /// </summary>
        public const string ISO8601Offset = @"yyyy-MM-dd HH\:mm\:ss zzz";

        /// <summary>
        /// A duration of time in HH:mm:ss.fff format.
        /// </summary>
        public const string TimeSpan = @"hh\:mm\:ss\.fff";

        public static JsonSerializerSettings JsonSettings => new()
        {
            DateParseHandling = DateParseHandling.DateTime,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            DateFormatString = Options.SpotifyTimeFormat,
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented
        };

        public static JsonSerializerSettings JsonReportSettings => new()
        {
            DateParseHandling = DateParseHandling.DateTimeOffset,
            DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
            DateFormatString = $"yyyy-MM-ddTHH:mm:ss.ffffffK",
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented
        };
    }

    public readonly partial struct SpotifyEntry
    {
        private const bool InterpretAsStartTime = false; // Interpret the time played as the start time of the track (false = interpret as end time)
        private const bool PreferEstimatedStartTime = true; // Estimate the start time of the track by subtracting the time played duration from the end time (assumes the user didn't pause playback)
    }

    public partial class PairingsHandler
    {
        private static double? MaximumAbsAccuracy => null; // Greatest accepted error (in seconds) between song and point time (null = allow all pairings regardless of accuracy)
    }

    public partial class DupeHandler
    {
        private const int MinimumMatchingCoords = 2; // Minimum number of matching coordinates to be considered a duplicate
    }
}

namespace SpotifyGPX.Input
{
    public partial class InputHandler
    {
        private static ISongInput CreateSongInput(string path)
        {
            string extension = Path.GetExtension(path).ToLower();

            return extension switch
            {
                ".json" => new Json(path),
                ".jsonreport" => new JsonReport(path),
                ".xspf" => new Xspf(path),
                _ => throw new Exception($"Unsupported song file format: {extension}"),
            };
        }

        private static IGpsInput CreateGpsInput(string path)
        {
            string extension = Path.GetExtension(path).ToLower();

            return extension switch
            {
                ".gpx" => new Gpx(path),
                ".kml" => new Kml(path),
                ".jsonreport" => new JsonReport(path),
                _ => throw new Exception($"Unsupported GPS file format: {extension}"),
            };
        }

        private static IPairInput CreatePairInput(string path)
        {
            string extension = Path.GetExtension(path).ToLower();

            return extension switch
            {
                ".jsonreport" => new JsonReport(path),
                _ => throw new Exception($"Unsupported pairs file format: {extension}")
            };
        }
    }

    public partial interface ISongInput
    {
        private static TimeSpan MinimumPlaytime => new(0, 0, 0); // Minimum accepted song playback time (0,0,0 for all songs)
        private const bool ExcludeSkipped = false; // Ignore songs skipped by the user, as defined by Spotify JSON (false for all songs)
    }

    public partial interface IGpsInput
    {
        private static Dictionary<string, Func<IEnumerable<GPXTrack>, IEnumerable<GPXTrack>>> MultiTrackFilters => new()
        {
            { "A", allTracks => allTracks.Where(track => track.Track.Type == TrackType.GPX) },
            { "B", allTracks => allTracks.Where(track => track.Track.Type != TrackType.Combined) },
            { "C", allTracks => allTracks.Where(track => track.Track.Type == TrackType.Gap) },
            { "D", allTracks => allTracks.Where(track => track.Track.Type != TrackType.Gap) },
            { "E", allTracks => allTracks.Where(track => track.Track.Type != TrackType.GPX) },
            { "F", allTracks => allTracks }
        };

        private static Dictionary<string, string> FilterDefinitions => new()
        {
            { "A", "GPS tracks" },
            { "B", "GPS tracks, and the Gaps between them as tracks" },
            { "C", "Gaps between GPS tracks as tracks" },
            { "D", "GPS tracks and Combined track" },
            { "E", "Gap tracks and Combined track" },
            { "F", "GPS, Gap, and Combined tracks (everything)" }
        };
    }

    public partial class Gpx
    {
        private static XNamespace InputNs => "http://www.topografix.com/GPX/1/0";
        private const string Track = "trk";
        private const string TrackPoint = "trkpt";
        private const string TimeFormat = $"yyyy-MM-ddTHH:mm:ss.fffzzz";
        private const DateTimeStyles TimeStyle = DateTimeStyles.None;
    }

    public partial class Json
    {
        private static JsonSerializerSettings JsonSettings => Options.JsonSettings;
    }

    public partial class JsonReport
    {
        private static JsonSerializerSettings JsonSettings => Options.JsonReportSettings;
    }

    public partial class Kml
    {
        private static XNamespace InputNs => "http://www.opengis.net/kml/2.2";
        private static XNamespace Gx => "http://www.google.com/kml/ext/2.2";
        private const string TimeFormat = $"yyyy-MM-ddTHH:mm:ss.fffZ";
        private const DateTimeStyles TimeStyle = DateTimeStyles.None;
    }

    public partial class Xspf
    {
        private static XNamespace InputNs => "http://xspf.org/ns/0/";
        private const string Track = "track";
        private const DateTimeStyles TimeStyle = DateTimeStyles.AssumeUniversal;
    }
}

namespace SpotifyGPX.Output
{
    public partial class OutputHandler
    {
        private const bool ReplaceFiles = true;
        private const string AllTracksName = "All";
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 10000;

        private static IFileOutput CreateFileOutput(Formats format, IEnumerable<SongPoint> pairs, string trackName)
        {
            return format switch
            {
                Formats.Csv => new Csv(pairs),
                Formats.Gpx => new Gpx(pairs, trackName),
                Formats.Json => new Json(pairs),
                Formats.JsonReport => new JsonReport(pairs),
                Formats.Kml => new Kml(pairs, trackName),
                Formats.Txt => new Txt(pairs),
                Formats.Xlsx => new Xlsx(pairs),
                Formats.Xspf => new Xspf(pairs, trackName),
                _ => throw new Exception($"Unsupported file export format: {format}")
            };
        }

        private static bool AllowsMultiTrack(Formats format)
        {
            return format switch
            {
                Formats.Csv => false,
                Formats.Gpx => false,
                Formats.Json => false,
                Formats.JsonReport => true,
                Formats.Kml => false,
                Formats.Txt => false,
                Formats.Xlsx => true,
                Formats.Xspf => false,
                _ => throw new Exception($"Unsupported file export format: {format}")
            };
        }
    }

    public partial class Csv
    {
        private const string Delimiter = ",";
        protected override SaveOptions OutputOptions => SaveOptions.None;
        protected override Encoding OutputEncoding => Encoding.UTF8;
    }

    public partial class Gpx
    {
        private static XNamespace Namespace => "http://www.topografix.com/GPX/1/0";
        private const string DocumentEncoding = "utf-8";
        private static XNamespace Xsi => "http://www.w3.org/2001/XMLSchema-instance";
        private const string Schema = "http://www.topografix.com/GPX/1/0 http://wwwtopografix.com/GPX/1/0/gpx.xsd";
        private const string Waypoint = "wpt";
        protected override SaveOptions OutputOptions => SaveOptions.None;
        protected override Encoding OutputEncoding => Encoding.UTF8;
    }

    public partial class Json
    {
        protected override JsonSerializerSettings JsonSettings => Options.JsonSettings;
        protected override SaveOptions OutputOptions => SaveOptions.None;
        protected override Encoding OutputEncoding => Encoding.UTF8;
    }

    public partial class JsonReport
    {
        protected override JsonSerializerSettings JsonSettings => Options.JsonReportSettings;
        protected override SaveOptions OutputOptions => SaveOptions.None;
        protected override Encoding OutputEncoding => Encoding.UTF8;
    }

    public partial class Kml
    {
        private static XNamespace Namespace => "http://www.opengis.net/kml/2.2";
        private const string DocumentEncoding = "utf-8";
        private static XNamespace Gx => "http://www.google.com/kml/ext/2.2";
        private const string Placemark = "Placemark";
        protected override SaveOptions OutputOptions => SaveOptions.None;
        protected override Encoding OutputEncoding => Encoding.UTF8;
    }

    public partial class Txt
    {
        protected override SaveOptions OutputOptions => SaveOptions.None;
        protected override Encoding OutputEncoding => Encoding.UTF8;
    }

    public partial class Xlsx
    {
        private const bool CreateTotalRow = true;
        private const bool CreatePivots = true;
    }

    public partial class Xspf
    {
        private static XNamespace Namespace => "http://xspf.org/ns/0/";
        private const string DocumentEncoding = "utf-8";
        private const string Track = "track";
        protected override SaveOptions OutputOptions => SaveOptions.None;
        protected override Encoding OutputEncoding => Encoding.UTF8;
    }

}
