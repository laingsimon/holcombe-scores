using System;
using System.Collections.Generic;
using Azure;

namespace HolcombeScores.Api.Repositories
{
    public class GenericEntityAdapter : IGenericEntityAdapter
    {
        public GenericTableEntity<T> Adapt<T>(T content, object partitionKey, object rowKey)
        {
            if (partitionKey == null)
            {
                throw new ArgumentNullException(nameof(partitionKey));
            }

            if (rowKey == null)
            {
                throw new ArgumentNullException(nameof(rowKey));
            }

            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            return new GenericTableEntity<T>
            {
                Timestamp = DateTimeOffset.UtcNow,
                ETag = new ETag(rowKey.ToString()),
                Content = content,
                PartitionKey = partitionKey.ToString(),
                RowKey = rowKey.ToString(),
            };
        }

        public T Adapt<T>(GenericTableEntity<T> tableRecord)
        {
            return tableRecord.Content;
        }

        public async IAsyncEnumerable<T> AdaptAll<T>(IAsyncEnumerable<GenericTableEntity<T>> results)
        {
            await foreach (var entity in results)
            {
                yield return Adapt(entity);
            }
        }
    }
}