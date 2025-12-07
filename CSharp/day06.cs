namespace AdventOfCode2025;

using FluentAssertions;
using NUnit.Framework;
using System.Diagnostics;

using matthiasffm.Common.Math;

/// <summary>
/// Theme: solving Cephalopod math worksheets
/// </summary>
[TestFixture]
public class Day06
{
    // For the first part the problem's numbers are arranged vertically; at the bottom of the problem is the symbol for the operation that needs to be
    // performed. Problems are separated by a full column of only spaces. The left/right alignment of numbers within each problem can be ignored.
    // so from digits   ab   cde   f
    //                  g    hij   k
    // to numbers =>   [ab] [cde] [f]
    //                  [g] [hij] [k]
    private static (long[,] numbers, char[] operators) ParseData1(string[] data)
    {
        var numbers = data.SkipLast(1)
                          .Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                              .Select(number => long.Parse(number))
                                              .ToArray())
                          .ToArray()
                          .ConvertToMatrix();

        var operators = data.Last()
                            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .Select(c => c[0])
                            .ToArray();

        return (numbers, operators);    
    }

    // For the second part the problem is instead read in columns. Each number is given in its own column, with the most significant digit at the top and the
    // least significant digit at the bottom. (Problems are still separated with a column consisting only of spaces, and the symbol at the bottom of the
    // problem is still the operator to use.)
    // so from digits  ab   cde    f
    //                 g    hij    k
    // to numbers =>  [ag]  [ch]  [fk]
    //                 [b]  [di]  [0/1]
    //                [0/1] [ej]  [0/1]
    // basically build a transposed matrix of the digits in the input text and fill blanks with 0 or 1 (identity value for operation + or *)
    private static (long[,] numbers, char[] operators) ParseData2(string[] data)
    {
        var operators = data.Last()
                            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .Select(c => c[0])
                            .ToArray();

        var cols = new List<List<long>>();

        var i = 0;
        var col = new List<long>();

        do
        {
            var nextNumber = new string([.. Enumerable.Range(0, data.Length - 1).Select(line => data[line][i])]).Trim();
            if(nextNumber.Length > 0)
            {
                col.Add(long.Parse(nextNumber));
            }
            else
            {
                cols.Add(col);
                col = [];
            }
            i++;
        }
        while(i < data[0].Length);
        cols.Add(col);

        var numbers = new long[cols.Max(n => n.Count), cols.Count];
        numbers.Populate((row, col) => row < cols[col].Count ? cols[col][row] : (operators[col] == '*' ? 1 : 0));

        return (numbers, operators);
    }

    [Test]
    public void TestSamples()
    {
        var data = new[]
        {
            "123 328  51 64 ",
            " 45 64  387 23 ",
            "  6 98  215 314",
            "*   +   *   +  ",
        };

        var (numbers, operators) = ParseData1(data);
        Puzzle(numbers, operators).Should().Be(33210L + 490L + 4243455L + 401L);

        (numbers, operators) = ParseData2(data);
        Puzzle(numbers, operators).Should().Be(1058L + 3253600L + 625L + 8544L);
    }

    [Test]
    public void TestAocInput()
    {
        var data = FileUtils.ReadAllLines(this);

        var (numbers, operators) = ParseData1(data);
        Puzzle(numbers, operators).Should().Be(4412382293768L);

        (numbers, operators) = ParseData2(data);
        Puzzle(numbers, operators).Should().Be(7858808482092L);
    }

    // Cephalopod math doesn't look that different from normal math. The math worksheet (your puzzle input) consists of a list of problems; each
    // problem has a group of numbers that need to be either added (+) or multiplied (*) together.
    // To check their work, cephalopod students are given the grand total of adding together all of the answers to the individual problems.
    //
    // Puzzle == What is the grand total found by adding together all of the answers to the individual problems?
    private static long Puzzle(long[,] numbers, char[] operators)
    {
        Debug.Assert(numbers.GetLength(1) == operators.Length);

        var sum = 0L;

        for(int col = 0; col < numbers.GetLength(1); col++)
        {
            sum += operators[col] switch {
                '+' => numbers.Col(col).Sum(n => n.Item2),
                _   => numbers.Col(col).Select(n => n.Item2).Aggregate(1L, (n, product) => n * product),
            };
        }

        return sum;
    }
}
