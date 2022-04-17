using System;
using Azure;
using Azure.Data.Tables;

namespace HolcombeScores.Models
{
    public class Game : ITableEntity
    {
        public Guid TeamId { get; set; }
        public Guid Id { get; set; }
        public string Opponent { get; set; }
        public bool PlayingAtHome { get; set; }
        public DateTime Date { get; set; }

        /// <summary>
        /// TeamId
        /// </summary>
        public string PartitionKey { get; set; }

        /// <summary>
        /// Id
        /// </summary>
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
