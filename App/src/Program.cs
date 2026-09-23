namespace BlueHeighliner.DocumentGenerator;

/// <summary>The command-line entry point: config JSON + markdown in, a <c>.docx</c> file out.</summary>
internal sealed class Program
{
    /// <param name="args">Expected as exactly <c>&lt;config.json&gt; &lt;input.md&gt; &lt;output.docx&gt;</c>.</param>
    /// <returns>0 on success, 1 on failure.</returns>
    internal static async Task<int> Main(string[] args)
    {
        if (args.Length != 3)
        {
            Console.Error.WriteLine("Usage: docgen <config.json> <input.md> <output.docx>");
            return 1;
        }

        string configurationPath = args[0];
        string markdownPath = args[1];
        string outputPath = args[2];

        using HttpClient httpClient = new();
        IConfigurationLoader configurationLoader = new ConfigurationLoader();
        IMarkdownContentTrimmer contentTrimmer = new MarkdownContentTrimmer();
        IHeadingNumberer headingNumberer = new HeadingNumberer();
        IInlineRunWriter inlineRunWriter = new InlineRunWriter();
        ITableConverter tableConverter = new TableConverter();
        IDrawingElementBuilder drawingElementBuilder = new DrawingElementBuilder();
        IMermaidImageRenderer mermaidImageRenderer = new MermaidImageRenderer(httpClient);
        ICoverPageWriter coverPageWriter = new CoverPageWriter();
        ITableOfContentsWriter tableOfContentsWriter = new TableOfContentsWriter();
        IHeaderFooterWriter headerFooterWriter = new HeaderFooterWriter();
        IMarkdownBodyWriter bodyWriter = new MarkdownBodyWriter(headingNumberer, inlineRunWriter, tableConverter, mermaidImageRenderer, drawingElementBuilder);
        IDocumentBuilder documentBuilder = new DocumentBuilder(coverPageWriter, tableOfContentsWriter, headerFooterWriter, bodyWriter);

        try
        {
            DocumentConfiguration configuration = await configurationLoader.Load(configurationPath, CancellationToken.None);
            string rawMarkdown = await File.ReadAllTextAsync(markdownPath);
            string markdown = contentTrimmer.Trim(rawMarkdown, configuration.Exclusion);

            await using FileStream output = File.Create(outputPath);
            await documentBuilder.Generate(configuration, markdown, output, CancellationToken.None);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Failed to generate document: {exception.Message}");
            return 1;
        }

        Console.WriteLine($"Generated '{outputPath}'.");
        return 0;
    }
}
