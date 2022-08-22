using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;
using HolcombeScores.Api.Repositories;
using HolcombeScores.Api.Services.Adapters;

namespace HolcombeScores.Api.Services;

public class AvailabilityService : IAvailabilityService
{
    private readonly IAccessService _accessService;
    private readonly IAvailabilityRepository _availabilityRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IAvailabilityDtoAdapter _availabilityDtoAdapter;
    private readonly ITeamRepository _teamRepository;
    private readonly IGameRepository _gameRepository;
    private readonly IServiceHelper _serviceHelper;

    public AvailabilityService(
        IAccessService accessService,
        IAvailabilityRepository availabilityRepository,
        IPlayerRepository playerRepository,
        IAvailabilityDtoAdapter availabilityDtoAdapter,
        ITeamRepository teamRepository,
        IGameRepository gameRepository,
        IServiceHelper serviceHelper)
    {
        _accessService = accessService;
        _availabilityRepository = availabilityRepository;
        _playerRepository = playerRepository;
        _availabilityDtoAdapter = availabilityDtoAdapter;
        _teamRepository = teamRepository;
        _gameRepository = gameRepository;
        _serviceHelper = serviceHelper;
    }

    public async Task<ActionResultDto<AvailabilityDto>> UpdateAvailability(UpdateAvailabilityDto request)
    {
        if (!await _accessService.CanAccessTeam(request.TeamId))
        {
            return _serviceHelper.NotPermitted<AvailabilityDto>("No access to team");
        }

        var team = await _teamRepository.Get(request.TeamId);
        if (team == null)
        {
            return _serviceHelper.NotFound<AvailabilityDto>("Team not found");
        }

        var game = await _gameRepository.Get(request.GameId);
        if (game == null)
        {
            return _serviceHelper.NotFound<AvailabilityDto>("Game not found");
        }

        var player = await _playerRepository.Get(request.PlayerId);
        if (player == null)
        {
            return _serviceHelper.NotFound<AvailabilityDto>("Player not found");
        }

        var currentAvailability = await _availabilityRepository.GetAvailability(request.TeamId, request.GameId, request.PlayerId);
        if (currentAvailability != null)
        {
            // update
            currentAvailability.Available = request.Available;
            currentAvailability.ReportedByUserId = (await _accessService.GetAccess()).UserId;
            await _availabilityRepository.UpdateAvailability(currentAvailability);
            return _serviceHelper.Success("Availability updated", await _availabilityDtoAdapter.Adapt(game, team, player, currentAvailability));
        }

        var availability = new Availability
        {
            Id = Guid.NewGuid(),
            Available = request.Available,
            GameId = request.GameId,
            PlayerId = request.PlayerId,
            TeamId = request.TeamId,
            ReportedByUserId = (await _accessService.GetAccess()).UserId,
        };
        await _availabilityRepository.CreateAvailability(availability);

        return _serviceHelper.Success("Availability recorded", await _availabilityDtoAdapter.Adapt(game, team, player, availability));
    }

    public async Task<ActionResultDto<string>> RemoveAvailability(UpdateAvailabilityDto request)
    {
        if (!await _accessService.CanAccessTeam(request.TeamId))
        {
            return _serviceHelper.NotPermitted<string>("No access to team");
        }

        var team = await _teamRepository.Get(request.TeamId);
        if (team == null)
        {
            return _serviceHelper.NotFound<string>("Team not found");
        }

        var game = await _gameRepository.Get(request.GameId);
        if (game == null)
        {
            return _serviceHelper.NotFound<string>("Game not found");
        }

        var player = await _playerRepository.Get(request.PlayerId);
        if (player == null)
        {
            return _serviceHelper.NotFound<string>("Player not found");
        }

        var currentAvailability = await _availabilityRepository.GetAvailability(request.TeamId, request.GameId, request.PlayerId);
        if (currentAvailability == null)
        {
            return _serviceHelper.Success("Availability updated", "Availability had already been removed");
        }

        await _availabilityRepository.DeleteAvailability(currentAvailability);
        return _serviceHelper.Success("Availability updated", "Availability deleted");
    }

    public async IAsyncEnumerable<AvailabilityDto> GetAvailability(Guid teamId, Guid gameId)
    {
        if (!await _accessService.CanAccessTeam(teamId))
        {
            yield break;
        }

        var team = await _teamRepository.Get(teamId);
        if (team == null)
        {
            yield break;
        }

        var game = await _gameRepository.Get(gameId);
        if (game == null)
        {
            yield break;
        }

        var currentAvailability = await _availabilityRepository.GetAvailability(teamId, gameId).ToArrayAsync();

        await foreach (var player in _playerRepository.GetAll(teamId))
        {
            var availability = currentAvailability.SingleOrDefault(a => a.PlayerId == player.Id);
            yield return await _availabilityDtoAdapter.Adapt(game, team, player, availability);
        }
    }
}