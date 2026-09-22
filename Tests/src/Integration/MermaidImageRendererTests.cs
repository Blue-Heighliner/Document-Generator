namespace BlueHeighliner.DocumentGenerator.Tests.Integration;

public sealed class MermaidImageRendererTests
{
    [Fact]
    public async Task Render_ValidDiagram_ReturnsPngImage()
    {
        using HttpClient httpClient = new();
        MermaidImageRenderer renderer = new(httpClient);

        MermaidImage image = await renderer.Render("graph TD\nA-->B", CancellationToken.None);

        Assert.True(image.Data.Length > 0);
        Assert.True(image.Width > 0);
        Assert.True(image.Height > 0);
    }
}
