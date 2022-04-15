using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HolcombeScores.Api.Models;
using HolcombeScores.Api.Repositories;
using HolcombeScores.Api.Services.Adapters;

namespace HolcombeScores.Api.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly IGameDtoAdapter _gameDtoAdapter;
        private readonly IAccessService _accessService;
        private readonly INewGameDtoAdapter _newGameDtoAdapter;

        public GameService(
            IGameRepository gameRepository,
            IGameDtoAdapter gameDtoAdapter,
            IAccessService accessService,
            INewGameDtoAdapter newGameDtoAdapter)
        {
            _gameRepository = gameRepository;
            _gameDtoAdapter = gameDtoAdapter;
            _accessService = accessService;
            _newGameDtoAdapter = newGameDtoAdapter;
        }

        public async IAsyncEnumerable<GameDto> GetAllGames()
        {
            var access = await _accessService.GetAccess();
            if (access == null || access.Revoked != null)
            {
                yield break;
            }

            await foreach (var game in _gameRepository.GetAll(access.Admin ? null : access.TeamId))
            {
                var gamePlayers = await _gameRepository.GetPlayers(game.Id);
                yield return _gameDtoAdapter.Adapt(game, gamePlayers);
            }
        }

        public async Task<GameDto> GetGame(Guid id)
        {
            var access = await _accessService.GetAccess();
            if (access == null || access.Revoked != null)
            {
                return null;
            }

            var game = await _gameRepository.Get(id);
            if (game != null && !await _accessService.CanAccessTeam(game.TeamId))
            {
                return null;
            }

            var gamePlayers = await _gameRepository.GetPlayers(game.Id);
            return _gameDtoAdapter.Adapt(game, gamePlayers);
        }

        public async Task<ActionResultDto<GameDto>> CreateGame(NewGameDto newGameDto)
        {
            var result = new ActionResultDto<GameDto>();
            if (!await _accessService.CanAccessTeam(newGameDto.TeamId))
            {
                result.Errors.Add("Not permitted to interact with team");
                return result;
            }

            try
            {
                // TODO: Add Validation

                var game = await _newGameDtoAdapter.AdaptToGame(newGameDto, result);
                game.Id = Guid.NewGuid();
                var squad = _newGameDtoAdapter.AdaptSquad(newGameDto, game.Id, result);
                await _gameRepository.Add(game);

                var gamePlayers = new List<GamePlayer>();
                await foreach (var gamePlayer in squad)
                {
                    await _gameRepository.AddGamePlayer(gamePlayer);
                    gamePlayers.Add(gamePlayer);
                }

                result.Outcome = _gameDtoAdapter.Adapt(game, gamePlayers);
                result.Success = true;
            }
            catch (Exception exc)
            {
                result.Errors.Add(exc.ToString());
            }

            return result;
        }
    }
}
