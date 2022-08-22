using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services.Adapters;

public interface IAvailabilityDtoAdapter
{
    Task<AvailabilityDto> Adapt(Game game, Team team, Player player, Availability availability);
}