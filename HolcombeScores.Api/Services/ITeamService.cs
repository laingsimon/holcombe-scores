using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services
{
    public interface ITeamService
    {
        IAsyncEnumerable<TeamDto> GetAllTeams();
        Task<ActionResultDto<TeamDto>> CreateTeam(TeamDto teamDto);
        Task<ActionResultDto<TeamDto>> UpdateTeam(TeamDto teamDto);
        Task<ActionResultDto<TeamDto>> DeleteTeam(Guid teamId);
        Task<TeamDto> GetTeam(Guid id);
    }
}
