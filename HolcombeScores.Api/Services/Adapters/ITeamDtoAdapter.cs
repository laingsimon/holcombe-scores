using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface ITeamDtoAdapter
    {
        TeamDto Adapt(Team team);
        Team Adapt(TeamDto team);
    }
}
