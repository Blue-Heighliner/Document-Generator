namespace BlueHeighliner.DocumentGenerator.Configuration;

/// <summary>Loads a <see cref="DocumentConfiguration"/> from a JSON file.</summary>
internal interface IConfigurationLoader
{
    /// <summary>Reads and deserializes the configuration file at <paramref name="path"/>.</summary>
    /// <param name="path">The path to the JSON configuration file.</param>
    /// <param name="cancellation">A token used to cancel the read.</param>
    /// <returns>The deserialized configuration.</returns>
    /// <exception cref="InvalidOperationException">The file did not contain a valid configuration.</exception>
    Task<DocumentConfiguration> Load(string path, CancellationToken cancellation);
}

/// <inheritdoc cref="IConfigurationLoader" />
internal sealed class ConfigurationLoader : IConfigurationLoader
{
    private readonly JsonSerializerOptions options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    /// <inheritdoc />
    public async Task<DocumentConfiguration> Load(string path, CancellationToken cancellation)
    {
        await using FileStream stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<DocumentConfiguration>(stream, options, cancellation)
            ?? throw new InvalidOperationException($"Configuration file '{path}' did not contain a valid document configuration.");
    }
}
