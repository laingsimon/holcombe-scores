using System.Threading.Tasks;
using HolcombeScores.Api.Models;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface INewGameDtoAdapter
    {
        Task<Game> AdaptToGame(NewGameDto newGameDto, ActionResultDto<GameDto> actionResult);
    }
}