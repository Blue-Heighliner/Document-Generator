namespace BlueHeighliner.DocumentGenerator.Configuration;

/// <summary>Which page(s) of a document a <see cref="PageContentLine"/> is shown on.</summary>
internal enum PageContentMode
{
    /// <summary>Shown on every page, including the cover page.</summary>
    All,

    /// <summary>Shown only on the document's cover (first) page.</summary>
    Cover,

    /// <summary>Shown on every page except the document's cover (first) page.</summary>
    Body,
}
