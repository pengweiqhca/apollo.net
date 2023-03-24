using System.Globalization;

namespace Com.Ctrip.Framework.Apollo.ConfigAdapter;

internal sealed class JsonConfigurationParser
{
    private JsonConfigurationParser() { }

    private readonly IDictionary<string, string> _data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    private readonly Stack<string> _context = new();

    public static IDictionary<string, string> Parse(string input) => new JsonConfigurationParser().ParseStream(input);

    private IDictionary<string, string> ParseStream(string input)
    {
        var jsonDocumentOptions = new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        using (var doc = JsonDocument.Parse(input, jsonDocumentOptions))
        {
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                throw new FormatException($"Top-level JSON element must be an object. Instead, '{doc.RootElement.ValueKind}' was found.");

            VisitObjectElement(doc.RootElement);
        }

        return _data;
    }

    private void VisitObjectElement(JsonElement element)
    {
        var isEmpty = true;

        foreach (var property in element.EnumerateObject())
        {
            isEmpty = false;
            EnterContext(property.Name);
            VisitValue(property.Value);
            ExitContext();
        }

        SetNullIfElementIsEmpty(isEmpty);
    }

    private void VisitArrayElement(JsonElement element)
    {
        var index = 0;

        foreach (var arrayElement in element.EnumerateArray())
        {
            EnterContext(index.ToString(CultureInfo.InvariantCulture));
            VisitValue(arrayElement);
            ExitContext();
            index++;
        }

        SetNullIfElementIsEmpty(isEmpty: index == 0);
    }

    private void SetNullIfElementIsEmpty(bool isEmpty)
    {
        if (isEmpty && _context.Count > 0)
            _data[_context.Peek()] = string.Empty;
    }

    private void VisitValue(JsonElement value)
    {
        Debug.Assert(_context.Count > 0);

        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                VisitObjectElement(value);
                break;

            case JsonValueKind.Array:
                VisitArrayElement(value);
                break;

            case JsonValueKind.Number:
            case JsonValueKind.String:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                var key = _context.Peek();

                if (_data.ContainsKey(key))
                    throw new FormatException($"A duplicate key '{key}' was found.");

                _data[key] = value.ToString();
                break;

            default:
                throw new FormatException($"Unsupported JSON token '{value.ValueKind}' was found.");
        }
    }

    private void EnterContext(string context) =>
        _context.Push(_context.Count > 0 ? _context.Peek() + ":" + context : context);

    private void ExitContext() => _context.Pop();
}
