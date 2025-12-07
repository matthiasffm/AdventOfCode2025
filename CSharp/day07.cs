namespace AdventOfCode2025;

using FluentAssertions;
using NUnit.Framework;

using static matthiasffm.Common.Algorithms.Basics;

/// <summary>
/// Theme: splitting quantum tachyon manifolds
/// </summary>
[TestFixture]
public class Day07
{
    private static (int Row, int Col)[] ParseData(string[] lines)
        => [.. lines.Where((line, row) => row % 2 == 0)
                    .SelectMany((line, row) => line.Select((l, col) => (l, col))
                                                   .Where(c => c.l == '^')
                                                   .Select(c => (row, c.col)))];

    [Test]
    public void TestSamples()
    {
        var data = new[]
        {
            ".......S.......",
            "...............",
            ".......^.......",
            "...............",
            "......^.^......",
            "...............",
            ".....^.^.^.....",
            "...............",
            "....^.^...^....",
            "...............",
            "...^.^...^.^...",
            "...............",
            "..^...^.....^..",
            "...............",
            ".^.^.^.^.^...^.",
            "...............",
        };
        var splitters = ParseData(data);

        Puzzle1(splitters, data[0].IndexOf('S')).Should().Be(21);
        Puzzle2(splitters, data[0].IndexOf('S')).Should().Be(40L);
    }

    [Test]
    public void TestAocInput()
    {
        var data = FileUtils.ReadAllLines(this);
        var splitters = ParseData(data);

        Puzzle1(splitters, data[0].IndexOf('S')).Should().Be(1507);
        Puzzle2(splitters, data[0].IndexOf('S')).Should().Be(1537373473728L);
    }

    // You quickly locate a diagram of the tachyon manifold (your puzzle input). A tachyon beam enters the manifold at the location marked S; tachyon beams always
    // move downward. Tachyon beams pass freely through empty space (.). However, if a tachyon beam encounters a splitter (^), the beam is stopped; instead, a new
    // tachyon beam continues from the immediate left and from the immediate right of the splitter. If two splitters dump tachyons into the same place between
    // them, this resulting beam counts as one single tachyon beam. This process continues until all of the tachyon beams reach a splitter or exit the manifold.
    //
    // Puzzle == Analyze your manifold diagram. How many times will the beam be split?
    private static int Puzzle1((int Row, int Col)[] splitters, int startCol)
        => UsedSplitters(splitters, startCol).Count();

    // However, as you open the side of the teleporter to replace the broken manifold, you are surprised to discover that it isn't a classical tachyon manifold - it's
    // a quantum tachyon manifold. With a quantum tachyon manifold, only a single tachyon particle is sent through the manifold. A tachyon particle takes both the left
    // and right path of each splitter encountered. The manual recommends the many-worlds interpretation of quantum tachyon splitting: each time a particle reaches a
    // splitter, it's actually time itself which splits. In one timeline, the particle went left, and in the other timeline, the particle went right.
    // To fix the manifold, what you really need to know is the number of timelines active after a single particle completes all of its possible journeys through the manifold.
    //
    // Puzzle == Apply the many-worlds interpretation of quantum tachyon splitting to your input. How many different timelines would a single tachyon particle end up on?
    private static long Puzzle2((int Row, int Col)[] splitters, int startCol)
    {
        var allSplitters = UsedSplitters(splitters, startCol).ToHashSet();

        // from the top count in each row how many possible timelines there are at every column with a splitter
        // timelines(col, row) = timelines(col - 1, row - 1) + timelines(col + 1, row - 1)
        // and if there is no splitter
        // timelines(col, row + 1) = timelines(col, row)

        var timelines = new long[splitters.Max(s => s.Col) + 2];
        timelines[startCol] = 1L;

        var nextTimelines = new long[timelines.Length];

        for(int row = 1; row <= splitters.Max(s => s.Row); row++)
        {
            for(int col = 0; col < timelines.Length; col++)
            {
                nextTimelines[col] = (allSplitters.Contains((row, col - 1)) ? timelines[col - 1] : 0L) +
                                     (allSplitters.Contains((row, col))     ? 0L : timelines[col]) +
                                     (allSplitters.Contains((row, col + 1)) ? timelines[col + 1] : 0L);
            }

            Swap(ref timelines, ref nextTimelines);
        }

        return timelines.Sum();
    }

    // remove all unreachable splitters like the middle one in the second row here
    // ...^...
    // ..^^^..
    private static IEnumerable<(int Row, int Col)> UsedSplitters((int Row, int Col)[] splitters, int startCol)
    {
        var splittersByRow = splitters.ToLookup(splitter => splitter.Row, splitter => splitter.Col);

        var activatedSplittersByCol = new Dictionary<int, List<int>>
        {
            [startCol] = [splitters.Where(s => s.Col == startCol).Min(s => s.Row)],
        };

        foreach(var splitterRow in splittersByRow.Skip(1))
        {
            foreach(var splitterCol in splitterRow)
            {
                var activatedToLeft  = RowAbove(activatedSplittersByCol, splitterCol - 1);
                var activatedOnTop   = RowAbove(activatedSplittersByCol, splitterCol);
                var activatedToRight = RowAbove(activatedSplittersByCol, splitterCol + 1);

                if((activatedToLeft >= 0 || activatedToRight >= 0) && (activatedOnTop < 0 || activatedOnTop < Math.Max(activatedToLeft, activatedToRight)))
                {
                    if(activatedSplittersByCol.TryGetValue(splitterCol, out var list))
                    {
                        list.Add(splitterRow.Key);
                    }
                    else
                    {
                        activatedSplittersByCol[splitterCol] = [splitterRow.Key];
                    }
                }
                    
            }
        }

        return activatedSplittersByCol.SelectMany(v => v.Value.Select(r => (r, v.Key)));
    }

    // finds the row of the nearest splitter above in the current col if it exists
    private static int RowAbove(Dictionary<int, List<int>> splitterCols, int col)
        => splitterCols.TryGetValue(col, out var row) ? row.Max() : -1;
}
