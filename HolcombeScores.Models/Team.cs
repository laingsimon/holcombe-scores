using System;
using Azure;
using Azure.Data.Tables;

namespace HolcombeScores.Models
{
    public class Team : ITableEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Coach { get; set; }

        /// <summary>
        /// TeamId
        /// </summary>
        public string PartitionKey { get; set; }

        /// <summary>
        /// TeamId
        /// </summary>
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}