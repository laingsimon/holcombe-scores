using HolcombeScores.Api.Models.AzureTables;

namespace HolcombeScores.Api.Repositories;

public interface IAvailabilityRepository
{
    Task<Availability> GetAvailability(Guid teamId, Guid gameId, Guid playerId);
    IAsyncEnumerable<Availability> GetAvailability(Guid teamId, Guid gameId);
    Task CreateAvailability(Availability availability);
    Task UpdateAvailability(Availability availability);
    Task DeleteAvailability(Availability availability);
}