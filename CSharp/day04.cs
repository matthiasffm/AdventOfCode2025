namespace AdventOfCode2025;

using FluentAssertions;
using NUnit.Framework;

using matthiasffm.Common.Math;

/// <summary>
/// Theme: removing paper rolls to make room for the forklifts
/// </summary>
[TestFixture]
public class Day04
{
    private static byte[,] ParseData(string[] data) =>
        FileUtils.ParseToMatrix(data, (ch, row, col) => (byte)(ch == '@' ? 1 : 0));

    [Test]
    public void TestSamples()
    {
        var data = new[]
        {
            "..@@.@@@@.",
            "@@@.@.@.@@",
            "@@@@@.@.@@",
            "@.@@@@..@.",
            "@@.@@@@.@@",
            ".@@@@@@@.@",
            ".@.@.@.@@@",
            "@.@@@.@@@@",
            ".@@@@@@@@.",
            "@.@.@@@.@.",
        };
        var map = ParseData(data);

        Puzzle1(map).Should().Be(13);
        Puzzle2(map, false).Should().Be(43);
    }

    [Test]
    public void TestAocInput()
    {
        var data = FileUtils.ReadAllLines(this);
        var map  = ParseData(data);

        Puzzle1(map).Should().Be(1428);
        Puzzle2(map, false).Should().Be(8936);
    }

    // All of the Elves forklifts are busy moving rolls of paper around. You need to help them optimize the work the forklifts are doing.
    // The rolls of paper (@) are arranged on a large grid; the Elves even have a helpful diagram (your puzzle input) indicating where everything
    // is located. The forklifts can only access a roll of paper if there are fewer than four rolls of paper in the eight adjacent positions. If
    // you can figure out which rolls of paper the forklifts can access, they'll spend less time looking and more time breaking down the wall to
    // the cafeteria.
    //
    // Puzzle == Consider your complete diagram of the paper roll locations. How many rolls of paper can be accessed by a forklift?
    private static int Puzzle1(byte[,] map)
        => CreateMapWithNeighborCount(map).Where((neighborCount, row, col) => map[row, col] > 0 && neighborCount < 4)
                                          .Sum(_ => 1);

    // Now, the Elves just need help accessing as much of the paper as they can. Once a roll of paper can be accessed by a forklift, it can be
    // removed. Once a roll of paper is removed, the forklifts might be able to access more rolls of paper, which they might also be able to
    // remove. How many total rolls of paper could the Elves remove if they keep repeating this process? Stop once no more rolls of paper are
    // accessible by a forklift.
    //
    // Puzzle == Start with your original map. How many rolls of paper in total can be removed by the Elves and their forklifts?
    private static int Puzzle2(byte[,] map, bool dumpMapsToImages)
    {
        var neighborCounts = CreateMapWithNeighborCount(map);

        var removed  = new HashSet<(int Row, int Col)>();
        var toRemove = neighborCounts.Where((neighborCount, row, col) => map[row, col] > 0 && neighborCount < 4).ToArray();

        // in each iteration remove the paper rolls with less than 4 neighbors
        // it is not neccessary to recalculate the whole map. only the neighbors of removed paper rolls have to be recalculated and considered for
        // removal in the next iteration. 

        var iteration = 0;

        do
        {
            if(dumpMapsToImages)
            {
                FileUtils.WriteMapToPngImage(map, $"map{iteration.ToString("D5")}.png");
            }

            if(toRemove.Length == 0)
            {
                return removed.Count;
            }

            var toRemoveInThisIteration = new HashSet<(int Row, int Col)>();

            foreach(var (_, row, col) in toRemove)
            {
                map[row, col] = 0;
                removed.Add((row, col));
            }

            foreach(var (_, row, col) in toRemove)
            {
                foreach(var neighborOffset in Neighbors8)
                {
                    var (neighborRow, neighborCol) = (row + neighborOffset.Row, col + neighborOffset.Col);

                    if(neighborRow >= 0 && neighborRow < map.GetLength(0) &&
                       neighborCol >= 0 && neighborCol < map.GetLength(1) &&
                       map[neighborRow, neighborCol] > 0)
                    {
                        var neighbors = Math.Clamp(neighborCounts[neighborRow, neighborCol] - 1, 0, int.MaxValue);
                        neighborCounts[neighborRow, neighborCol] = neighbors;

                        if(neighbors < 4)
                        {
                            toRemoveInThisIteration.Add((neighborRow, neighborCol));    
                        }
                    }
                }
            }

            toRemove = toRemoveInThisIteration.Select(pos => (0, pos.Row, pos.Col)).ToArray();

            iteration++;
        }
        while(true);
    }

    // maps (row, col) -> number of neighbors
    private static int[,] CreateMapWithNeighborCount(byte[,] map)
    {
        var neighborhood = new int[map.GetLength(0), map.GetLength(1)];
        neighborhood.Populate((row, col) => Neighbors8.Select(offset => (Row: offset.Row + row, Col: offset.Col + col))
                                                      .Where(pos => pos.Row >= 0 && pos.Row < map.GetLength(0) &&
                                                                    pos.Col >= 0 && pos.Col < map.GetLength(1))
                                                      .Sum(pos => map[pos.Row, pos.Col]));
        return neighborhood;
    }

    // all 8:
    // XXX
    // XOX
    // XXX
    private static readonly IEnumerable<(int Row, int Col)> Neighbors8 =
        [ (-1, -1), (0, -1), (+1, -1), (-1, 0), (1, 0), (-1, 1), (0, 1), (1, 1) ];
}
