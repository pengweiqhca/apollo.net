using Com.Ctrip.Framework.Apollo.Exceptions;
using System.Xml;

namespace Com.Ctrip.Framework.Apollo;

public class NodeReplaceSectionBuilder : ApolloConfigurationBuilder
{
    private string? _key;

    public override void Initialize(string name, NameValueCollection config)
    {
        base.Initialize(name, config.ThrowIfNull());

        _key = config["key"];
    }

    public override XmlNode ProcessRawXml(XmlNode rawXml)
    {
        rawXml.ThrowIfNull();

        if (string.IsNullOrWhiteSpace(_key)) _key = rawXml.Name;

        if (!GetConfig().TryGetProperty(_key!, out var xml) || string.IsNullOrWhiteSpace(xml))
            return base.ProcessRawXml(rawXml);

        var doc = new XmlDocument();

        try
        {
            using var reader = XmlReader.Create(xml);

            doc.Load(reader);
        }
        catch (Exception ex)
        {
            throw new ApolloConfigException($"Can't parse xml of `{_key}` value", ex);
        }

        return base.ProcessRawXml(doc.DocumentElement!);
    }
}
