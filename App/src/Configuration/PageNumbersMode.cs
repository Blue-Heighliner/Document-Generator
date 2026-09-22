namespace BlueHeighliner.DocumentGenerator.Configuration;

/// <summary>Whether a page header/footer shows the current page number, and where it's aligned.</summary>
internal enum PageNumbersMode
{
    /// <summary>No page number is shown.</summary>
    None,

    /// <summary>The page number is shown, left-aligned.</summary>
    Left,

    /// <summary>The page number is shown, center-aligned.</summary>
    Center,

    /// <summary>The page number is shown, right-aligned.</summary>
    Right,
}
