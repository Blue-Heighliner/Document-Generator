namespace BlueHeighliner.DocumentGenerator.Rendering;

/// <summary>Reads a PNG image's pixel dimensions directly from its header, without decoding it.</summary>
internal static class PngDimensionReader
{
    private static readonly byte[] signature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    /// <summary>Reads the width and height from a PNG image's <c>IHDR</c> chunk.</summary>
    /// <param name="png">The PNG image bytes.</param>
    /// <returns>The image's (width, height) in pixels.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="png"/> is not a valid PNG image.</exception>
    public static (int Width, int Height) Read(ReadOnlySpan<byte> png)
    {
        if (png.Length < 24 || !png[..8].SequenceEqual(signature))
        {
            throw new InvalidOperationException("Data is not a valid PNG image.");
        }

        int width = BinaryPrimitives.ReadInt32BigEndian(png.Slice(16, 4));
        int height = BinaryPrimitives.ReadInt32BigEndian(png.Slice(20, 4));
        return (width, height);
    }
}
