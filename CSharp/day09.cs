namespace AdventOfCode2025;

using FluentAssertions;
using NUnit.Framework;

using matthiasffm.Common.Collections;

/// <summary>
/// Theme: finding the largest rectangle inside on an movie theater floor
/// </summary>
[TestFixture]
public class Day09
{
    private static (long Row, long Col)[] ParseData(string[] lines)
        => [.. lines.Select(l => (long.Parse(l.Split(',')[1]), long.Parse(l.Split(',')[0])))];

    [Test]
    public void TestSamples()
    {
        var data = new[]
        {
            "7,1",
            "11,1",
            "11,7",
            "9,7",
            "9,5",
            "2,5",
            "2,3",
            "7,3",
        };
        var redTiles = ParseData(data);

        Puzzle1(redTiles).Should().Be(50L);
        Puzzle2(redTiles, false).Should().Be(24L);
    }

    [Test]
    public void TestAocInput()
    {
        var data = FileUtils.ReadAllLines(this);
        var redTiles = ParseData(data);

        Puzzle1(redTiles).Should().Be(4754955192L);
        Puzzle2(redTiles, false).Should().Be(1568849600L);
    }

    // The Elves are redecorating the movie theater by switching out some of the square tiles in the big grid they form. Some of the tiles are
    // red; the Elves would like to find the largest rectangle that uses red tiles for two of its opposite corners. They even have a list of
    // where the red tiles are located in the grid (your puzzle input). You can choose any two red tiles as the opposite corners of your rectangle;
    // your goal is to find the largest rectangle possible.
    //
    // Puzzle == Using two red tiles as opposite corners, what is the largest area of any rectangle you can make?
    private static long Puzzle1((long Row, long Col)[] redTiles)
        => Rectangles(redTiles)
               .Max(r => Size(r.Left, r.Right));

    // The Elves just remembered: they can only switch out tiles that are red or green. So, your rectangle can only include red or green tiles.
    // In your list, every red tile is connected to the red tile before and after it by a straight line of green tiles. The list wraps, so the
    // first red tile is also connected to the last red tile. Tiles that are adjacent in your list will always be on either the same row or the
    // same column. All of the tiles inside this loop of red and green tiles are also green.
    // The rectangle you choose still must have red tiles in opposite corners, but any other tiles it includes must now be red or green. This
    // significantly limits your options.
    // 
    // Puzzle == Using two red tiles as opposite corners, what is the largest area of any rectangle you can make using only red and green tiles?
    private static long Puzzle2((long Row, long Col)[] redTiles, bool dumpMapToImage)
    {
        // The input tiles array forms the perimeter => walk it along and store all vertical lines in list sorted by column
        // (rows have a minimum distance of 1 in the example; to have an easier time later with intersecting stuff all rows are getting doubled
        // which makes center values between rows clean integers)

        var verticalPerimeterEdges = redTiles.Append(redTiles.First())
                                             .Pairs()
                                             .Where(p => p.Item1.Col == p.Item2.Col)
                                             .OrderBy(p => p.Item1.Col)
                                             .Select(p => (p.Item1.Col, Top: Math.Min(p.Item1.Row, p.Item2.Row) * 2, Bottom: Math.Max(p.Item1.Row, p.Item2.Row) * 2))
                                             .ToArray();

        // let all column values and all row values of the input tiles span a grid
        // then compute the center points of all cells in this grid and determine for every center point if its inside or outside the perimeter
        // go from top and then left to right and count intersections => odd/even == inside/outside

        var redTileCols = redTiles.Select(tile => tile.Col).Distinct().Order().ToArray();
        var redTileRows = redTiles.Select(tile => tile.Row * 2).Distinct().Order().ToArray();

        var centerGridIsInside = new Dictionary<(long Row, long Col), bool>();

        for(int row = 0; row < redTileRows.Length - 1; row++)
        {
            var centerRow = (redTileRows[row] + redTileRows[row + 1]) / 2;

            bool isInside = false;
            int i = 0;

            for(int col = 0; col < redTileCols.Length - 1; col++)
            {
                var centerCol = (redTileCols[col] + redTileCols[col + 1]) / 2;

                while(i < verticalPerimeterEdges.Length && verticalPerimeterEdges[i].Col < centerCol)
                {
                    if(verticalPerimeterEdges[i].Top < centerRow && verticalPerimeterEdges[i].Bottom > centerRow)
                    {
                        isInside = !isInside;
                    }
                    i++;
                }

                centerGridIsInside[(centerRow, centerCol)] = isInside;
            }
        }

        // now sort rectangles by size and find first rectangle r where all its inner center points are all inside the perimeter

        foreach(var (corner1, corner2) in Rectangles(redTiles).OrderByDescending(r => Size(r.Left, r.Right)))
        {
            var size = Size(corner1, corner2);

            bool outsideCenterFound = false;

            var top    = Math.Min(corner1.Row, corner2.Row) * 2;
            var bottom = Math.Max(corner1.Row, corner2.Row) * 2;
            var left   = Math.Min(corner1.Col, corner2.Col);
            var right  = Math.Max(corner1.Col, corner2.Col);

            for(int i = 0; i < redTileRows.Length - 1 && !outsideCenterFound; i++)
            {
                var currentCenterRow = (redTileRows[i] + redTileRows[i + 1]) / 2;

                if(currentCenterRow < top)
                {
                    continue;
                }
                else if(currentCenterRow > bottom)
                {
                    break;
                }

                for(int j = 0; j < redTileCols.Length - 1 && !outsideCenterFound; j++)
                {
                    var currentCenterCol = (redTileCols[j] + redTileCols[j + 1]) / 2;

                    if(currentCenterCol < left)
                    {
                        continue;
                    }
                    else if(currentCenterCol > right)
                    {
                        break;
                    }

                    if(centerGridIsInside[(currentCenterRow, currentCenterCol)] == false)
                    {
                        outsideCenterFound = true;
                    }
                }
            }

            if(!outsideCenterFound)
            {
                if(dumpMapToImage)
                {
                    DumpMap(redTiles, redTileCols, redTileRows, centerGridIsInside, corner1, corner2);
                }

                return Size(corner1, corner2);
            }
        }

        return 0L;
    }

