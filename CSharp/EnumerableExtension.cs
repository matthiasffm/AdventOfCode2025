namespace AdventOfCode2025;

public static class EnumerableExtension
{
    /// <summary>
    /// Returns all possible combinations of two distinct elements from <i>coll</i> (order doesn't matter).
    /// </summary>
    public static IEnumerable<(T, T)> TwoCombinations<T>(this IEnumerable<T> coll)
        => coll.SelectMany((elem1, i) => coll.Skip(i + 1)
                                             .Select(elem2 => (elem1, elem2)));
}
