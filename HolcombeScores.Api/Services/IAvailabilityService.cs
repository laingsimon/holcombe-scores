using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services;

public interface IAvailabilityService
{
    IAsyncEnumerable<AvailabilityDto> GetAvailability(Guid teamId, Guid gameId);
    Task<ActionResultDto<AvailabilityDto>> UpdateAvailability(UpdateAvailabilityDto request);
    Task<ActionResultDto<string>> RemoveAvailability(UpdateAvailabilityDto request);
}