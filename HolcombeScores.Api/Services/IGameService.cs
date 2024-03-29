using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services
{
    public interface IGameService
    {
        Task<ActionResultDto<GameDto>> CreateGame(GameDetailsDto gameDetailsDto);
        IAsyncEnumerable<GameDto> GetAllGames(Guid teamId);
        Task<GameDto> GetGame(Guid id);
        Task<ActionResultDto<GameDto>> DeleteGame(Guid id);
        Task<ActionResultDto<GameDto>> DeleteGamePlayer(Guid gameId, Guid playerId);
        Task<ActionResultDto<GameDto>> DeleteGoal(Guid gameId, Guid goalId);
        Task<ActionResultDto<GameDto>> RecordGoal(GoalDto goalDto);
        Task<ActionResultDto<GameDto>> UpdateGame(ExistingGameDetailsDto gameDetailsDto);
    }
}
