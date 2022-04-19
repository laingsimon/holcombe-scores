using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HolcombeScores.Api.Services
{
    public static class AsyncEnumerableExtensions
    {
        public static async Task<IEnumerable<T>> ToEnumerable<T>(this IAsyncEnumerable<T> asyncEnumerable)
        {
            var items = new List<T>();

            await foreach (var item in asyncEnumerable)
            {
                items.Add(item);
            }

            return items;
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