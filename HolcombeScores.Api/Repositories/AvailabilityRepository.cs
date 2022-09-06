using Azure;
using HolcombeScores.Api.Models.AzureTables;

namespace HolcombeScores.Api.Repositories;

public class AvailabilityRepository : IAvailabilityRepository
{
    private readonly TypedTableClient<Availability> _availabilityTableClient;

    public AvailabilityRepository(ITableClientFactory tableClientFactory)
    {
        _availabilityTableClient = new TypedTableClient<Availability>(tableClientFactory.CreateTableClient("Availability"));
    }

    public async Task<Availability> GetAvailability(Guid teamId, Guid gameId, Guid playerId)
    {
        return await _availabilityTableClient.SingleOrDefaultAsync(a => a.TeamId == teamId && a.GameId == gameId && a.PlayerId == playerId);
    }

    public async Task CreateAvailability(Availability availability)
    {
        availability.Id = Guid.NewGuid();
        availability.PartitionKey = availability.GameId.ToString();
        availability.RowKey = availability.PlayerId.ToString();
        availability.ETag = ETag.All;
        availability.Timestamp = DateTimeOffset.UtcNow;
        await _availabilityTableClient.AddEntityAsync(availability);
    }

    public async Task UpdateAvailability(Availability availability)
    {
        availability.PartitionKey = availability.GameId.ToString();
        availability.RowKey = availability.PlayerId.ToString();
        availability.Timestamp = DateTimeOffset.UtcNow;
        await _availabilityTableClient.UpdateEntityAsync(availability, ETag.All);
    }

    public async Task DeleteAvailability(Availability availability)
    {
        await _availabilityTableClient.DeleteEntityAsync(availability.GameId.ToString(), availability.RowKey);
    }

    public IAsyncEnumerable<Availability> GetAvailability(Guid teamId, Guid gameId)
    {
        return _availabilityTableClient.QueryAsync(a => a.TeamId == teamId && a.GameId == gameId);
    }
}