using System;
using Azure;
using Azure.Data.Tables;

namespace HolcombeScores.Models
{
    public class Access : ITableEntity
    {
        public Guid TeamId { get; set; }
        public DateTime Granted { get; set; }
        public DateTime? Revoked { get; set; }
        public Guid UserId { get; set; }
        public bool Admin { get; set; }
        public string Name { get; set; }
        public string RevokedReason { get; set; }
        public string Token { get; set; }

        /// <summary>
        /// TeamId
        /// </summary>
        public string PartitionKey { get; set; }

        /// <summary>
        /// UserId
        /// </summary>
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
