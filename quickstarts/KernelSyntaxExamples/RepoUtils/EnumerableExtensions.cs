namespace KernelSyntaxExamples.RepoUtils;

public static class EnumerableExtensions
{
    public static IEnumerable<List<TSource>> ChunkByAggregate<TSource, TAccumulate>(
        this IEnumerable<TSource> source,
        TAccumulate seed,
        Func<TAccumulate, TSource, TAccumulate> aggregator,
        Func<TAccumulate, int, bool> predicate)
    {
        using IEnumerator<TSource> enumerator = source.GetEnumerator();

        TAccumulate aggregate = seed;

        int index = 0;

        List<TSource> chunk = new List<TSource>();

        while (enumerator.MoveNext())
        {
            TSource current = enumerator.Current;

            aggregate = aggregator(aggregate, current);

            if (predicate(aggregate, index++))
            {
                chunk.Add(current);
            }
            else
            {
                if (chunk.Count > 0)
                {
                    yield return chunk;
                }

                chunk = new List<TSource>() { current };

                aggregate = aggregator(seed, current);

                index = 1;
            }
        }

        if (chunk.Count > 0)
        {
            yield return chunk;
        }
    }
}
