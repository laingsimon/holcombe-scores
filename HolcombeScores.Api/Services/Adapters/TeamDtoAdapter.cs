using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services.Adapters
{
    public class TeamDtoAdapter : ITeamDtoAdapter
    {
        public TeamDto Adapt(Team team)
        {
            if (team == null)
            {
                return null;
            }

            return new TeamDto
            {
                Coach = team.Coach,
                Name = team.Name,
                Id = team.Id,
            };
        }

        public Team Adapt(TeamDto team)
        {
            if (team == null)
            {
                return null;
            }

            return new Team
            {
                Coach = team.Coach,
                Name = team.Name,
                Id = team.Id,
            };
        }
    }
}
