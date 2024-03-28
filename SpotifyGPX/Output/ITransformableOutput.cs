// SpotifyGPX by Simon Field

namespace SpotifyGPX.Output;

/// <summary>
/// Interfaces with formats sharing the ability to transform XML using an XSLT stylesheet.
/// </summary>
public interface ITransformableOutput
{
    /// <summary>
    /// Transform this file to an XML document, and then save it to the disk.
    /// </summary>
    /// <param name="name">The name of this file, without the extension.</param>
    /// <param name="xsltPath">The path to an XSLT stylesheet that should be used in the transformation to XML.</param>
    void TransformAndSave(string name, string xsltPath);
}
