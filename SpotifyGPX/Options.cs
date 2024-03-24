// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace SpotifyGPX
{
    /// <summary>
    /// Holds constant values representing user options accessible globally.
    /// </summary>
    public class Options
    {
        private static bool IsMiniSpotify => false; // (true = "Account data", false = "Extended streaming history data")

        public static string SpotifyTimeFormat => IsMiniSpotify ? DateTimeOnly : ISO8601UTC;

        /// <summary>
        /// A time in HH:mm:ss format with no date or offset.
        /// </summary>
        public static string TimeOnly => @"HH\:mm\:ss";

        /// <summary>
        /// A date in yyyy-MM-dd and time in HH:mm format with no offset.
        /// </summary>
        public static string DateTimeOnly => @"yyyy-MM-dd HH\:mm";

        /// <summary>
        /// An ISO8601 time in UTC.
        /// DateTimeOffset must be converted to UTC before converting to string.
        /// </summary>
        public static string ISO8601UTC => @"yyyy-MM-ddTHH\:mm\:ssZ";

        /// <summary>
        /// An ISO8601 time including an offset.
        /// Can be any UTC offset.
        /// </summary>
        public static string ISO8601Offset => @"yyyy-MM-dd HH\:mm\:ss zzz";

        /// <summary>
        /// A duration of time in HH:mm:ss.fff format.
        /// </summary>
        public static string TimeSpan => @"hh\:mm\:ss\.fff";

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
        private static bool InterpretAsStartTime => false; // Interpret the time played as the start time of the track (false = interpret as end time)
        private static bool PreferEstimatedStartTime => true; // Estimate the start time of the track by subtracting the time played duration from the end time (assumes the user didn't pause playback)
    }

    public partial class PairingsHandler
    {
        private static double? MaximumAbsAccuracy => null; // Greatest accepted error (in seconds) between song and point time (null = allow all pairings regardless of accuracy)
    }

    public partial class DupeHandler
    {
        private static int MinimumMatchingCoords => 2; // Minimum number of matching coordinates to be considered a duplicate
    }
}

namespace SpotifyGPX.Input
{
    public partial interface ISongInput
    {
        private static TimeSpan MinimumPlaytime => new(0, 0, 0); // Minimum accepted song playback time (0,0,0 for all songs)
        private static bool ExcludeSkipped => false; // Ignore songs skipped by the user, as defined by Spotify JSON (false for all songs)
    }

    public partial class Gpx
    {
        private static XNamespace InputNs => "http://www.topografix.com/GPX/1/0";
        private static string Track => "trk";
        private static string TrackPoint => "trkpt";
        private static string TimeFormat => $"yyyy-MM-ddTHH:mm:ss.fffzzz";
        private static DateTimeStyles TimeStyle => DateTimeStyles.None;
    }

    public partial class Kml
    {
        private static XNamespace InputNs => "http://www.opengis.net/kml/2.2";
        private static XNamespace Gx => "http://www.google.com/kml/ext/2.2";
        private static string TimeFormat => $"yyyy-MM-ddTHH:mm:ss.fffZ";
        private static DateTimeStyles TimeStyle => DateTimeStyles.None;
    }

    public partial class Json
    {
        private static JsonSerializerSettings JsonSettings => Options.JsonSettings;
    }

    public partial class JsonReport
    {
        private static JsonSerializerSettings JsonSettings => Options.JsonReportSettings;
    }

    public partial class Xspf
    {
        private static XNamespace InputNs => "http://xspf.org/ns/0/";
        private static string Track => "track";
        private static DateTimeStyles TimeStyle => DateTimeStyles.AssumeUniversal;
    }
}

namespace SpotifyGPX.Output
{
    public partial class OutputHandler
    {
        private static bool ReplaceFiles => true;
        private static string AllTracksName => "All";
    }

    public partial class JsonReport
    {
        protected override JsonSerializerSettings JsonSettings => Options.JsonReportSettings;
        protected override SaveOptions OutputOptions => SaveOptions.None;
        protected override Encoding OutputEncoding => Encoding.UTF8;
    }

    public partial class Json
    {
        protected override JsonSerializerSettings JsonSettings => Options.JsonSettings;
        protected override SaveOptions OutputOptions => SaveOptions.None;
        protected override Encoding OutputEncoding => Encoding.UTF8;
    }

    public partial class Csv
    {
        private static string Delimiter => ",";
        protected override SaveOptions OutputOptions => SaveOptions.None;
        protected override Encoding OutputEncoding => Encoding.UTF8;
    }

    public partial class Gpx
    {
        private static XNamespace Namespace => "http://www.topografix.com/GPX/1/0";
        private static string DocumentEncoding => "utf-8";
        private static XNamespace Xsi => "http://www.w3.org/2001/XMLSchema-instance";
        private static string Schema => "http://www.topografix.com/GPX/1/0 http://wwwtopografix.com/GPX/1/0/gpx.xsd";
        private static string Waypoint => "wpt";
        protected override SaveOptions OutputOptions => SaveOptions.None;
        protected override Encoding OutputEncoding => Encoding.UTF8;
    }

    public partial class Kml
    {
        private static XNamespace Namespace => "http://www.opengis.net/kml/2.2";
        private const string DocumentEncoding = "utf-8";
        private static XNamespace Gx => "http://www.google.com/kml/ext/2.2";
        private static string Placemark => "Placemark";
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

    }

    public partial class Xspf
    {
        private static XNamespace Namespace => "http://xspf.org/ns/0/";
        private static string DocumentEncoding => "utf-8";
        private static string Track => "track";
        protected override SaveOptions OutputOptions => SaveOptions.None;
        protected override Encoding OutputEncoding => Encoding.UTF8;
    }
}
