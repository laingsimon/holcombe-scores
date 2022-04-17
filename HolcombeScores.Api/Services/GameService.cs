using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HolcombeScores.Api.Models;
using HolcombeScores.Api.Repositories;
using HolcombeScores.Api.Services.Adapters;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly IGameDtoAdapter _gameDtoAdapter;
        private readonly IAccessService _accessService;
        private readonly INewGameDtoAdapter _newGameDtoAdapter;
        private readonly IGoalDtoAdapter _goalDtoAdapter;

        public GameService(
            IGameRepository gameRepository,
            IGameDtoAdapter gameDtoAdapter,
            IAccessService accessService,
            INewGameDtoAdapter newGameDtoAdapter,
            IGoalDtoAdapter goalDtoAdapter)
        {
            _gameRepository = gameRepository;
            _gameDtoAdapter = gameDtoAdapter;
            _accessService = accessService;
            _newGameDtoAdapter = newGameDtoAdapter;
            _goalDtoAdapter = goalDtoAdapter;
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
                var goals = await _gameRepository.GetGoals(game.Id);
                yield return _gameDtoAdapter.Adapt(game, gamePlayers, goals);
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
            var goals = await _gameRepository.GetGoals(game.Id);
            return _gameDtoAdapter.Adapt(game, gamePlayers, goals);
        }

        public async Task<ActionResultDto<GameDto>> CreateGame(NewGameDto newGameDto)
        {
            if (!await _accessService.CanAccessTeam(newGameDto.TeamId))
            {
                return NotPermitted("Not permitted to interact with this team");
            }

            try
            {
                // TODO: Add Validation

                var result = new ActionResultDto<GameDto>();
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

                result.Success = true;
                result.Messages.Add("Game created");
                result.Outcome = _gameDtoAdapter.Adapt(game, gamePlayers, new Goal[0]);
                return result;
            }
            catch (Exception exc)
            {
                return Error(exc.ToString());
            }
        }

        public async Task<ActionResultDto<GameDto>> RecordGoal(GoalDto goalDto)
        {
            var access = await _accessService.GetAccess();
            if (access == null || access.Revoked != null)
            {
                return NotLoggedIn();
            }

            var goal = _goalDtoAdapter.Adapt(goalDto);

            var game = await _gameRepository.Get(goal.GameId);
            if (game == null)
            {
                return NotFound("Game not found");
            }

            if (!await _accessService.CanAccessTeam(game.TeamId))
            {
                return NotPermitted("Not permitted to interact with this team");
            }

            await _gameRepository.AddGoal(goal);

            return Success("Goal recorded", await GetGame(goal.GameId));
        }

        private static ActionResultDto<GameDto> Success(string message, GoalDto outcome = null)
        {
           return new ActionResultDto<GameDto>
           {
               Messages =
               {
                   message,
               },
               Outcome = outcome,
               Success = true,
           };
        }

        private static ActionResultDto<GameDto> Error(string error)
        {
           return new ActionResultDto<GameDto>
           {
               Errors =
               {
                   error,
               }
           };
        }

        private static ActionResultDto<GameDto> NotFound(string message)
        {
           return new ActionResultDto<GameDto>
           {
               Warnings =
               {
                   message,
               },
           };
        }

        private static ActionResultDto<GameDto> NotAnAdmin()
        {
           return new ActionResultDto<GameDto>
           {
               Errors =
               {
                   "Not an admin",
               },
           };
        }

        private static ActionResultDto<GameDto> NotPermitted(string message)
        {
           return new ActionResultDto<GameDto>
           {
               Errors =
               {
                   message,
               },
           };
        }

        private static ActionResultDto<GameDto> NotLoggedIn()
        {
           return new ActionResultDto<GameDto>
           {
               Warnings =
               {
                   "Not logged in",
               },
           };
        }
    }
}
