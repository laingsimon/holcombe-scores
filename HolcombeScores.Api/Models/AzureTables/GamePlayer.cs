using Azure;
using Azure.Data.Tables;

namespace HolcombeScores.Api.Models.AzureTables
{
    public class GamePlayer : ITableEntity
    {
        public Guid TeamId { get; set; }
        public string Name { get; set; }
        public int? Number { get; set; }
        public Guid GameId { get; set; }
        public Guid PlayerId { get; set; }

        /// <summary>
        /// GameId
        /// </summary>
        public string PartitionKey { get; set; }
        /// <summary>
        /// (Player) Number
        /// </summary>
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