    // dump the map as a 1024x1024 png
    private static void DumpMap((long Row, long Col)[] redTiles,
                                long[] redTileCols,
                                long[] redTileRows,
                                Dictionary<(long Row, long Col), bool> centerGridIsInside,
                                (long Row, long Col) corner1,
                                (long Row, long Col) corner2)
    {
        var border = 10;
        var map = new (byte Red, byte Green, byte Blue)[1024 + 2 * border, 1024 + 2 * border];

        var minCol = redTiles.Min(r => r.Col);
        var maxCol = redTiles.Max(r => r.Col);
        var minRow = redTiles.Min(r => r.Row);
        var maxRow = redTiles.Max(r => r.Row);

        // draw inside as green tiles
        for(int i = 0; i < redTileRows.Length - 1; i++)
        {
            var currentCenterRow = (redTileRows[i] + redTileRows[i + 1]) / 2;
            for(int j = 0; j < redTileCols.Length - 1; j++)
            {
                var currentCenterCol = (redTileCols[j] + redTileCols[j + 1]) / 2;
                if(centerGridIsInside[(currentCenterRow, currentCenterCol)])
                {
                    DrawRectangleOnMap(map,
                                       (redTileRows[i] / 2 - minRow) * 1024 / (maxRow - minRow) + border,
                                       (redTileCols[j] - minCol) * 1024 / (maxCol - minCol) + border,
                                       (redTileRows[i + 1] / 2 - minRow) * 1024 / (maxRow - minRow) + border,
                                       (redTileCols[j + 1] - minCol) * 1024 / (maxCol - minCol) + border,
                                       (50, 200, 50));
                }
            }
        }

        // draw biggest rectangle grey
        DrawRectangleOnMap(map,
                           (Math.Min(corner1.Row, corner2.Row) - minRow) * 1024 / (maxRow - minRow) + border,
                           (Math.Min(corner1.Col, corner2.Col) - minCol) * 1024 / (maxCol - minCol) + border,
                           (Math.Max(corner1.Row, corner2.Row) - minRow) * 1024 / (maxRow - minRow) + border,
                           (Math.Max(corner1.Col, corner2.Col) - minCol) * 1024 / (maxCol - minCol) + border,
                           (150, 150, 150));

        // draw red tiles as red dots
        foreach(var (row, col) in redTiles)
        {
            map[(row - minRow) * 1024 / (maxRow - minRow) + border, (col - minCol) * 1024 / (maxCol - minCol) + border] = (200, 50, 50);
        }

        VisualizationUtils.WriteMulticolorMapToPngImage(map, "map.png");
    }

    private static void DrawRectangleOnMap((byte Red, byte Green, byte Blue)[,] map, long top, long left, long bottom, long right, (byte, byte, byte) color)
    {
        for(var y = top; y < bottom; y++)
        {
            for(var x = left; x < right; x++)
            {
                map[y, x] = color;
            }
        }
    }

    // enumerate all rectangles spanned by combining pairs of red tiles
    // ignore all rectangles with height or width == 1: | or -
    private static IEnumerable<((long Row, long Col) Left, (long Row, long Col) Right)> Rectangles((long Row, long Col)[] redTiles)
        => redTiles.TwoCombinations()
                   .Where(p => p.Item1.Row != p.Item2.Row && p.Item1.Col != p.Item2.Col);

    private static long Size((long Row, long Col) edge1, (long Row, long Col) edge2)
        => (Math.Abs(edge1.Row - edge2.Row) + 1) *
           (Math.Abs(edge1.Col - edge2.Col) + 1);
}
