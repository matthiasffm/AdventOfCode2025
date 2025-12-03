namespace AdventOfCode2025;

using FluentAssertions;
using NUnit.Framework;

/// <summary>
/// Theme: 
/// </summary>
[TestFixture]
public class Day03
{
    private static byte[][] ParseData(string[] data) =>
        [.. data.Select(line => line.ToCharArray().Select(c => (byte)(c - 48)).ToArray())];

    [Test]
    public void TestSamples()
    {
        var data = new[]
        {
            "987654321111111",
            "811111111111119",
            "234234234234278",
            "818181911112111",
        };
        var batteryBanks = ParseData(data);

        Puzzle1(batteryBanks).Should().Be(98 + 89 + 78 + 92);
        Puzzle2(batteryBanks).Should().Be(987654321111L + 811111111119L + 434234234278L + 888911112111L);
    }

    [Test]
    public void TestAocInput()
    {
        var data = FileUtils.ReadAllLines(this);
        var batteryBanks = ParseData(data);

        Puzzle1(batteryBanks).Should().Be(17244L);
        Puzzle2(batteryBanks).Should().Be(171435596092638L);
    }

    // There are batteries nearby that can supply emergency power to the escalator for just such an occasion. The batteries are each labeled with their joltage
    // rating, a value from 1 to 9. You make a note of their joltage ratings (your puzzle input). The batteries are arranged into banks; each line of digits in
    // your input corresponds to a single bank of batteries. Within each bank, you need to turn on exactly two batteries; the joltage that the bank produces is
    // equal to the number formed by the digits on the batteries you've turned on. For example, if you have a bank like 12345 and you turn on batteries 2 and 4,
    // the bank would produce 24 jolts. (You cannot rearrange batteries.)
    // You'll need to find the largest possible joltage each bank can produce. The total output joltage is the sum of the maximum joltage from each bank.
    //
    // Puzzle == Find the maximum joltage possible from each bank; what is the total output joltage?
    private static long Puzzle1(byte[][] batteryBanks)
        => batteryBanks.Sum(bank => LargestJoltage(bank, 2));

    // Now, you need to make the largest joltage by turning on exactly twelve batteries within each bank. The joltage output for the bank is still the number formed
    // by the digits of the batteries you've turned on; the only difference is that now there will be 12 digits in each bank's joltage output instead of two.
    //
    // Puzzle == What is the new total output joltage?
    private static long Puzzle2(byte[][] batteryBanks)
        => batteryBanks.Sum(bank => LargestJoltage(bank, 12));

    // you cant rearrange batteries and the joltage digits form a decimal number from the left so at every step the best digit is the maximum still
    // available digit
    // the range of still available digits in the bank is just [from left where we found the last digit .. from right minus still to turn on batteries]
    // so in 818181911112111 for 2 to batteries turn on
    // 1.    ^     9      ^
    // 2.           ^   2  ^
    // => 92
    private static long LargestJoltage(byte[] bank, int numberToTurnOn)
    {
        var sum = 0L;
        var pos = 0;

        for(var turnedOn = 0; turnedOn < numberToTurnOn; turnedOn++)
        {
            var highestDigit    = 0;
            var highestDigitPos = 0;

            for(int i = pos; i < bank.Length - (numberToTurnOn - turnedOn - 1); i++)
            {
                if(bank[i] > highestDigit)
                {
                    highestDigit    = bank[i];
                    highestDigitPos = i;
                }
            }

            sum = sum * 10 + highestDigit;
            pos = highestDigitPos + 1;
        }

        return sum;
    }
}
