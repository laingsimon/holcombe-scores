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
        private readonly ITeamRepository _teamRepository;
        private readonly IServiceHelper _serviceHelper;

        public GameService(
            IGameRepository gameRepository,
            IGameDtoAdapter gameDtoAdapter,
            IAccessService accessService,
            INewGameDtoAdapter newGameDtoAdapter,
            IGoalDtoAdapter goalDtoAdapter,
            ITeamRepository teamRepository,
            IServiceHelper serviceHelper)
        {
            _gameRepository = gameRepository;
            _gameDtoAdapter = gameDtoAdapter;
            _accessService = accessService;
            _newGameDtoAdapter = newGameDtoAdapter;
            _goalDtoAdapter = goalDtoAdapter;
            _teamRepository = teamRepository;
            _serviceHelper = serviceHelper;
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
            if (game == null || !await _accessService.CanAccessTeam(game.TeamId))
            {
                return null;
            }

            var gamePlayers = await _gameRepository.GetPlayers(game.Id);
            var goals = await _gameRepository.GetGoals(game.Id);
            return _gameDtoAdapter.Adapt(game, gamePlayers, goals);
        }

        public async Task<ActionResultDto<GameDto>> CreateGame(NewGameDto newGameDto)
        {
            if (await _teamRepository.Get(newGameDto.TeamId) == null)
            {
                return _serviceHelper.NotFound<GameDto>("Team not found");
            }

            if (!await _accessService.CanAccessTeam(newGameDto.TeamId))
            {
                return _serviceHelper.NotPermitted<GameDto>("Not permitted to interact with this team");
            }

            try
            {
                // TODO: Add Validation

                var result = new ActionResultDto<GameDto>();
                var game = _newGameDtoAdapter.AdaptToGame(newGameDto, result);
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
                result.Outcome = _gameDtoAdapter.Adapt(game, gamePlayers, Array.Empty<Goal>());
                return result;
            }
            catch (Exception exc)
            {
                return _serviceHelper.Error<GameDto>(exc.ToString());
            }
        }

        public async Task<ActionResultDto<GameDto>> RecordGoal(GoalDto goalDto)
        {
            var access = await _accessService.GetAccess();
            if (access == null || access.Revoked != null)
            {
                return _serviceHelper.NotLoggedIn<GameDto>();
            }

            var goal = _goalDtoAdapter.Adapt(goalDto);

            var game = await _gameRepository.Get(goal.GameId);
            if (game == null)
            {
                return _serviceHelper.NotFound<GameDto>("Game not found");
            }

            if (!await _accessService.CanAccessTeam(game.TeamId))
            {
                return _serviceHelper.NotPermitted<GameDto>("Not permitted to interact with this team");
            }

            await _gameRepository.AddGoal(goal);

            return _serviceHelper.Success("Goal recorded", await GetGame(goal.GameId));
        }

        public async Task<ActionResultDto<GameDto>> DeleteGame(Guid id)
        {
            var game = await GetGame(id);
            if (game == null)
            {
                return _serviceHelper.NotFound<GameDto>("Game not found");
            }

            await _gameRepository.DeleteGame(id);

            return _serviceHelper.Success("Game deleted", game);
        }

        public async Task<ActionResultDto<GameDto>> DeleteGamePlayer(Guid gameId, int playerNumber)
        {
            var game = await GetGame(gameId);
            if (game == null)
            {
                return _serviceHelper.NotFound<GameDto>("Game not found");
            }

            await _gameRepository.DeleteGamePlayer(gameId, playerNumber);

            return _serviceHelper.Success("Game player deleted", await GetGame(gameId));
        }

        public async Task<ActionResultDto<GameDto>> DeleteGoal(Guid gameId, Guid goalId)
        {
            var game = await GetGame(gameId);
            if (game == null)
            {
                return _serviceHelper.NotFound<GameDto>("Game not found");
            }

            await _gameRepository.DeleteGoal(gameId, goalId);

            return _serviceHelper.Success("Goal deleted", await GetGame(gameId));
        }
    }
}
