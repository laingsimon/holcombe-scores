using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;
using HolcombeScores.Api.Repositories;

namespace HolcombeScores.Api.Services.Adapters;

public class AvailabilityDtoAdapter : IAvailabilityDtoAdapter
{
    private readonly ITeamDtoAdapter _teamDtoAdapter;
    private readonly IPlayerDtoAdapter _playerDtoAdapter;
    private readonly IAccessDtoAdapter _accessDtoAdapter;
    private readonly IAccessRepository _accessRepository;

    public AvailabilityDtoAdapter(
        ITeamDtoAdapter teamDtoAdapter,
        IPlayerDtoAdapter playerDtoAdapter,
        IAccessDtoAdapter accessDtoAdapter,
        IAccessRepository accessRepository)
    {
        _teamDtoAdapter = teamDtoAdapter;
        _playerDtoAdapter = playerDtoAdapter;
        _accessDtoAdapter = accessDtoAdapter;
        _accessRepository = accessRepository;
    }

    public async Task<AvailabilityDto> Adapt(Game game, Team team, Player player, Availability availability)
    {
        var reportedBy = availability == null
            ? null
            : _accessDtoAdapter.Adapt(await _accessRepository.GetAccess(availability.ReportedByUserId));

        return new AvailabilityDto
        {
            Available = availability?.Available,
            GameId = game.Id,
            Id = availability?.Id,
            Player = _playerDtoAdapter.Adapt(player),
            ReportedBy = reportedBy,
            Team = _teamDtoAdapter.Adapt(team),
        };
    }
}