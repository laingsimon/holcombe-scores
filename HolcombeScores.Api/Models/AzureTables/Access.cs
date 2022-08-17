using System.Runtime.Serialization;
using Azure;
using Azure.Data.Tables;

namespace HolcombeScores.Api.Models.AzureTables
{
    public class Access : ITableEntity
    {
        [IgnoreDataMember]
        public Guid[] Teams { get; set; }
        public DateTime Granted { get; set; }
        public DateTime? Revoked { get; set; }
        public Guid UserId { get; set; }
        public bool Admin { get; set; }
        public bool Manager { get; set; }
        public string Name { get; set; }
        public string RevokedReason { get; set; }
        public string Token { get; set; }

        [DataMember(Name = "Teams")]
        [Obsolete("Don't use this property directly, use " + nameof(TeamsList) + " instead")]
        public string TeamsList
        {
            get => string.Join(",", Teams ?? Array.Empty<Guid>());
            set =>
                Teams = string.IsNullOrEmpty(value)
                    ? Array.Empty<Guid>()
                    : value.Split(",").Select(Guid.Parse).ToArray();
        }

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
