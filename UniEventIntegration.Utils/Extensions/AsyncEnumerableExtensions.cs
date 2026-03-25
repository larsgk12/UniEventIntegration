namespace UniEventIntegration.Utils.Extensions;

public static class AsyncEnumerableExtensions
{
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> items)
    {
        List<T> list = [];
        if (items is null) return list;
        await foreach (var item in items.ConfigureAwait(false))
        {
            list.Add(item);
        }
        return list;
    }
}
