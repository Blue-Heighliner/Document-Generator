namespace BlueHeighliner.DocumentGenerator.Configuration;

/// <summary>
/// A single line of page header/footer content, optionally restricted to only the cover page or
/// only non-cover pages. In JSON, a plain string is shorthand for a line shown on every page (mode
/// <see cref="PageContentMode.All"/>); an object form (<c>{ "line": "...", "mode": "..." }</c>) is
/// needed to restrict it to <see cref="PageContentMode.Cover"/> or <see cref="PageContentMode.Body"/>.
/// </summary>
[JsonConverter(typeof(PageContentLineConverter))]
internal sealed record PageContentLine
{
    /// <summary>The line's text.</summary>
    public required string Line { get; init; }

    /// <summary>Which page(s) this line is shown on. Defaults to <see cref="PageContentMode.All"/>.</summary>
    public PageContentMode Mode { get; init; } = PageContentMode.All;
}

/// <summary>
/// Deserializes a <see cref="PageContentLine"/> from either a plain JSON string (shorthand for
/// <see cref="PageContentMode.All"/>) or a <c>{ "line": "...", "mode": "..." }</c> object, and
/// serializes it back the same way.
/// </summary>
internal sealed class PageContentLineConverter : JsonConverter<PageContentLine>
{
    /// <inheritdoc />
    public override PageContentLine Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return new PageContentLine { Line = reader.GetString() ?? string.Empty };
        }

        using JsonDocument document = JsonDocument.ParseValue(ref reader);
        JsonElement root = document.RootElement;
        if (!root.TryGetProperty("line", out JsonElement lineElement))
        {
            throw new JsonException("A page content line object must have a \"line\" property.");
        }

        PageContentMode mode = root.TryGetProperty("mode", out JsonElement modeElement)
            ? modeElement.Deserialize<PageContentMode>(options)
            : PageContentMode.All;

        return new PageContentLine { Line = lineElement.GetString() ?? string.Empty, Mode = mode };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, PageContentLine value, JsonSerializerOptions options)
    {
        if (value.Mode == PageContentMode.All)
        {
            writer.WriteStringValue(value.Line);
            return;
        }

        writer.WriteStartObject();
        writer.WriteString("line", value.Line);
        writer.WritePropertyName("mode");
        JsonSerializer.Serialize(writer, value.Mode, options);
        writer.WriteEndObject();
    }
}
