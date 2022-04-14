using System.Collections.Generic;
using System.Threading.Tasks;
using HolcombeScores.Api.Models;
using HolcombeScores.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HolcombeScores.Api.Controllers
{
    [ApiController]
    public class TeamController : Controller
    {
        private readonly ITeamService _teamService;

        public PlayerController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        [HttpGet("/api/Teams")]
        public IAsyncEnumerable<TeamDto> List()
        {
            return _teamsService.GetAllTeams();
        }
    }
}
