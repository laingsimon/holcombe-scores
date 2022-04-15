using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HolcombeScores.Api.Models;
using HolcombeScores.Api.Repositories;
using HolcombeScores.Api.Services.Adapters;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepository;
        private readonly ITeamDtoAdapter _teamDtoAdapter;
        private readonly IAccessService _accessService;
        private readonly IPlayerRepository _playerRepository;

        public TeamService(
            ITeamRepository teamRepository,
            ITeamDtoAdapter teamDtoAdapter,
            IAccessService accessService,
            IPlayerRepository playerRepository)
        {
            _teamRepository = teamRepository;
            _teamDtoAdapter = teamDtoAdapter;
            _accessService = accessService;
            _playerRepository = playerRepository;
        }

        public async IAsyncEnumerable<TeamDto> GetAllTeams()
        {
            var access = await _accessService.GetAccess();
            if (access == null || access.Revoked != null)
            {
                yield break;
            }

            await foreach (var team in _teamRepository.GetAll())
            {
                yield return _teamDtoAdapter.Adapt(team);
            }
        }

        public async Task<ActionResultDto<TeamDto>> CreateTeam(TeamDto teamDto)
        {
            if (!await _accessService.IsAdmin())
            {
                return NotAnAdmin();
            }

            var team = _teamDtoAdapter.Adapt(teamDto);

            var existingTeams = (await GetTeamsMatching(t => t.Name == team.Name)).ToArray();

            if (existingTeams.Any())
            {
                return NotSuccess("Team exists with this name already");
            }

            team.Id = Guid.NewGuid();
            team.PartitionKey = team.Id.ToString();
            team.RowKey = team.Id.ToString();
            team.ETag = ETag.All;
            await _teamRepository.CreateTeam(team);

            return Success("Team created", _teamDtoAdapter.Adapt(team));
        }

        public async Task<ActionResultDto<TeamDto>> UpdateTeam(TeamDto teamDto)
        {
            if (!await _accessService.IsAdmin())
            {
                return NotAnAdmin();
            }

            var updatedTeam = _teamDtoAdapter.Adapt(teamDto);

            var existingTeam = (await GetTeamsMatching(t => t.Id == updatedTeam.Id)).SingleOrDefault();

            if (existingTeam == null)
            {
                return NotFound("Team not found");
            }

            existingTeam.Name = updatedTeam.Name;
            existingTeam.Coach = updatedTeam.Coach;
            await _teamRepository.UpdateTeam(existingTeam);

            return Success("Team updated", _teamDtoAdapter.Adapt(existingTeam));
        }

        public async Task<ActionResultDto<TeamDto>> DeleteTeam(Guid id)
        {
            if (!await _accessService.IsAdmin())
            {
                return NotAnAdmin();
            }

            var teamToDelete = (await GetTeamsMatching(t => t.Id == id)).SingleOrDefault();

            if (teamToDelete == null)
            {
                return NotFound("Team not found");
            }

            var playersToDelete = _playerRepository.GetAll(teamToDelete.Id);
            await foreach (var playerToDelete in playersToDelete)
            {
                await _playerRepository.DeletePlayer(teamToDelete.Id, playerToDelete.Number);
            }

            await _teamRepository.DeleteTeam(teamToDelete.Id);
            return Success("Team and players deleted", _teamDtoAdapter.Adapt(teamToDelete));
        }

        private static ActionResultDto<TeamDto> Success(string message, TeamDto outcome = null)
        {
           return new ActionResultDto<TeamDto>
           {
               Messages =
               {
                   message,
               },
               Outcome = outcome,
               Success = true,
           };
        }

        private static ActionResultDto<TeamDto> NotSuccess(string message)
        {
           return new ActionResultDto<TeamDto>
           {
               Messages =
               {
                   message,
               }
           };
        }

        private static ActionResultDto<TeamDto> NotFound(string message)
        {
           return new ActionResultDto<TeamDto>
           {
               Warnings =
               {
                   message,
               },
           };
        }

        private static ActionResultDto<TeamDto> NotAnAdmin()
        {
           return new ActionResultDto<TeamDto>
           {
               Errors =
               {
                   "Not an admin",
               },
           };
        }

        private static ActionResultDto<TeamDto> NotPermitted(string message)
        {
           return new ActionResultDto<TeamDto>
           {
               Errors =
               {
                   message,
               },
           };
        }

        private static ActionResultDto<TeamDto> NotLoggedIn()
        {
           return new ActionResultDto<TeamDto>
           {
               Warnings =
               {
                   "Not logged in",
               },
           };
        }

        private async Task<IEnumerable<Team>> GetTeamsMatching(Predicate<Team> predicate)
        {
            var teams = new List<Team>();
            await foreach (var team in _teamRepository.GetAll())
            {
                if (predicate(team))
                {
                    teams.Add(team);
                }
            }

            return teams;
        }
    }
}
