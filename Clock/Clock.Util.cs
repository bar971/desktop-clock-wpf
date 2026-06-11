using System.Text;
using System.Windows.Markup;
using System.Xml;

namespace Clock;

public partial class MainWindow
{
    /// <summary>
    /// Serializes the specified object.
    /// </summary>
    /// <param name="toSerialize">Object to serialize.</param>
    /// <returns>The object serialized to XAML.</returns>
    private static string Serialize(object toSerialize)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            ConformanceLevel = ConformanceLevel.Fragment
        };

        var sb = new StringBuilder();
        using (XmlWriter writer = XmlWriter.Create(sb, settings))
        {
            var manager = new XamlDesignerSerializationManager(writer)
            {
                XamlWriterMode = XamlWriterMode.Expression
            };
            XamlWriter.Save(toSerialize, manager);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Deserializes an object from XAML.
    /// </summary>
    /// <param name="xamlText">The XAML text.</param>
    /// <returns>The deserialized object.</returns>
    /// <exception cref="XmlException">Thrown if the serialized text is not well formed XML.</exception>
    /// <exception cref="XamlParseException">Thrown if unable to deserialize from XAML.</exception>
    private static object Deserialize(string xamlText)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xamlText);
        return XamlReader.Load(new XmlNodeReader(doc));
    }
}
