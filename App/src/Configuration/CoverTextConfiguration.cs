namespace BlueHeighliner.DocumentGenerator.Configuration;

/// <summary>Styled text shown on the cover page (the title or subtitle).</summary>
internal sealed record CoverTextConfiguration
{
    /// <summary>The text, one entry per line. Defaults to no lines.</summary>
    public IReadOnlyList<string> Lines { get; init; } = [];

    /// <summary>The font size, in points. Defaults to 36.</summary>
    public double Size { get; init; } = 36;

    /// <summary>Whether the text is bold. Defaults to <see langword="false" />.</summary>
    public bool Bold { get; init; }

    /// <summary>Whether the text is italic. Defaults to <see langword="false" />.</summary>
    public bool Italic { get; init; }

    /// <summary>
    /// The font color, as a hex RGB string (e.g. <c>"1F2937"</c> or <c>"#1F2937"</c>). Defaults to
    /// black (<c>"#000000"</c>).
    /// </summary>
    public string Color { get; init; } = "#000000";
}

/// <summary>
/// Deserializes a <see cref="CoverTextConfiguration"/> from either a plain JSON array of strings
/// (shorthand for just setting <see cref="CoverTextConfiguration.Lines"/>, leaving every other
/// property at <paramref name="defaultValue"/>) or a full object, and serializes it back as an
/// object. Any property the object omits also falls back to <paramref name="defaultValue"/>, so a
/// partial override (e.g. just <c>italic</c>) doesn't lose the rest of that default styling.
/// </summary>
/// <param name="defaultValue">The value used for properties not present in the JSON.</param>
internal abstract class CoverTextConfigurationConverter(CoverTextConfiguration defaultValue) : JsonConverter<CoverTextConfiguration>
{
    /// <inheritdoc />
    public override CoverTextConfiguration Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            List<string> shorthandLines = JsonSerializer.Deserialize<List<string>>(ref reader, options) ?? [];
            return defaultValue with { Lines = shorthandLines };
        }

        using JsonDocument document = JsonDocument.ParseValue(ref reader);
        JsonElement root = document.RootElement;

        return defaultValue with
        {
            Lines = root.TryGetProperty("lines", out JsonElement lines) ? lines.Deserialize<List<string>>(options) ?? [] : defaultValue.Lines,
            Size = root.TryGetProperty("size", out JsonElement size) ? size.GetDouble() : defaultValue.Size,
            Bold = root.TryGetProperty("bold", out JsonElement bold) ? bold.GetBoolean() : defaultValue.Bold,
            Italic = root.TryGetProperty("italic", out JsonElement italic) ? italic.GetBoolean() : defaultValue.Italic,
            Color = root.TryGetProperty("color", out JsonElement color) ? color.GetString() ?? defaultValue.Color : defaultValue.Color,
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, CoverTextConfiguration value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("lines");
        JsonSerializer.Serialize(writer, value.Lines, options);
        writer.WriteNumber("size", value.Size);
        writer.WriteBoolean("bold", value.Bold);
        writer.WriteBoolean("italic", value.Italic);
        writer.WriteString("color", value.Color);
        writer.WriteEndObject();
    }
}

/// <summary>Deserializes the <c>title</c> key, defaulting to size 36, bold.</summary>
internal sealed class TitleConfigurationConverter() : CoverTextConfigurationConverter(new CoverTextConfiguration { Bold = true });

/// <summary>Deserializes the <c>subtitle</c> key, defaulting to size 20.</summary>
internal sealed class SubtitleConfigurationConverter() : CoverTextConfigurationConverter(new CoverTextConfiguration { Size = 20 });
