using System;
using Azure;
using Azure.Data.Tables;

namespace HolcombeScores.Models
{
    public class AccessRequest : ITableEntity
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public Guid TeamId { get; set; }
        public DateTime Requested { get; set; }
        public bool Recovery { get; set; }

        /// <summary>
        /// The same as TeamId
        /// </summary>
        public string PartitionKey { get; set; }

        /// <summary>
        /// The same as UserId
        /// </summary>
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
