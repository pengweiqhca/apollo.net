namespace Com.Ctrip.Framework.Apollo.Core.Utils;

public class Properties
{
    private readonly Dictionary<string, string> _dict;

    public Properties() => _dict = new(StringComparer.OrdinalIgnoreCase);

    public Properties(IDictionary<string, string>? dictionary) => _dict = dictionary == null
        ? new(StringComparer.OrdinalIgnoreCase)
        : new(dictionary, StringComparer.OrdinalIgnoreCase);

    public Properties(Properties source) => _dict = new(source.ThrowIfNull()._dict, StringComparer.OrdinalIgnoreCase);

    public Properties(Stream stream)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));

        var dict = JsonSerializer.Deserialize<IDictionary<string, string>>(stream);

        _dict = dict == null ? new(StringComparer.OrdinalIgnoreCase) : new(dict, StringComparer.OrdinalIgnoreCase);
    }

    internal Properties SpecialDelimiter(IReadOnlyCollection<string>? specialDelimiter)
    {
        if (specialDelimiter == null || specialDelimiter.Count < 1) return this;

        var properties = new Properties();

        foreach (var kv in _dict)
        {
            var key = kv.Key;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var delimiter in specialDelimiter)
                key = key.Replace(delimiter, ":");

            properties._dict[key] = kv.Value;
        }

        return properties;
    }

    public bool TryGetProperty(string key, [NotNullWhen(true)] out string? value) => _dict.TryGetValue(key, out value);

    public string? GetProperty(string key)
    {
        _dict.TryGetValue(key, out var result);

        return result;
    }

    public ISet<string> GetPropertyNames() => new HashSet<string>(_dict.Keys);

    public void Store(Stream stream)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));

        JsonSerializer.Serialize(stream, _dict);
    }
}
