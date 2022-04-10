using System;
using Azure;
using Azure.Data.Tables;

namespace HolcombeScores.Models
{
    public class Player : ITableEntity
    {
        public Guid TeamId { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }

        /// <summary>
        /// TeamId
        /// </summary>
        public string PartitionKey { get; set; }
        /// <summary>
        /// Number
        /// </summary>
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}