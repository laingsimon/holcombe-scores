using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HolcombeScores.Api.Models;
using HolcombeScores.Api.Repositories;
using HolcombeScores.Api.Services.Adapters;

namespace HolcombeScores.Api.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _teamRepository;
        private readonly ITeamDtoAdapter _teamDtoAdapter;
        private readonly IAccessService _accessService;

        public TeamService(ITeamRepository teamRepository, ITeamDtoAdapter teamDtoAdapter, IAccessService accessService)
        {
            _teamRepository = teamRepository;
            _teamDtoAdapter = teamDtoAdapter;
            _accessService = accessService;
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
    }
}
