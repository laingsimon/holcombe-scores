using HolcombeScores.Api.Models;
using HolcombeScores.Models;

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

            return new Player
            {
                Coach = team.Coach,
                Name = team.Name,
                Id = team.Id,
            };
        }
    }
}
