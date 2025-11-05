namespace NetUtility;

public static partial class Extension
{
    public static IEnumerable<T> Concat<T>(params IEnumerable[] enumerables)
    {
        return enumerables.NotNull().SelectMany(enumerable => enumerable.OfType<T>());
    }

    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
    {
        return items.GroupBy(property).Select(x => x.First());
    }

    public static IEnumerable<T> NotNull<T>(this IEnumerable<T> enumerable)
    {
        return enumerable.Where(i => i != null);
    }

    public static IEnumerable<T> Yield<T>(this T t)
    {
        yield return t;
    }

    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable)
    {
        return [..enumerable];
    }

    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items) collection.Add(item);
    }

    public static void AddRange(this IList list, IEnumerable items)
    {
        foreach (var item in items) list.Add(item);
    }

    // NETUP: Replace with IReadOnlyCollection, IReadOnlyList

    public static ICollection<T> AsReadOnlyCollection<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable is ICollection<T> collection) return collection;

        return enumerable.ToList().AsReadOnly();
    }

    public static IList<T> AsReadOnlyList<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable is IList<T> list) return list;

        return enumerable.ToList().AsReadOnly();
    }

    public static IEnumerable<T> Flatten<T>
    (
        this IEnumerable<T> source,
        Func<T, IEnumerable<T>> childrenSelector
    )
    {
        var flattenedList = source;

        foreach (var element in source)
            flattenedList = flattenedList.Concat(childrenSelector(element).Flatten(childrenSelector));

        return flattenedList;
    }

    public static IEnumerable<T> IntersectAll<T>(this IEnumerable<IEnumerable<T>> groups)
    {
        HashSet<T> hashSet = null;

        foreach (var group in groups)
            if (hashSet == null)
                hashSet = new HashSet<T>(group);
            else
                hashSet.IntersectWith(group);

        return hashSet.AsEnumerable();
    }

    public static IEnumerable<T> OrderByDependencies<T>(this IEnumerable<T> source,
        Func<T, IEnumerable<T>> getDependencies, bool throwOnCycle = true)
    {
        var sorted = new List<T>();
        var visited = HashSetPool<T>.New();

        foreach (var item in source) OrderByDependenciesVisit(item, visited, sorted, getDependencies, throwOnCycle);

        HashSetPool<T>.Free(visited);

        return sorted;
    }

    private static void OrderByDependenciesVisit<T>(T item, HashSet<T> visited, List<T> sorted,
        Func<T, IEnumerable<T>> getDependencies, bool throwOnCycle)
    {
        if (visited.Add(item))
        {
            foreach (var dependency in getDependencies(item))
                OrderByDependenciesVisit(dependency, visited, sorted, getDependencies, throwOnCycle);

            sorted.Add(item);
        }
        else
        {
            if (throwOnCycle && !sorted.Contains(item)) throw new InvalidOperationException("Cyclic dependency.");
        }
    }

    public static IEnumerable<T> OrderByDependers<T>(this IEnumerable<T> source,
        Func<T, IEnumerable<T>> getDependers, bool throwOnCycle = true) where T : notnull
    {
        // TODO: Optimize, or use another algorithm (Kahn's?)

        // Convert dependers to dependencies
        var dependencies = new Dictionary<T, HashSet<T>>();

        foreach (var dependency in source)
        foreach (var depender in getDependers(dependency))
        {
            if (!dependencies.TryGetValue(depender, out var value))
            {
                value = [];
                dependencies.Add(depender, value);
            }

            value.Add(dependency);
        }

        return source.OrderByDependencies(
            depender => dependencies.TryGetValue(depender, out var expression) ? expression : [], throwOnCycle);
    }

    public static IEnumerable<T> Catch<T>(this IEnumerable<T> source, Action<Exception> @catch)
    {
        if(source is null)throw new ArgumentNullException(nameof(source));

        using var enumerator = source.GetEnumerator();
        bool success;

        do
        {
            try
            {
                success = enumerator.MoveNext();
            }
            catch (OperationCanceledException)
            {
                yield break;
            }
            catch (Exception ex)
            {
                @catch?.Invoke(ex);
                success = false;
            }

            if (success) yield return enumerator.Current;
        } while (success);
    }

    public static IEnumerable<T> Catch<T>(this IEnumerable<T> source, ICollection<Exception> exceptions)
    {
        return source is null ? throw new ArgumentNullException(nameof(source)) : source.Catch(exceptions.Add);
    }
}

public static class HashSetPool<T>
{
    private static readonly object Lock = new();
    private static readonly Stack<HashSet<T>> free = new();
    private static readonly HashSet<HashSet<T>> busy = [];

    public static HashSet<T> New()
    {
        lock (Lock)
        {
            if (free.Count == 0) free.Push([]);

            var hashSet = free.Pop();

            busy.Add(hashSet);

            return hashSet;
        }
    }

    public static void Free(HashSet<T> hashSet)
    {
        lock (Lock)
        {
            if (!busy.Remove(hashSet))
                throw new ArgumentException("The hash set to free is not in use by the pool.", nameof(hashSet));

            hashSet.Clear();

            free.Push(hashSet);
        }
    }
}