namespace AdventOfCode2025;

using SkiaSharp;
using System.Numerics;

using matthiasffm.Common.Math;

public static class VisualizationUtils
{
    // dump map to a PNG file where every value on the map gets a green 2x2 pixel
    public static void WriteMonochromeMapToPngImage<T>(T[,] map, string path) where T: INumber<T>
    {
        // convert map to image with fat 2x2 pixels

        var height = map.GetLength(0);
        var width  = map.GetLength(1);

        var pixels = new SKColor[height * width * 4];

        var foreColor = new SKColor(50, 200, 50);
        var backColor = new SKColor(0, 0, 0);

        for(var row = 0; row < height; row++)
        {
            for(var col = 0; col < width; col++)
            {
                pixels[row * 4 * width + col * 2] =
                pixels[row * 4 * width + col * 2 + 1] =
                pixels[row * 4 * width + col * 2 + width * 2] =
                pixels[row * 4 * width + col * 2 + width * 2 + 1] = map[row, col] == default ? backColor : foreColor;
            }
        }

        // dump pixels to png file

        var bitmap = new SKBitmap(width * 2, height * 2)
        {
            Pixels = pixels
        };

        using MemoryStream memStream = new();
        using SKManagedWStream wstream = new(memStream);
        bitmap.Encode(wstream, SKEncodedImageFormat.Png, 0);
        byte[] pgnBytes = memStream.ToArray();

        File.WriteAllBytes(path, pgnBytes);
    }

    // dump multicolor map to a PNG file
    public static void WriteMulticolorMapToPngImage((byte Red, byte Green, byte Blue)[,] map, string path)
    {
        var height = map.GetLength(0);
        var width  = map.GetLength(1);

        var pixels = new SKColor[height * width];

        for(var row = 0; row < height; row++)
        {
            for(var col = 0; col < width; col++)
            {
                pixels[row * width + col] = new SKColor(map[row, col].Red, map[row, col].Green, map[row, col].Blue);
            }
        }

        // dump pixels to png file

        var bitmap = new SKBitmap(width, height)
        {
            Pixels = pixels
        };

        using MemoryStream memStream = new();
        using SKManagedWStream wstream = new(memStream);
        bitmap.Encode(wstream, SKEncodedImageFormat.Png, 0);
        byte[] pgnBytes = memStream.ToArray();

        File.WriteAllBytes(path, pgnBytes);
    }

    // dump map to a PNG file where every value on the map gets a green 2x2 pixel
    public static void WriteHeatmapToPngImage(long[,] map, string path)
    {
        // scale map values to 50..255 red

        var minHeat = map.Select((heat, _, _) => Math.Sqrt(heat)).Where(h => h > 0).Min();
        var maxHeat = map.Select((heat, _, _) => Math.Sqrt(heat)).Max();

        // convert map to image with fat 2x2 pixels

        var height = map.GetLength(0);
        var width  = map.GetLength(1);

        var pixels = new SKColor[height * width * 4];

        for(var row = 0; row < height; row++)
        {
            for(var col = 0; col < width; col++)
            {
                var color = map[row, col] > 0 ? 
                                new SKColor((byte)((Math.Sqrt(map[row, col]) - minHeat) * 250.0 / (maxHeat - minHeat) + 50), 50, 80) :
                                0;

                pixels[row * 4 * width + col * 2] =
                pixels[row * 4 * width + col * 2 + 1] =
                pixels[row * 4 * width + col * 2 + width * 2] =
                pixels[row * 4 * width + col * 2 + width * 2 + 1] = color;
            }
        }

        // dump pixels to png file

        var bitmap = new SKBitmap(width * 2, height * 2)
        {
            Pixels = pixels
        };

        using MemoryStream memStream = new();
        using SKManagedWStream wstream = new(memStream);
        bitmap.Encode(wstream, SKEncodedImageFormat.Png, 0);
        byte[] pgnBytes = memStream.ToArray();

        File.WriteAllBytes(path, pgnBytes);
    }
}
