// SpotifyGPX by Simon Field

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace SpotifyGPX.Output;

/// <summary>
/// Provides instructions for serializing data in the source format to a byte array and saving it in the target format.
/// </summary>
/// <typeparam name="T">The source format type</typeparam>
public abstract class SaveableBase<T> : IFileOutput
{
    protected abstract T Document { get; }
    public abstract int Count { get; }

    public void Save(string path)
    {
        byte[] doc = ConvertToBytes();
        File.WriteAllBytes(path, doc);
    }

    protected abstract byte[] ConvertToBytes();
}

/// <summary>
/// Provides instructions for serializing data in the source format to a byte array and saving and/or transforming data in the target format.
/// </summary>
/// <typeparam name="T">The source format type.</typeparam>
public abstract class SaveableAndTransformableBase<T> : SaveableBase<T>, ITransformableOutput
{
    protected abstract SaveOptions OutputOptions { get; }
    protected abstract Encoding OutputEncoding { get; }

    public void TransformAndSave(string name, string xsltPath)
    {
        TransformationResult transformation = TransformationResult.Transform(TransformToXml(), xsltPath);
        string doc = transformation.TransformedDocument.ToString(OutputOptions);
        string outputPath = $"{name}.{transformation.Format}";
        File.WriteAllText(outputPath, doc, transformation.TargetEncoding ?? OutputEncoding);
    }

    protected abstract XDocument TransformToXml();
}

/// <summary>
/// Provides instructions for serializing and transforming JSON data.
/// </summary>
public abstract class JsonSaveable : SaveableAndTransformableBase<List<JObject>>
{
    protected abstract JsonSerializerSettings JsonSettings { get; }

    protected override byte[] ConvertToBytes()
    {
        string doc = JsonConvert.SerializeObject(Document, JsonSettings.Formatting, JsonSettings);
        return OutputEncoding.GetBytes(doc);
    }

    protected override XDocument TransformToXml()
    {
        XElement root = new("Root");

        Document.Select(obj => JsonConvert.DeserializeXNode(obj.ToString(), "Root")?.Root)
            .Where(element => element != null)
            .ToList()
            .ForEach(root.Add);

        return new XDocument(root);
    }
}

/// <summary>
/// Provides instructions for serializing and transforming XML data.
/// </summary>
public abstract class XmlSaveable : SaveableAndTransformableBase<XDocument>
{
    protected override byte[] ConvertToBytes()
    {
        string doc = Document.ToString(OutputOptions);
        return OutputEncoding.GetBytes(doc);
    }

    protected override XDocument TransformToXml()
    {
        return Document;
    }
}

/// <summary>
/// Provides instructions for serializing and transforming TXT data.
/// </summary>
public abstract class TxtSaveable : SaveableAndTransformableBase<string?[]>
{
    protected override byte[] ConvertToBytes()
    {
        string doc = string.Join(Environment.NewLine, Document);
        return OutputEncoding.GetBytes(doc);
    }

    protected override XDocument TransformToXml()
    {
        XElement root = new("Root");

        Document.Select(line => new XElement("Line", line))
            .ToList()
            .ForEach(root.Add);

        return new XDocument(root);
    }
}

/// <summary>
/// Provides instructions for serializing an array of bytes.
/// </summary>
public abstract class ByteSaveable : SaveableBase<byte[]>
{
    protected override byte[] ConvertToBytes()
    {
        return Document;
    }
}

/// <summary>
/// Provides access to the resulting document and settings of the transformation.
/// </summary>
public class TransformationResult
{
    public XDocument TransformedDocument { get; }
    public XmlWriterSettings? TransformationSettings { get; }
    public Encoding? TargetEncoding => TransformationSettings?.Encoding;
    public XmlOutputMethod? TargetOutputMethod => TransformationSettings?.OutputMethod;

    public TransformationResult(XDocument document, XmlWriterSettings? settings)
    {
        TransformedDocument = document;
        TransformationSettings = settings;
    }

    public string Format
    {
        get
        {
            return TargetOutputMethod switch
            {
                XmlOutputMethod.Xml => "xml",
                XmlOutputMethod.Text => "txt",
                XmlOutputMethod.Html => "html",
                XmlOutputMethod.AutoDetect => "xml",
                _ => "xml"
            };
        }
    }

    public static TransformationResult Transform(XDocument serializedData, string xsltPath)
    {
        if (!File.Exists(xsltPath))
        {
            return new TransformationResult(serializedData, null);
        }

        XDocument transformedDocument = new();

        XslCompiledTransform xslt = new();
        xslt.Load(xsltPath);

        // Create an XmlWriter to write the transformed XML directly to the XDocument
        using (XmlWriter xw = transformedDocument.CreateWriter())
        {
            // Perform the transformation
            xslt.Transform(serializedData.CreateReader(), xw);
        }

        return new TransformationResult(transformedDocument, xslt.OutputSettings);
    }
}
