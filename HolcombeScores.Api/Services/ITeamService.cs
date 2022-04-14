using System.Collections.Generic;
using System.Threading.Tasks;
using HolcombeScores.Api.Models;

namespace HolcombeScores.Api.Services
{
    public interface ITeamService
    {
        IAsyncEnumerable<TeamDto> GetAllTeams();
    }
}
