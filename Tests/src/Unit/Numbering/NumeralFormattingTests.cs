namespace BlueHeighliner.DocumentGenerator.Tests.Unit.Numbering;

public sealed class NumeralFormattingTests
{
    [Theory]
    [InlineData(1, "I")]
    [InlineData(4, "IV")]
    [InlineData(9, "IX")]
    [InlineData(40, "XL")]
    [InlineData(1994, "MCMXCIV")]
    [InlineData(3999, "MMMCMXCIX")]
    public void ToRoman_FormatsExpectedNumeral(int number, string expected)
    {
        Assert.Equal(expected, number.ToRoman());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4000)]
    public void ToRoman_OutOfRange_Throws(int number)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => number.ToRoman());
    }

    [Theory]
    [InlineData(1, "A")]
    [InlineData(26, "Z")]
    [InlineData(27, "AA")]
    [InlineData(52, "AZ")]
    [InlineData(53, "BA")]
    [InlineData(702, "ZZ")]
    [InlineData(703, "AAA")]
    public void ToAlphabetic_FormatsExpectedNumeral(int number, string expected)
    {
        Assert.Equal(expected, number.ToAlphabetic());
    }

    [Fact]
    public void ToAlphabetic_LessThanOne_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => 0.ToAlphabetic());
    }
}
