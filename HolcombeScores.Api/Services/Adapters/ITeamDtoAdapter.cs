using HolcombeScores.Api.Models;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface ITeamDtoAdapter
    {
        TeamDto Adapt(Team team);
        Team Adapt(TeamDto team);
    }
}
