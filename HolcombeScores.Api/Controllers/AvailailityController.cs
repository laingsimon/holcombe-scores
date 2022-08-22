using HolcombeScores.Api.Models.Dtos;
using HolcombeScores.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HolcombeScores.Api.Controllers;

[ApiController]
public class AvailabilityController : Controller
{
    private readonly IAvailabilityService _availabilityService;

    public AvailabilityController(IAvailabilityService availabilityService)
    {
        _availabilityService = availabilityService;
    }

    [HttpGet("/api/Availability/{teamId}/{gameId}")]
    public IAsyncEnumerable<AvailabilityDto> ListAvailability(Guid teamId, Guid gameId)
    {
        return _availabilityService.GetAvailability(teamId, gameId);
    }

    [HttpPost("/api/Availability")]
    public async Task<ActionResultDto<AvailabilityDto>> UpdateAvailability(UpdateAvailabilityDto availabilityDto)
    {
        return await _availabilityService.UpdateAvailability(availabilityDto);
    }

    [HttpDelete("/api/Availability")]
    public async Task<ActionResultDto<string>> DeleteAvailability(UpdateAvailabilityDto availabilityDto)
    {
        return await _availabilityService.RemoveAvailability(availabilityDto);
    }
}