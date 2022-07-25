using System;
using Azure;
using Azure.Data.Tables;

namespace HolcombeScores.Api.Models.AzureTables
{
    public class Goal : ITableEntity
    {
        public DateTime Time { get; set; }
        public bool HolcombeGoal { get; set; }
        public int? PlayerNumber { get; set; }
        public Guid? TeamId { get; set; }
        public Guid GameId { get; set; }
        public Guid GoalId { get; set; }

        /// <summary>
        /// GameId
        /// </summary>
        public string PartitionKey { get; set; }

        /// <summary>
        /// GoalId
        /// </summary>
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
