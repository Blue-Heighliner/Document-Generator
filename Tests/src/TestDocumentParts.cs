namespace BlueHeighliner.DocumentGenerator.Tests;

/// <summary>Creates in-memory <see cref="MainDocumentPart"/> instances for tests to append content to.</summary>
internal static class TestDocumentParts
{
    /// <summary>Creates a new, empty in-memory Word document and returns its main part.</summary>
    public static MainDocumentPart CreateMainPart()
    {
        MemoryStream stream = new();
        WordprocessingDocument document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);
        MainDocumentPart mainPart = document.AddMainDocumentPart();
        mainPart.Document = new Document(new Body());
        return mainPart;
    }
}
