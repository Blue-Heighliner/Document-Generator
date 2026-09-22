namespace BlueHeighliner.DocumentGenerator.Tests.Unit.Rendering;

public sealed class PngDimensionReaderTests
{
    [Fact]
    public void Read_ValidPng_ReturnsWidthAndHeight()
    {
        byte[] png = BuildMinimalPng(width: 640, height: 480);

        (int width, int height) = PngDimensionReader.Read(png);

        Assert.Equal(640, width);
        Assert.Equal(480, height);
    }

    [Fact]
    public void Read_InvalidSignature_Throws()
    {
        byte[] invalid = new byte[24];
        Assert.Throws<InvalidOperationException>(() => PngDimensionReader.Read(invalid));
    }

    [Fact]
    public void Read_TooShort_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => PngDimensionReader.Read(new byte[4]));
    }

    private static byte[] BuildMinimalPng(int width, int height)
    {
        byte[] png = new byte[24];
        byte[] signature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        signature.CopyTo(png, 0);
        BinaryPrimitives.WriteInt32BigEndian(png.AsSpan(16, 4), width);
        BinaryPrimitives.WriteInt32BigEndian(png.AsSpan(20, 4), height);
        return png;
    }
}
