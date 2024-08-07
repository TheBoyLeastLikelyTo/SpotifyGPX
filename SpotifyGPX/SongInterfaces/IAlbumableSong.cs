﻿// SpotifyGPX by Simon Field

namespace SpotifyGPX.SongInterfaces;

/// <summary>
/// Provides access to song album names.
/// </summary>
public interface IAlbumableSong
{
    string? Song_Album { get; }
    string SongAlbum => Song_Album ?? string.Empty;
}
