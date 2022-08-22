using Azure;
using Azure.Data.Tables;

namespace HolcombeScores.Api.Models.AzureTables;

public class Availability : ITableEntity
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public Guid GameId { get; set; }
    public Guid PlayerId { get; set; }
    public bool Available { get; set; }
    public Guid ReportedByUserId { get; set; }

    /// <summary>
    /// The same as GameId
    /// </summary>
    public string PartitionKey { get; set; }

    /// <summary>
    /// The same as PlayerId
    /// </summary>
    public string RowKey { get; set; }
    public ETag ETag { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
}