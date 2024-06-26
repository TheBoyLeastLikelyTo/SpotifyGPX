﻿// SpotifyGPX by Simon Field

using SpotifyGPX.Broadcasting;
using System.IO;

namespace SpotifyGPX.Input;

public abstract class FileInputBase : DisposableBase
{
    protected FileInputBase(string path, StringBroadcaster bcaster) : base(bcaster)
    {
        FileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        BCaster.Broadcast($"New file stream opened for reading '{path}'.", Observation.LogLevel.Debug);
        StreamReader = new StreamReader(FileStream);
    }

    /// <summary>
    /// The name of the file format.
    /// </summary>
    protected abstract string FormatName { get; }

    protected override string BroadcasterPrefix => $"INP, {FormatName.ToUpper()}";

    /// <summary>
    /// Serves as the reading stream for a file on the disk.
    /// </summary>
    protected FileStream FileStream { get; private set; }

    /// <summary>
    /// Serves as the stream reader for the file stream, <see cref="FileStream"/>.
    /// </summary>
    protected StreamReader StreamReader { get; private set; }

    /// <summary>
    /// Clears this file's original document contents from memory.
    /// </summary>
    protected abstract void DisposeDocument();

    protected override void DisposeClass()
    {
        StreamReader.Dispose();
        FileStream.Dispose();
        DisposeDocument();
    }
}
