using System;
using Azure;
using Azure.Data.Tables;

namespace HolcombeScores.Api.Repositories
{
    public class GenericTableEntity<T> : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public T Content { get; set; }
    }
}