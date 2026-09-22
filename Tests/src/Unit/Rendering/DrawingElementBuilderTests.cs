namespace BlueHeighliner.DocumentGenerator.Tests.Unit.Rendering;

public sealed class DrawingElementBuilderTests
{
    private readonly DrawingElementBuilder builder = new();

    [Fact]
    public void BuildPicture_WithinMaxWidth_UsesNativeSize()
    {
        Drawing drawing = builder.BuildPicture("rId1", "Picture", pixelWidth: 100, pixelHeight: 50, maxWidthEmu: 5_486_400);

        DocumentFormat.OpenXml.Drawing.Wordprocessing.Extent extent = drawing.Descendants<DocumentFormat.OpenXml.Drawing.Wordprocessing.Extent>().Single();
        Assert.Equal(100 * 9525L, extent.Cx!.Value);
        Assert.Equal(50 * 9525L, extent.Cy!.Value);
    }

    [Fact]
    public void BuildPicture_ExceedsMaxWidth_ScalesDownPreservingAspectRatio()
    {
        long maxWidthEmu = 1_000_000;
        Drawing drawing = builder.BuildPicture("rId1", "Picture", pixelWidth: 1000, pixelHeight: 500, maxWidthEmu: maxWidthEmu);

        DocumentFormat.OpenXml.Drawing.Wordprocessing.Extent extent = drawing.Descendants<DocumentFormat.OpenXml.Drawing.Wordprocessing.Extent>().Single();
        Assert.Equal(maxWidthEmu, extent.Cx!.Value);
        Assert.Equal(maxWidthEmu / 2, extent.Cy!.Value);
    }

    [Fact]
    public void BuildPicture_ReferencesGivenRelationshipId()
    {
        Drawing drawing = builder.BuildPicture("rId42", "Picture", pixelWidth: 10, pixelHeight: 10, maxWidthEmu: 5_486_400);

        DocumentFormat.OpenXml.Drawing.Blip blip = drawing.Descendants<DocumentFormat.OpenXml.Drawing.Blip>().Single();
        Assert.Equal("rId42", blip.Embed!.Value);
    }
}
