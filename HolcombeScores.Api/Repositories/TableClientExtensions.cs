using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Azure.Data.Tables;

namespace HolcombeScores.Api.Repositories
{
    public static class TableClientExtensions
    {
        public static async Task<T> SingleOrDefaultAsync<T>(this TableClient client, Expression<Func<T, bool>> predicate)
            where T : class, ITableEntity, new()
        {
            var results = client.QueryAsync(predicate);

            await foreach (var result in results)
            {
                return result;
            }

            return default;
        }
    }
}