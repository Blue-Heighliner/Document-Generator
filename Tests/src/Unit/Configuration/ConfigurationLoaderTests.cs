namespace BlueHeighliner.DocumentGenerator.Tests.Unit.Configuration;

public sealed class ConfigurationLoaderTests
{
    private readonly ConfigurationLoader loader = new();

    [Fact]
    public async Task Load_DeserializesAllFields()
    {
        string json = """
        {
          "title": { "lines": ["Line 1", "Line 2"], "size": 40, "bold": true, "italic": true, "color": "#111111" },
          "subtitle": { "lines": ["Sub"], "size": 18, "bold": false, "italic": true, "color": "#222222" },
          "exclusion": true,
          "heading1": {
            "size": 24,
            "bold": true,
            "italic": true,
            "color": "#111111",
            "numbering": "decimal",
            "numberingPrefix": false,
            "spaceBefore": 12,
            "spaceAfter": 6,
            "tableOfContents": true,
            "pageBreakAfter": true
          },
          "header": {
            "left": ["Header"],
            "right": [{ "line": "Draft", "mode": "cover" }],
            "pageNumbers": "left"
          },
          "footer": {
            "center": [{ "line": "Confidential", "mode": "body" }],
            "pageNumbers": "right"
          }
        }
        """;

        string path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, json);
            DocumentConfiguration configuration = await loader.Load(path, CancellationToken.None);

            Assert.Equal(["Line 1", "Line 2"], configuration.Title.Lines);
            Assert.Equal(40, configuration.Title.Size);
            Assert.True(configuration.Title.Bold);
            Assert.True(configuration.Title.Italic);
            Assert.Equal("#111111", configuration.Title.Color);

            Assert.Equal(["Sub"], configuration.Subtitle.Lines);
            Assert.Equal(18, configuration.Subtitle.Size);
            Assert.False(configuration.Subtitle.Bold);
            Assert.True(configuration.Subtitle.Italic);
            Assert.Equal("#222222", configuration.Subtitle.Color);

            Assert.True(configuration.Exclusion);

            HeadingConfiguration heading = configuration.Heading1;
            Assert.Equal(24, heading.Size);
            Assert.True(heading.Bold);
            Assert.True(heading.Italic);
            Assert.Equal("#111111", heading.Color);
            Assert.Equal(HeadingNumberingStyle.Decimal, heading.Numbering);
            Assert.False(heading.NumberingPrefix);
            Assert.True(heading.TableOfContents);
            Assert.True(heading.PageBreakAfter);

            Assert.Equal("Header", configuration.Header.Left[0].Line);
            Assert.Equal(PageContentMode.All, configuration.Header.Left[0].Mode);
            Assert.Equal("Draft", configuration.Header.Right[0].Line);
            Assert.Equal(PageContentMode.Cover, configuration.Header.Right[0].Mode);
            Assert.Equal(PageNumbersMode.Left, configuration.Header.PageNumbers);

            Assert.Equal("Confidential", configuration.Footer.Center[0].Line);
            Assert.Equal(PageContentMode.Body, configuration.Footer.Center[0].Mode);
            Assert.Equal(PageNumbersMode.Right, configuration.Footer.PageNumbers);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task Load_TitleAsRawStringArray_IsShorthandForJustLines()
    {
        string json = """{ "title": ["My Document"], "subtitle": ["A Subtitle"] }""";

        string path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, json);
            DocumentConfiguration configuration = await loader.Load(path, CancellationToken.None);

            Assert.Equal(["My Document"], configuration.Title.Lines);
            Assert.True(configuration.Title.Bold);
            Assert.Equal(36, configuration.Title.Size);

            Assert.Equal(["A Subtitle"], configuration.Subtitle.Lines);
            Assert.False(configuration.Subtitle.Bold);
            Assert.Equal(20, configuration.Subtitle.Size);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task Load_TitleWithOnlyLines_PreservesTheTitlesOtherDefaults()
    {
        string json = """{ "title": { "lines": ["My Document"] } }""";

        string path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, json);
            DocumentConfiguration configuration = await loader.Load(path, CancellationToken.None);

