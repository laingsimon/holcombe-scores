using HolcombeScores.Api.Models.Dtos;
using HolcombeScores.Api.Repositories;
using HolcombeScores.Api.Services.Adapters;

namespace HolcombeScores.Api.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepository;
        private readonly ITeamDtoAdapter _teamDtoAdapter;
        private readonly IAccessService _accessService;
        private readonly IPlayerRepository _playerRepository;
        private readonly IServiceHelper _serviceHelper;

        public TeamService(
            ITeamRepository teamRepository,
            ITeamDtoAdapter teamDtoAdapter,
            IAccessService accessService,
            IPlayerRepository playerRepository,
            IServiceHelper serviceHelper)
        {
            _teamRepository = teamRepository;
            _teamDtoAdapter = teamDtoAdapter;
            _accessService = accessService;
            _playerRepository = playerRepository;
            _serviceHelper = serviceHelper;
        }

        public async IAsyncEnumerable<TeamDto> GetAllTeams()
        {
            var access = await _accessService.GetAccess();
            if (access?.Revoked != null)
            {
                // if someone has revoked access don't permit them access to the list of teams
                yield break;
            }

            await foreach (var team in _teamRepository.GetAll())
            {
                yield return _teamDtoAdapter.Adapt(team);
            }
        }

        public async Task<TeamDto> GetTeam(Guid id)
        {
            var access = await _accessService.GetAccess();
            if (access?.Revoked != null)
            {
                // if someone has revoked access don't permit them access to the list of teams
                return null;
            }

            return _teamDtoAdapter.Adapt(await _teamRepository.Get(id));
        }

        public async Task<ActionResultDto<TeamDto>> CreateTeam(TeamDto teamDto)
        {
            if (!await _accessService.IsAdmin())
            {
                return _serviceHelper.NotAnAdmin<TeamDto>();
            }

            var team = _teamDtoAdapter.Adapt(teamDto);

            var existingTeams = (await _teamRepository.GetAll().ToEnumerable()).Where(t => t.Name == team.Name).ToArray();

            if (existingTeams.Any())
            {
                return _serviceHelper.NotSuccess<TeamDto>("Team exists with this name already");
            }

            team.Id = Guid.NewGuid();
            await _teamRepository.CreateTeam(team);

            return _serviceHelper.Success("Team created", _teamDtoAdapter.Adapt(team));
        }

        public async Task<ActionResultDto<TeamDto>> UpdateTeam(TeamDto teamDto)
        {
            if (!await _accessService.IsAdmin())
            {
                return _serviceHelper.NotAnAdmin<TeamDto>();
            }

            var updatedTeam = _teamDtoAdapter.Adapt(teamDto);

            var existingTeam = (await _teamRepository.GetAll().ToEnumerable()).SingleOrDefault(t => t.Id == updatedTeam.Id);

            if (existingTeam == null)
            {
                return _serviceHelper.NotFound<TeamDto>("Team not found");
            }

            existingTeam.Name = updatedTeam.Name;
            existingTeam.Coach = updatedTeam.Coach;
            await _teamRepository.UpdateTeam(existingTeam);

            return _serviceHelper.Success("Team updated", _teamDtoAdapter.Adapt(existingTeam));
        }

        public async Task<ActionResultDto<TeamDto>> DeleteTeam(Guid teamId)
        {
            if (!await _accessService.IsAdmin())
            {
                return _serviceHelper.NotAnAdmin<TeamDto>();
            }

            var teamToDelete = (await _teamRepository.GetAll().ToEnumerable()).SingleOrDefault(t => t.Id == teamId);

            if (teamToDelete == null)
            {
                return _serviceHelper.NotFound<TeamDto>("Team not found");
            }

            var playersToDelete = _playerRepository.GetAll(teamId);
            await foreach (var playerToDelete in playersToDelete)
            {
                await _playerRepository.DeletePlayer(playerToDelete.Id);
            }

            await _teamRepository.DeleteTeam(teamToDelete.Id);
            return _serviceHelper.Success("Team and players deleted", _teamDtoAdapter.Adapt(teamToDelete));
        }
    }
}
