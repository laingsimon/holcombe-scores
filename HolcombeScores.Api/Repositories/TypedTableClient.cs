using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;

namespace HolcombeScores.Api.Repositories
{
    public class TypedTableClient<T>
        where T : class, ITableEntity, new()
    {
        private readonly TableClient _tableClient;

        public TypedTableClient(TableClient tableClient)
        {
            _tableClient = tableClient;
        }

        public async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            var results = _tableClient.QueryAsync(predicate);

            await foreach (var result in results)
            {
                return result;
            }

            return default;
        }

        public AsyncPageable<T> QueryAsync(Expression<Func<T, bool>> filter, int? maxPerPage = null, IEnumerable<string> select = null,
            CancellationToken cancellationToken = default)
        {
            return _tableClient.QueryAsync(filter, maxPerPage, select, cancellationToken);
        }

        public AsyncPageable<T> QueryAsync(string filter = null, int? maxPerPage = null, IEnumerable<string> select = null,
            CancellationToken cancellationToken = default)
        {
            return _tableClient.QueryAsync<T>(filter, maxPerPage, select, cancellationToken);
        }

        public Task<Response> DeleteEntityAsync(string partitionKey, string rowKey, ETag ifMatch = default,
            CancellationToken cancellationToken = default)
        {
            return _tableClient.DeleteEntityAsync(partitionKey, rowKey, ifMatch, cancellationToken);
        }

        public Task<Response> UpdateEntityAsync(T entity, ETag ifMatch, TableUpdateMode mode = TableUpdateMode.Merge,
            CancellationToken cancellationToken = default)
        {
            return _tableClient.UpdateEntityAsync(entity, ifMatch, mode, cancellationToken);
        }

        public Task<Response> AddEntityAsync(T entity, CancellationToken cancellationToken = default)
        {
            return _tableClient.AddEntityAsync(entity, cancellationToken);
        }
    }
}