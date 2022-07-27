using Azure;
using Azure.Data.Tables;

namespace HolcombeScores.Api.Models.AzureTables
{
    public class Player : ITableEntity
    {
        public Guid TeamId { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }
        public Guid Id { get; set; }

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