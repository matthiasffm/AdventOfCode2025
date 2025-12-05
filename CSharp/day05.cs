namespace AdventOfCode2025;

using FluentAssertions;
using NUnit.Framework;

using matthiasffm.Common.Math;

/// <summary>
/// Theme: merge id ranges
/// </summary>
[TestFixture]
public class Day05
{
    private static ((long start, long end)[] freshIngredients, long[] availableIngredients) ParseData(string[] data)
    {
        var split = string.Join('#', data)
                          .Split("##", StringSplitOptions.RemoveEmptyEntries);
        return (split[0].Split('#').Select(line => (long.Parse(line.Split('-')[0]), long.Parse(line.Split('-')[1]))).ToArray(), 
                split[1].Split('#').Select(line => long.Parse(line)).ToArray());
    }

    [Test]
    public void TestSamples()
    {
        var data = new[]
        {
            "3-5",
            "10-14",
            "16-20",
            "12-18",
            "",
            "1",
            "5",
            "8",
            "11",
            "17",
            "32",
        };
        var (freshIngredients, availableIngredients) = ParseData(data);

        Puzzle1(freshIngredients, availableIngredients).Should().Be(3L);
        Puzzle2(freshIngredients).Should().Be(14L);
    }

    [Test]
    public void TestAocInput()
    {
        var data = FileUtils.ReadAllLines(this);
        var (freshIngredients, availableIngredients) = ParseData(data);

        Puzzle1(freshIngredients, availableIngredients).Should().Be(511L);
        Puzzle2(freshIngredients).Should().Be(350939902751909L);
    }

    // The Elves can't figure out which of their ingredients are fresh and which are spoiled. When you ask how it works, they give you a copy
    // of their database (your puzzle input). The database operates on ingredient IDs. It consists of a list of fresh ingredient ID ranges, a
    // blank line, and a list of available ingredient IDs. The fresh ID ranges are inclusive and can also overlap; an ingredient ID is fresh
    // if it is in any range.
    //
    // Puzzle == Process the database file from the new inventory management system. How many of the available ingredient IDs are fresh?
    private static long Puzzle1((long start, long end)[] freshIngredients, long[] availableIngredients)
        => availableIngredients.Sum(ingredient => freshIngredients.Any(fresh => ingredient.Between(fresh.start, fresh.end)) ? 1L : 0L);

    // The Elves would also like to know all of the IDs that the fresh ingredient ID ranges consider to be fresh. An ingredient ID is still
    // considered fresh if it is in any range.
    //
    // Puzzle == Process the database file again. How many ingredient IDs are considered to be fresh according to the fresh ingredient ID ranges?
    private static long Puzzle2(IEnumerable<(long start, long end)> freshIngredients)
    {
        // merge ranges

        var sortedRanges = new LinkedList<(long start, long end)>(freshIngredients.OrderBy(ingredient => ingredient.start));

        var iter = sortedRanges.First!;
        while(iter.Next != null)
        {
            var next = iter.Next;

            if(RangesIntersect(iter.Value, next.Value))
            {
                iter.Value = MergeRanges(iter.Value, next.Value);
                sortedRanges.Remove(next);
            }
            else
            {
                iter = next;
            }
        }

        // sum merged range sizes

        return sortedRanges.Sum(range => Math.Abs(range.end - range.start) + 1);
    }

    private static bool RangesIntersect((long start, long end) left, (long start, long end) right)
        => left.end >= right.start;

    private static (long start, long end) MergeRanges((long start, long end) left, (long start, long end) right)
        => (Math.Min(left.start, right.start), Math.Max(left.end, right.end));
}
