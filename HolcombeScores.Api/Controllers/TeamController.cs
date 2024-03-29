using HolcombeScores.Api.Models.Dtos;
using HolcombeScores.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HolcombeScores.Api.Controllers
{
    [ApiController]
    public class TeamController : Controller
    {
        private readonly ITeamService _teamService;

        public TeamController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        [HttpGet("/api/Teams")]
        public IAsyncEnumerable<TeamDto> List()
        {
            return _teamService.GetAllTeams();
        }

        [HttpGet("/api/Team/{id}")]
        public async Task<TeamDto> GetTeam(Guid id)
        {
            return await _teamService.GetTeam(id);
        }

        [HttpPost("/api/Team")]
        public async Task<ActionResultDto<TeamDto>> Create(TeamDto teamDto)
        {
            return await _teamService.CreateTeam(teamDto);
        }

        [HttpPatch("/api/Team")]
        public async Task<ActionResultDto<TeamDto>> Update(TeamDto teamDto)
        {
            return await _teamService.UpdateTeam(teamDto);
        }

        [HttpDelete("/api/Team/{id}")]
        public async Task<ActionResultDto<TeamDto>> Create(Guid id)
        {
            return await _teamService.DeleteTeam(id);
        }
    }
}
