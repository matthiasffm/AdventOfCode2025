namespace AdventOfCode2025;

using SkiaSharp;
using System.Numerics;

public static class FileUtils
{
    public static string[] ReadAllLines(object day)
    {
        return File.ReadAllLines(day.GetType().Name.ToLower() + ".data");
    }

    public static string ReadAllText(object day)
    {
        return File.ReadAllText(day.GetType().Name.ToLower() + ".data");
    }

    public static TResult[] ParseByLine<TResult>(object day, Func<string, int, TResult> converter)
    {
        return ReadAllLines(day).Select((l, i) => converter(l, i))
                                .ToArray();
    }

    public static byte[,] ParseToMatrix(string[] lines, Func<char, int, int, byte> Converter)
    {
        byte[,] matrix = new byte[lines.Length, lines[0].Length];

        for(int row = 0; row < lines.Length; row++)
        {
            for(int col = 0; col < lines[0].Length; col++)
            {
                matrix[row, col] = Converter(lines[row][col], row, col);
            }
        }

        return matrix;
    }

    public static IEnumerable<TResult> ParseMultilinePairs<TResult>(string[] data, Func<(string, string), TResult> PairConverter)
    {
        var pairs = new List<TResult>();

        foreach(var pair in string.Join('#', data).Split("##").Select(p => p.Split('#')))
        {
            pairs.Add(PairConverter((pair[0], pair[1])));
        }

        return pairs;
    }

    public static IEnumerable<IEnumerable<TResult>> ParseMultilineTuples<TResult>(string[] data, Func<string, TResult> ElementConverter)
    {
        return string.Join('#', data)
                     .Split("##", StringSplitOptions.RemoveEmptyEntries)
                     .Select(s => s.Split('#', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(elem => ElementConverter(elem)));
    }

    // dump map to PNG file
    public static void WriteMapToPngImage<T>(T[,] map, string path) where T: INumber<T>
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
}
