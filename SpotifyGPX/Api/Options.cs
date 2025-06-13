// SpotifyGPX by Simon Field

namespace SpotifyGPX.Api.Geocoding.MapQuest
{
    public partial class GeocoderMapQuest
    {
        protected override int BatchSize => 100;
        private const string API_KEY = "";
    }
}

namespace SpotifyGPX.Api.Geocoding.Google
{
    public partial class GeocoderGoogle
    {
        protected override int BatchSize => 3000;
        private const string API_KEY = "";
    }
}

namespace SpotifyGPX.Api.SpotifyAPI
{
    public partial class SpotifyEntryMatcher
    {
        private static string CLIENT_ID => "YOUR_CLIENT_ID";
        private static string CLIENT_SECRET => "YOUR_CLIENT_SECRET";
    }
}
