﻿// SpotifyGPX by Simon Field

namespace SpotifyGPX;

public struct Formats
{
    // Time format for console printing of point-song time comparison:
    public static string Console => @"HH\:mm\:ss";

    // ================== //
    // GPX IMPORT FORMATS //
    // ================== //

    // Time format used to interpret your GPX track <time> tags
    public static string GpxInput => @"yyyy-MM-ddTHH\:mm\:ss.fffzzz"; // Can be any UTC offset

    // =================== //
    // JSON IMPORT FORMATS //
    // =================== //

    // Time format used in Spotify-distributed JSONs
    public static string SpotifyFull => @"MM/dd/yyyy HH\:mm\:ss"; // 30 day (full acc data) dump
    public static string SpotifyMini => @"yyyy-MM-dd HH\:mm"; // 5 day (past year) dump

    // ================== //
    // GPX EXPORT FORMATS //
    // ================== //

    // Time format used in the <desc> field of GPX song point (your choice)
    public static string DescriptionPlayedAt => @"yyyy-MM-dd HH\:mm\:ss zzz"; // Can be any UTC offset
    public static string DescriptionTimePlayed => @"hh\:mm\:ss\.fff";

    // Time format used in the <time> field of GPX song point (requires ISO 8601):
    public static string GpxOutput => @"yyyy-MM-ddTHH\:mm\:ssZ"; // Must first be converted to UTC
}
