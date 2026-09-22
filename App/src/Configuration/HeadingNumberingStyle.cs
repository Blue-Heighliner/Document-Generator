namespace BlueHeighliner.DocumentGenerator.Configuration;

/// <summary>The numbering style applied to a heading level.</summary>
internal enum HeadingNumberingStyle
{
    /// <summary>No numbering.</summary>
    None,

    /// <summary>Arabic decimal numbering (1, 2, 3, ...).</summary>
    Decimal,

    /// <summary>Uppercase Roman numeral numbering (I, II, III, ...).</summary>
    UpperRoman,

    /// <summary>Lowercase Roman numeral numbering (i, ii, iii, ...).</summary>
    LowerRoman,

    /// <summary>Uppercase alphabetic numbering (A, B, C, ..., Z, AA, ...).</summary>
    UpperLetter,

    /// <summary>Lowercase alphabetic numbering (a, b, c, ..., z, aa, ...).</summary>
    LowerLetter,
}

/// <summary>Formatting for <see cref="HeadingNumberingStyle"/> values.</summary>
internal static class HeadingNumberingStyleFormatting
{
    extension(HeadingNumberingStyle style)
    {
        /// <summary>Formats <paramref name="number"/> according to this numbering style.</summary>
        /// <param name="number">The 1-based counter value to format.</param>
        /// <returns>The formatted number text.</returns>
        public string Format(int number) => style switch
        {
            HeadingNumberingStyle.None => string.Empty,
            HeadingNumberingStyle.Decimal => number.ToString(CultureInfo.InvariantCulture),
            HeadingNumberingStyle.UpperRoman => number.ToRoman(),
            HeadingNumberingStyle.LowerRoman => number.ToRoman().ToLowerInvariant(),
            HeadingNumberingStyle.UpperLetter => number.ToAlphabetic(),
            HeadingNumberingStyle.LowerLetter => number.ToAlphabetic().ToLowerInvariant(),
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, "Unknown heading numbering style."),
        };
    }
}
