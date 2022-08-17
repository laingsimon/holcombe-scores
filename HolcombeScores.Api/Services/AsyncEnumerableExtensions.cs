namespace HolcombeScores.Api.Services
{
    public static class AsyncEnumerableExtensions
    {
        public static async Task<IEnumerable<T>> ToEnumerable<T>(this IAsyncEnumerable<T> asyncEnumerable)
        {
            return await asyncEnumerable.ToListAsync();
        }

        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> asyncEnumerable)
        {
            var items = new List<T>();

            await foreach (var item in asyncEnumerable)
            {
                items.Add(item);
            }

            return items;
        }

        public static async Task<T[]> ToArrayAsync<T>(this IAsyncEnumerable<T> asyncEnumerable)
        {
            var items = new List<T>();

            await foreach (var item in asyncEnumerable)
            {
                items.Add(item);
            }

            return items.ToArray();
        }

        public static async Task<TIn> SingleOrDefaultAsync<TIn>(this IAsyncEnumerable<TIn> asyncEnumerable,
            Func<TIn, bool> predicate)
        {
            TIn result = default;
            var found = false;

            await foreach (var item in asyncEnumerable)
            {
                if (!predicate(item))
                {
                    continue;
                }

                if (found)
                {
                    // another item was previously found
                    throw new InvalidOperationException("More than one element satisfies the condition in predicate.");
                }

                found = true;
                result = item;
            }

            return result;
        }

        public static async IAsyncEnumerable<TOut> SelectAsync<TIn, TOut>(this IAsyncEnumerable<TIn> asyncEnumerable,
            Func<TIn, TOut> select)
        {
            await foreach (var item in asyncEnumerable)
            {
                yield return select(item);
            }
        }

        public static async Task<IEnumerable<TOut>> SelectAsync<TIn, TOut>(this IEnumerable<TIn> asyncEnumerable,
            Func<TIn, Task<TOut>> select)
        {
            var items = new List<TOut>();

            foreach (var item in asyncEnumerable)
            {
                items.Add(await select(item));
            }

            return items;
        }
    }
}