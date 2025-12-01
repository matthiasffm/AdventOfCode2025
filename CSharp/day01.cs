namespace AdventOfCode2025;

using System;
using FluentAssertions;
using NUnit.Framework;

/// <summary>
/// Theme: Panzerknacker
/// </summary>
[TestFixture]
public class Day01
{
    private static int[] ParseData(string[] data) =>
        [.. data.Select(line => (line[0] == 'L' ? -1 : 1) * int.Parse(line[1..]))];

    [Test]
    public void TestSamples()
    {
        var data = new [] {
            "L68",
            "L30",
            "R48",
            "L5",
            "R60",
            "L55",
            "L1",
            "L99",
            "R14",
            "L82",
        };
        var dialTurns = ParseData(data);

        Puzzle1(50, 100, dialTurns).Should().Be(3);
        Puzzle2(50, 100, dialTurns).Should().Be(6);
    }

    [Test]
    public void TestAocInput()
    {
        var data      = FileUtils.ReadAllLines(this);
        var dialTurns = ParseData(data);

        Puzzle1(50, 100, dialTurns).Should().Be(962);
        Puzzle2(50, 100, dialTurns).Should().Be(5782);
    }

    // The safe has a dial with only an arrow on it; around the dial are the numbers 0 through 99 in order. As you turn the dial, it makes a
    // small click noise as it reaches each number. The attached document (your puzzle input) contains a sequence of rotations, one per line,
    // which tell you how to open the safe. The rotation sign indicates whether the rotation should be to the left or to the right. Then, the
    // rotation has a distance value which indicates how many clicks the dial should be rotated in that direction.
    // Your recent security training seminar taught you that the safe is actually a decoy. The actual password is the number of times the dial
    // is left pointing at 0 after any rotation in the sequence.
    //
    // Puzzle == Analyze the rotations in your attached document. What's the actual password to open the door?
    private static int Puzzle1(int dialPos, int dialSize, int[] dialTurns)
    {
        var dialAtZero = 0;

        foreach(var dialTurn in dialTurns)
        {
            dialPos = Modulo(dialPos + dialTurn, dialSize);

            if(dialPos == 0)
            {
                dialAtZero++;
            }
        }

        return dialAtZero;
    }

    // As you're rolling the snowballs for your snowman, you find another security document that must have fallen into the snow: It reads you're
    // actually supposed to count the number of times _any_ click causes the dial to point at 0, regardless of whether it happens during a rotation
    // or at the end of one.
    //
    // Puzzle == Using this password method, what is the password to open the door?
    private static int Puzzle2(int dialPos, int dialSize, int[] dialTurns)
    {
        var dialCrossesZero = 0;

        foreach(var dialTurn in dialTurns)
        {
            var oldDialPos = dialPos;
            dialPos = Modulo(dialPos + dialTurn, dialSize);

            if((dialTurn > 0 && oldDialPos > dialPos && oldDialPos != 0) ||
               (dialTurn < 0 && oldDialPos < dialPos && oldDialPos != 0) ||
               (dialPos == 0))
            {
                dialCrossesZero++;
            }

            // there are turns > dialSize, count each time the 0 is passed
            dialCrossesZero += Math.Abs(dialTurn) / dialSize;
        }

        return dialCrossesZero;
    }

    // correct for positive and negative n
    private static int Modulo(int n, int m) => ((n % m) + m) % m;
}
