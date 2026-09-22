namespace BlueHeighliner.DocumentGenerator.Tests;

/// <summary>Builds minimal fake image bytes for tests.</summary>
internal static class TestImages
{
    /// <summary>Builds a 10x10 PNG header with no actual pixel data, enough for <see cref="PngDimensionReader"/>.</summary>
    public static ReadOnlyMemory<byte> BuildMinimalPng()
    {
        byte[] png = new byte[24];
        byte[] signature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        signature.CopyTo(png, 0);
        BinaryPrimitives.WriteInt32BigEndian(png.AsSpan(16, 4), 10);
        BinaryPrimitives.WriteInt32BigEndian(png.AsSpan(20, 4), 10);
        return png;
    }
}
