using Azure;
using Azure.Data.Tables;

namespace HolcombeScores.Api.Models.AzureTables
{
    public class Game : ITableEntity
    {
        public Guid TeamId { get; set; }
        public Guid Id { get; set; }
        public string Opponent { get; set; }
        public bool PlayingAtHome { get; set; }
        public DateTime Date { get; set; }
        public bool Training { get; set; }
        public bool Postponed { get; set; }
        public string Address { get; set; }
        public bool Friendly { get; set; }

        /// <summary>
        /// Manager awarded player of the session
        /// </summary>
        public Guid? ManagerPots { get; set; }
        /// <summary>
        /// Supporter awarded player of the session
        /// </summary>
        public Guid? SupporterPots { get; set; }
        /// <summary>
        /// Player awarded player of the session
        /// </summary>
        public Guid? PlayerPots { get; set; }

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