            Assert.Equal(["My Document"], configuration.Title.Lines);
            Assert.True(configuration.Title.Bold);
            Assert.Equal(36, configuration.Title.Size);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task Load_MissingFile_Throws()
    {
        await Assert.ThrowsAnyAsync<Exception>(() => loader.Load(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()), CancellationToken.None));
    }

    [Fact]
    public async Task Load_EmptyObject_AppliesDefaultsToEveryField()
    {
        string path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, "{}");
            DocumentConfiguration configuration = await loader.Load(path, CancellationToken.None);

            Assert.Empty(configuration.Title.Lines);
            Assert.Equal(36, configuration.Title.Size);
            Assert.True(configuration.Title.Bold);
            Assert.False(configuration.Title.Italic);
            Assert.Equal("#000000", configuration.Title.Color);

            Assert.Empty(configuration.Subtitle.Lines);
            Assert.Equal(20, configuration.Subtitle.Size);
            Assert.False(configuration.Subtitle.Bold);
            Assert.False(configuration.Subtitle.Italic);
            Assert.Equal("#000000", configuration.Subtitle.Color);

            Assert.False(configuration.Exclusion);

            (int Level, double Size, bool TableOfContents)[] expected = [(1, 24, true), (2, 20, true), (3, 16, false), (4, 14, false), (5, 12, false), (6, 11, false)];
            foreach ((int level, double size, bool tableOfContents) in expected)
            {
                HeadingConfiguration heading = configuration.GetHeading(level);
                Assert.Equal(size, heading.Size);
                Assert.True(heading.Bold);
                Assert.False(heading.Italic);
                Assert.Equal("#000000", heading.Color);
                Assert.Equal(HeadingNumberingStyle.None, heading.Numbering);
                Assert.Equal(tableOfContents, heading.TableOfContents);
                Assert.False(heading.PageBreakAfter);
            }

            Assert.Empty(configuration.Header.Left);
            Assert.Empty(configuration.Header.Center);
            Assert.Empty(configuration.Header.Right);
            Assert.Equal(PageNumbersMode.None, configuration.Header.PageNumbers);
            Assert.Empty(configuration.Footer.Left);
            Assert.Empty(configuration.Footer.Center);
            Assert.Empty(configuration.Footer.Right);
            Assert.Equal(PageNumbersMode.None, configuration.Footer.PageNumbers);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task Load_HeadingWithOnlyOneField_AppliesLevelDefaultsToTheRest()
    {
        string json = """{ "heading1": { "italic": true } }""";

        string path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, json);
            DocumentConfiguration configuration = await loader.Load(path, CancellationToken.None);

            HeadingConfiguration heading = configuration.Heading1;
            Assert.True(heading.Italic);
            Assert.True(heading.Bold);
            Assert.Equal(24, heading.Size);
            Assert.Equal("#000000", heading.Color);
            Assert.Equal(HeadingNumberingStyle.None, heading.Numbering);
            Assert.False(heading.NumberingPrefix);
            Assert.Equal(24, heading.SpaceBefore);
            Assert.Equal(12, heading.SpaceAfter);
            Assert.True(heading.TableOfContents);
            Assert.False(heading.PageBreakAfter);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task Load_DifferentHeadingLevels_MapToTheirOwnProperty()
    {
        string json = """{ "heading2": { "italic": true }, "heading6": { "size": 9 } }""";

        string path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, json);
            DocumentConfiguration configuration = await loader.Load(path, CancellationToken.None);

            Assert.True(configuration.Heading2.Italic);
            Assert.Equal(9, configuration.Heading6.Size);
            Assert.False(configuration.Heading1.Italic);
            Assert.Equal(24, configuration.Heading1.Size);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task Load_RawStringLine_DefaultsModeToAll()
    {
        string json = """{ "header": { "left": ["Header"] } }""";

        string path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, json);
            DocumentConfiguration configuration = await loader.Load(path, CancellationToken.None);

            Assert.Equal("Header", configuration.Header.Left[0].Line);
            Assert.Equal(PageContentMode.All, configuration.Header.Left[0].Mode);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task Load_LineObjectWithNoMode_DefaultsModeToAll()
    {
        string json = """{ "header": { "left": [{ "line": "Header" }] } }""";

        string path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, json);
            DocumentConfiguration configuration = await loader.Load(path, CancellationToken.None);

            Assert.Equal("Header", configuration.Header.Left[0].Line);
            Assert.Equal(PageContentMode.All, configuration.Header.Left[0].Mode);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task Load_LineObjectWithoutLineProperty_Throws()
    {
        string json = """{ "header": { "left": [{ "mode": "cover" }] } }""";

        string path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, json);
            await Assert.ThrowsAsync<JsonException>(() => loader.Load(path, CancellationToken.None));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
