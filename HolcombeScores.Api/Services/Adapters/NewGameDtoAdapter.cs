using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HolcombeScores.Api.Models;
using HolcombeScores.Api.Repositories;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services.Adapters
{
    public class NewGameDtoAdapter : INewGameDtoAdapter
    {
        private readonly IPlayerRepository _playerRepository;

        public NewGameDtoAdapter(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public async Task<Game> AdaptToGame(NewGameDto newGameDto, ActionResultDto<GameDto> actionResult)
        {
            return new Game
            {
                Date = newGameDto.Date ?? DateTime.Today,
                Goals = Array.Empty<Goal>(),
                Id = Guid.NewGuid(),
                Opponent = newGameDto.Opponent,
                Squad = await ToArray(AdaptSquad(newGameDto, actionResult)),
                PlayingAtHome = newGameDto.PlayingAtHome,
            };
        }

        private async IAsyncEnumerable<Player> AdaptSquad(NewGameDto newGameDto, ActionResultDto<GameDto> actionResult)
        {
            var knownPlayersLookup = new Dictionary<string, Player>();
            await foreach (var player in _playerRepository.GetAll(newGameDto.TeamId))
            {
                knownPlayersLookup.Add(player.Name, player);
            }

            foreach (var player in newGameDto.Players)
            {
                if (knownPlayersLookup.TryGetValue(player, out var knownPlayer))
                {
                    yield return knownPlayer;
                    continue;
                }

                actionResult.Warnings.Add($"Player not found: `{player}`");
            }
        }

        private static async Task<T[]> ToArray<T>(IAsyncEnumerable<T> asyncEnumerable)
        {
            var list = new List<T>();

            await foreach (var item in asyncEnumerable)
            {
                list.Add(item);
            }

            return list.ToArray(); // TODO Improve this, don't convert a list to an array, it copies the memory around.
        }
    }
}