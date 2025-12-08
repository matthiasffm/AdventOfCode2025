namespace AdventOfCode2025;

using FluentAssertions;
using NUnit.Framework;
using System.Diagnostics;

using matthiasffm.Common.Collections;

/// <summary>
/// Theme: clustering a graph of junction boxes
/// </summary>
[TestFixture]
public class Day08
{
    private readonly record struct Coord(int X, int Y, int Z);

    private static Coord[] ParseData(string[] lines)
        => [.. lines.Select(l => new Coord(int.Parse(l.Split(',')[0]), int.Parse(l.Split(',')[1]), int.Parse(l.Split(',')[2])))];

    [Test]
    public void TestSamples()
    {
        var data = new[]
        {
            "162,817,812",
            "57,618,57",
            "906,360,560",
            "592,479,940",
            "352,342,300",
            "466,668,158",
            "542,29,236",
            "431,825,988",
            "739,650,466",
            "52,470,668",
            "216,146,977",
            "819,987,18",
            "117,168,530",
            "805,96,715",
            "346,949,466",
            "970,615,88",
            "941,993,340",
            "862,61,35",
            "984,92,344",
            "425,690,689",
        };
        var junctionBoxes = ParseData(data);

        Puzzle1(junctionBoxes, 10, 3).Should().Be(5 * 4 * 2);
        Puzzle2(junctionBoxes).Should().Be(216 * 117);
    }

    [Test]
    public void TestAocInput()
    {
        var data = FileUtils.ReadAllLines(this);
        var junctionBoxes = ParseData(data);

        Puzzle1(junctionBoxes, 1000, 3).Should().Be(79560L);
        Puzzle2(junctionBoxes).Should().Be(31182420L);
    }

    // A group of Elves are working on setting up an ambitious Christmas decoration project. Through careful rigging, they have suspended a large number of small
    // electrical junction boxes. Their plan is to connect the junction boxes with long strings of lights. Most of the junction boxes don't provide electricity;
    // however, when two junction boxes are connected by a string of lights, electricity can pass between those two junction boxes.
    // The Elves are trying to figure out which junction boxes to connect so that electricity can reach every junction box. They even have a list of all of the
    // junction boxes' positions in 3D space (your puzzle input).
    // To save on string lights, the Elves would like to focus on connecting pairs of junction boxes that are as close together as possible according to straigh
    // line distance. By connecting two junction boxes together, because electricity can flow between them, they become part of the same circuit.
    //
    // Puzzle == Your list contains many junction boxes; connect together the n pairs of junction boxes which are closest together. Afterward, what do you get if
    //           you multiply together the sizes of the three largest circuits?
    private static long Puzzle1(Coord[] junctionBoxes, int numberOfConnections, int largestCurcuitsToSum)
    {
        // first compute all distances between the junction boxes and take the numberOfConnections shortest of them
        var distances = junctionBoxes.Variations()
                                     .Select(boxes => (A: boxes.Item1, B: boxes.Item2, Distance: EuclidianDistance(boxes.Item1, boxes.Item2)))
                                     .OrderBy(boxPairs => boxPairs.Distance)
                                     .Where((t, i) => i % 2 == 0) // removes permutations
                                     .Take(numberOfConnections)
                                     .ToArray();
        // TODO: not variations/permutations but 2-combinations
        //       this also should be addressed in common

        // with the n shortest distances build clusters from their connections

        var clusters       = new Dictionary<int, List<Coord>>();
        var clusterMapping = new Dictionary<Coord, int>();

        for(var i = 0; i < numberOfConnections; i++)
        {
            var (a, b, distance) = distances[i];

            InsertConnectionIntoClusters(clusters, clusterMapping, a, b);
        }

        return clusters.OrderByDescending(c => c.Value.Count)
                       .Take(largestCurcuitsToSum)
                       .Aggregate(1L, (p, cluster) => p * cluster.Value.Count);
    }

    // The Elves don't have enough extension cables. You'll need to keep connecting junction boxes together until they're all in one large circuit.
    //
    // Puzzle == Continue connecting the closest unconnected pairs of junction boxes together until they're all in the same circuit. What do you get
    //           if you multiply together the X coordinates of the last two junction boxes you need to connect?
    private static long Puzzle2(Coord[] junctionBoxes)
    {
        var distances = junctionBoxes.Variations()
                                     .Select(boxes => (A: boxes.Item1, B: boxes.Item2, Distance: EuclidianDistance(boxes.Item1, boxes.Item2)))
                                     .OrderBy(boxPairs => boxPairs.Distance)
                                     .Where((t, i) => i % 2 == 0)
                                     .ToArray();
        // TODO: not variations/permutations but 2-combinations
        //       this also should be addressed in common

        var clusters       = new Dictionary<int, List<Coord>>();
        var clusterMapping = new Dictionary<Coord, int>();

        // same merging as in part 1, but this time the exit condition is not a predetermined iteration number but when all
        // connections cluster in one single cluster (takes longer)

        int i = 0;
        do
        {
            var (a, b, distance) = distances[i++];

            bool clustersChanged = InsertConnectionIntoClusters(clusters, clusterMapping, a, b);

            if(clustersChanged && clusters.Max(c => c.Value.Count) == junctionBoxes.Length)
            {
                return (long)a.X * (long)b.X;
            }
        }
        while (true);
    }

    // inserts a connection between coords a and b into the correct clusters. creates or merges clusters if necessary
    private static bool InsertConnectionIntoClusters(Dictionary<int, List<Coord>> clusters, Dictionary<Coord, int> clusterMapping, Coord a, Coord b)
    {
        bool isMappedA = clusterMapping.TryGetValue(a, out int clusterIdA);
        bool isMappedB = clusterMapping.TryGetValue(b, out int clusterIdB);

        if(isMappedA && isMappedB)
        {
            if(clusterIdA == clusterIdB)
            {
                return false;
            }

            // merge clusters

            Debug.Assert(clusters[clusterIdA].Count > 0);
            Debug.Assert(clusters[clusterIdB].Count > 0);

            foreach(var coordsB in clusters[clusterIdB])
            {
                clusters[clusterIdA].Add(coordsB);
                clusterMapping[coordsB] = clusterIdA;
            }
            clusters[clusterIdB].Clear();
        }
        else if(isMappedA)
        {
            // add b to a

            Debug.Assert(clusters[clusterIdA].Count > 0);
            clusters[clusterIdA].Add(b);
            clusterMapping[b] = clusterIdA;
        }
        else if(isMappedB)
        {
            // add a to b

            Debug.Assert(clusters[clusterIdB].Count > 0);
            clusters[clusterIdB].Add(a);
            clusterMapping[a] = clusterIdB;
        }
        else
        {
            // create new cluster for a and b
            var newClusterId = clusters.Count;
            clusters[newClusterId] = [a, b];
            clusterMapping[a] = newClusterId;
            clusterMapping[b] = newClusterId;
        }

        return true;
    }

    private static double EuclidianDistance(Coord a, Coord b)
        => Math.Sqrt((double)(a.X - b.X) * (double)(a.X - b.X) +
                     (double)(a.Y - b.Y) * (double)(a.Y - b.Y) +
                     (double)(a.Z - b.Z) * (double)(a.Z - b.Z));
}
