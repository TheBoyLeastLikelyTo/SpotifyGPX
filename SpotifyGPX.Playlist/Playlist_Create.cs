﻿// SpotifyGPX by Simon Field

using SpotifyGPX.Options;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace SpotifyGPX.Playlist;

public class XSPF
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
            artist.InnerText = Options.Playlist.Tag(entry, 1);
            track.AppendChild(artist);

            // Set the title of the track to the song name
            XmlElement title = document.CreateElement("title");
            title.InnerText = Options.Playlist.Tag(entry, 2);
            track.AppendChild(title);

            // Set the annotation of the song to the end time
            XmlElement annotation = document.CreateElement("annotation");
            annotation.InnerText = Options.Playlist.Tag(entry, 3);
            track.AppendChild(annotation);

            // Set the duration of the song to the amount of time it was listened to
            XmlElement duration = document.CreateElement("duration");
            duration.InnerText = Options.Playlist.Tag(entry, 4);
            track.AppendChild(duration);
        }

        return document;
    }
}
