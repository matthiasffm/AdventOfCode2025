namespace AdventOfCode2025;

using FluentAssertions;
using NUnit.Framework;

using matthiasffm.Common.Collections;

/// <summary>
/// Theme: matching patterns in ids
/// </summary>
[TestFixture]
public class Day02
{
    private static (long FirstId, long LastId)[] ParseData(string data) =>
        [.. data.Split(',').Select(ids => (long.Parse(ids.Split('-')[0]), long.Parse(ids.Split('-')[1])))];

    [Test]
    public void TestSamples()
    {
        var data = "11-22,95-115,998-1012,1188511880-1188511890,222220-222224,1698522-1698528,446443-446449,38593856-38593862,565653-565659,824824821-824824827,2121212118-2121212124";
        var idRanges = ParseData(data);

        Puzzle1(idRanges).Should().Be(1227775554L);
        Puzzle2(idRanges).Should().Be(4174379265L);
    }

    [Test]
    public void TestAocInput()
    {
        var data = FileUtils.ReadAllText(this);
        var idRanges = ParseData(data);

        Puzzle1(idRanges).Should().Be(19386344315L);
        Puzzle2(idRanges).Should().Be(34421651192L);
    }

    // A younger Elve was playing on a gift shop computer and managed to add a whole bunch of invalid product IDs to their gift shop database! Surely, it would be no trouble
    // for you to identify the invalid product IDs for them, right? The ranges are separated by commas (,); each range gives its first ID and last ID separated by a dash (-).
    // Since the young Elf was just doing silly patterns, you can find the invalid IDs by looking for any ID which is made only of some sequence of digits repeated twice: 55
    // (5 twice), 6464 (64 twice), and 123123 (123 twice) would all be invalid IDs. Your job is to find all of the invalid IDs that appear in the given ranges.
    //
    // Puzzle == What do you get if you add up all of the invalid IDs?
    private static long Puzzle1((long FirstId, long LastId)[] idRanges)
        => idRanges.Sum(idRange => idRange.FirstId.To(idRange.LastId).Where(id => IsDoublePattern(id.ToString())).Sum());

    // for part 1 only strings of the form abcabc are valid
    // TODO: could this be directly calculated by cutting the numeric number in 2 equal parts without ToString()?
    private static bool IsDoublePattern(string id)
        => (id.Length % 2 == 0) &&
           id[..(id.Length / 2)] == id[(id.Length / 2)..];

    // There are still invalid IDs in the list. Additionally an ID is invalid if it is made only of _some_ sequence of digits repeated at least twice. So,
    // 12341234 (1234 two times), 123123123 (123 three times), 1212121212 (12 five times), and 1111111 (1 seven times) are all invalid IDs.
    //
    // Puzzle == What do you get if you add up all of the invalid IDs using these new rules?
    private static long Puzzle2((long FirstId, long LastId)[] idRanges)
        => idRanges.Sum(idRange => idRange.FirstId.To(idRange.LastId).Where(id => IsRepeatingPattern(id)).Sum());
    // TODO: is a bit slow
    //       instead of testing every number in range it could be faster to just produce all repeating patterns in the range directly

    // for part 2 any pattern has to be considered, so start from left and pattern length * repeats has to be id length
    private static bool IsRepeatingPattern(long id)
    {
        ReadOnlySpan<char> number = id.ToString();

        for(int patternLength = 1; patternLength <= number.Length / 2L; patternLength++)
        {
            // we are basically only looping over all denominators of number.Length otherwise there can be no full repeat
            if(number.Length % patternLength == 0 &&
               CountRepeats(number, number[..patternLength]) == number.Length / patternLength)
            {
                return true;
            }
        }

        return false;
    }

    private static int CountRepeats(ReadOnlySpan<char> text, ReadOnlySpan<char> pattern)
    {
        int repeats = 1;

		for(int i = pattern.Length; i < text.Length - pattern.Length + 1; i += pattern.Length)
        {
            if(text[i..(i + pattern.Length)].SequenceEqual(pattern))
            {
                repeats++;
            }
            else
            {
                break;
            }
        }

        return repeats;
    }
}
