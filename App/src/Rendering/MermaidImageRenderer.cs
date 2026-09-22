namespace BlueHeighliner.DocumentGenerator.Rendering;

/// <summary>A rendered mermaid diagram image.</summary>
internal sealed record MermaidImage
{
    /// <summary>The PNG image bytes.</summary>
    public required ReadOnlyMemory<byte> Data { get; init; }

    /// <summary>The image width, in pixels.</summary>
    public required int Width { get; init; }

    /// <summary>The image height, in pixels.</summary>
    public required int Height { get; init; }
}

/// <summary>Renders a mermaid diagram definition into a PNG image.</summary>
internal interface IMermaidImageRenderer
{
    /// <summary>Renders <paramref name="diagram"/> into a PNG image.</summary>
    /// <param name="diagram">The mermaid diagram source text.</param>
    /// <param name="cancellation">A token used to cancel the render.</param>
    /// <returns>The rendered image.</returns>
    Task<MermaidImage> Render(string diagram, CancellationToken cancellation);
}

/// <summary>
/// Renders mermaid diagrams via the <see href="https://mermaid.ink">mermaid.ink</see> hosted
/// rendering service, so no local diagram-rendering engine needs to be bundled with this
/// self-contained executable.
/// </summary>
internal sealed class MermaidImageRenderer(HttpClient httpClient) : IMermaidImageRenderer
{
    /// <inheritdoc />
    public async Task<MermaidImage> Render(string diagram, CancellationToken cancellation)
    {
        string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(diagram)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        using HttpResponseMessage response = await httpClient.GetAsync($"https://mermaid.ink/img/{encoded}?type=png", cancellation);
        response.EnsureSuccessStatusCode();
        byte[] data = await response.Content.ReadAsByteArrayAsync(cancellation);
        (int width, int height) = PngDimensionReader.Read(data);
        return new MermaidImage { Data = data, Width = width, Height = height };
    }
}
