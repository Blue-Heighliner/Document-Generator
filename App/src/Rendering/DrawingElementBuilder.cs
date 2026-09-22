namespace BlueHeighliner.DocumentGenerator.Rendering;

/// <summary>Builds the OpenXML drawing markup for an inline picture.</summary>
internal interface IDrawingElementBuilder
{
    /// <summary>Builds an inline picture drawing, scaled down to fit within <paramref name="maxWidthEmu"/>.</summary>
    /// <param name="relationshipId">The relationship id of the image part to display.</param>
    /// <param name="name">The picture's display name.</param>
    /// <param name="pixelWidth">The image's native width, in pixels.</param>
    /// <param name="pixelHeight">The image's native height, in pixels.</param>
    /// <param name="maxWidthEmu">The maximum display width, in EMUs (914400 per inch).</param>
    /// <returns>The drawing element, ready to place inside a run.</returns>
    Drawing BuildPicture(string relationshipId, string name, int pixelWidth, int pixelHeight, long maxWidthEmu);
}

/// <inheritdoc cref="IDrawingElementBuilder" />
internal sealed class DrawingElementBuilder : IDrawingElementBuilder
{
    private readonly long emuPerPixel = 9525;

    /// <inheritdoc />
    public Drawing BuildPicture(string relationshipId, string name, int pixelWidth, int pixelHeight, long maxWidthEmu)
    {
        long widthEmu = pixelWidth * emuPerPixel;
        long heightEmu = pixelHeight * emuPerPixel;
        if (widthEmu > maxWidthEmu)
        {
            heightEmu = heightEmu * maxWidthEmu / widthEmu;
            widthEmu = maxWidthEmu;
        }

        DocumentFormat.OpenXml.Drawing.Pictures.Picture picture = new(
            new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualPictureProperties(
                new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualDrawingProperties { Id = 1, Name = name },
                new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualPictureDrawingProperties()),
            new DocumentFormat.OpenXml.Drawing.Pictures.BlipFill(
                new DocumentFormat.OpenXml.Drawing.Blip { Embed = relationshipId },
                new DocumentFormat.OpenXml.Drawing.Stretch(new DocumentFormat.OpenXml.Drawing.FillRectangle())),
            new DocumentFormat.OpenXml.Drawing.Pictures.ShapeProperties(
                new DocumentFormat.OpenXml.Drawing.Transform2D(
                    new DocumentFormat.OpenXml.Drawing.Offset { X = 0, Y = 0 },
                    new DocumentFormat.OpenXml.Drawing.Extents { Cx = widthEmu, Cy = heightEmu }),
                new DocumentFormat.OpenXml.Drawing.PresetGeometry(new DocumentFormat.OpenXml.Drawing.AdjustValueList()) { Preset = DocumentFormat.OpenXml.Drawing.ShapeTypeValues.Rectangle }));

        DocumentFormat.OpenXml.Drawing.Wordprocessing.Inline inline = new(
            new DocumentFormat.OpenXml.Drawing.Wordprocessing.Extent { Cx = widthEmu, Cy = heightEmu },
            new DocumentFormat.OpenXml.Drawing.Wordprocessing.EffectExtent { LeftEdge = 0, TopEdge = 0, RightEdge = 0, BottomEdge = 0 },
            new DocumentFormat.OpenXml.Drawing.Wordprocessing.DocProperties { Id = 1, Name = name },
            new DocumentFormat.OpenXml.Drawing.Wordprocessing.NonVisualGraphicFrameDrawingProperties(
                new DocumentFormat.OpenXml.Drawing.GraphicFrameLocks { NoChangeAspect = true }),
            new DocumentFormat.OpenXml.Drawing.Graphic(
                new DocumentFormat.OpenXml.Drawing.GraphicData(picture) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }));

        return new Drawing(inline);
    }
}
