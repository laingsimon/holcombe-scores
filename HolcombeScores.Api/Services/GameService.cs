using HolcombeScores.Api.Models.Dtos;
using HolcombeScores.Api.Repositories;
using HolcombeScores.Api.Services.Adapters;

namespace HolcombeScores.Api.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly IGameDtoAdapter _gameDtoAdapter;
        private readonly IAccessService _accessService;
        private readonly IGameDetailsDtoAdapter _gameDetailsDtoAdapter;
        private readonly IGoalDtoAdapter _goalDtoAdapter;
        private readonly ITeamRepository _teamRepository;
        private readonly IServiceHelper _serviceHelper;

        public GameService(
            IGameRepository gameRepository,
            IGameDtoAdapter gameDtoAdapter,
            IAccessService accessService,
            IGameDetailsDtoAdapter gameDetailsDtoAdapter,
            IGoalDtoAdapter goalDtoAdapter,
            ITeamRepository teamRepository,
            IServiceHelper serviceHelper)
        {
            _gameRepository = gameRepository;
            _gameDtoAdapter = gameDtoAdapter;
            _accessService = accessService;
            _gameDetailsDtoAdapter = gameDetailsDtoAdapter;
            _goalDtoAdapter = goalDtoAdapter;
            _teamRepository = teamRepository;
            _serviceHelper = serviceHelper;
        }

        public async IAsyncEnumerable<GameDto> GetAllGames(Guid? teamId)
        {
            var access = await _accessService.GetAccess();
            if (access == null || access.Revoked != null)
            {
                yield break;
            }

            if (teamId == null && !access.Admin)
            {
                teamId = access.TeamId;
            }

            await foreach (var game in _gameRepository.GetAll(teamId))
            {
                var gamePlayers = await _gameRepository.GetPlayers(game.Id);
                var goals = await _gameRepository.GetGoals(game.Id);
                yield return await _gameDtoAdapter.Adapt(game, gamePlayers, goals);
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
            return await _gameDtoAdapter.Adapt(game, gamePlayers, goals);
        }

        public async Task<ActionResultDto<GameDto>> CreateGame(GameDetailsDto gameDetailsDto)
        {
            if (await _teamRepository.Get(gameDetailsDto.TeamId) == null)
            {
                return _serviceHelper.NotFound<GameDto>("Team not found");
            }

            if (!await _accessService.CanAccessTeam(gameDetailsDto.TeamId))
            {
                return _serviceHelper.NotPermitted<GameDto>("Not permitted to interact with this team");
            }

            if (!await _accessService.IsManagerOrAdmin())
            {
                return _serviceHelper.NotPermitted<GameDto>("Only managers and admins can create games");
            }

            try
            {
                // TODO: Add Validation

                var game = _gameDetailsDtoAdapter.AdaptToGame(gameDetailsDto);
                game.Id = Guid.NewGuid();
                var missingPlayers = new List<Guid>();
                var squad = _gameDetailsDtoAdapter.AdaptSquad(gameDetailsDto, game.Id, missingPlayers);
                await _gameRepository.Add(game);

                await foreach (var gamePlayer in squad)
                {
                    await _gameRepository.AddGamePlayer(gamePlayer);
                }

                if (missingPlayers.Count == gameDetailsDto.PlayerIds.Length)
                {
                    await _gameRepository.DeleteGame(game.Id);
                }

                var result = missingPlayers.Count == gameDetailsDto.PlayerIds.Length
                    ? _serviceHelper.NotSuccess<GameDto>("Game not created, no players found")
                    : _serviceHelper.Success("Game created", await GetGame(game.Id));

                foreach (var player in missingPlayers)
                {
                    result.Warnings.Add($"Player not found: `{player}`");
                }

                return result;
            }
            catch (Exception exc)
            {
                return _serviceHelper.Error<GameDto>(exc.ToString());
            }
        }

        public async Task<ActionResultDto<GameDto>> UpdateGame(ExistingGameDetailsDto gameDetailsDto)
        {
            if (await _teamRepository.Get(gameDetailsDto.TeamId) == null)
            {
                return _serviceHelper.NotFound<GameDto>("Team not found");
            }

            if (!await _accessService.IsManagerOrAdmin())
            {
                return _serviceHelper.NotPermitted<GameDto>("Only managers and admins can update games");
            }

            try
            {
                // TODO: Add Validation

                var update = _gameDetailsDtoAdapter.AdaptToGame(gameDetailsDto);

                var game = await _gameRepository.Get(update.Id);
                if (game == null)
                {
                    return _serviceHelper.NotFound<GameDto>("Game not found");
                }

                if (!await _accessService.CanAccessTeam(game.TeamId))
                {
                    return _serviceHelper.NotPermitted<GameDto>("Not permitted to interact with this team");
                }

                var updates = new List<string>();
                if (game.PlayingAtHome != update.PlayingAtHome)
                {
                    updates.Add("PlayingAtHome updated");
                }

                game.PlayingAtHome = update.PlayingAtHome;

                if (update.Date != default && game.Date != update.Date)
                {
                    game.Date = update.Date;
                    updates.Add("Date updated");
                }

                if (!string.IsNullOrEmpty(update.Opponent) && game.Opponent != update.Opponent)
                {
                    game.Opponent = update.Opponent;
                    updates.Add("Opponent updated");
                }

                if (update.TeamId != default && game.TeamId != update.TeamId)
                {
                    if (!await _accessService.IsAdmin())
                    {
                        return _serviceHelper.NotAnAdmin<GameDto>();
                    }

                    game.TeamId = update.TeamId;
                    updates.Add("Team updated");
                }

                if (updates.Any())
                {
                    await _gameRepository.Update(game);
                }

                var missingPlayers = new List<Guid>();
                if (gameDetailsDto.PlayerIds != null && gameDetailsDto.PlayerIds.Any())
                {
                    var squad = (await _gameDetailsDtoAdapter.AdaptSquad(gameDetailsDto, game.Id, missingPlayers).ToEnumerable()).ToArray();

                    // remove existing players
                    foreach (var player in await _gameRepository.GetPlayers(game.Id))
                    {
                        if (squad.Any(p => p.Name == player.Name))
                        {
                            // player in new squad, don't remove
                            squad = squad.Where(p => p.Name != player.Name).ToArray();
                            continue;
                        }
                        updates.Add($"`{player.Name}` removed from game");
                        await _gameRepository.DeleteGamePlayer(game.Id, player.PlayerId);
                    }

                    foreach (var gamePlayer in squad)
                    {
                        updates.Add($"`{gamePlayer.Name}` added to game");
                        await _gameRepository.AddGamePlayer(gamePlayer);
                    }
                }

                var statement = updates.Any()
                    ? "Game updated"
                    : "No changes to game";

                var result = _serviceHelper.Success(statement, await GetGame(game.Id));

                foreach (var player in missingPlayers)
                {
                    result.Warnings.Add($"Player not found: `{player}`");
                }

                foreach (var message in updates)
                {
                    result.Messages.Add(message);
                }

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

            var game = await _gameRepository.Get(goalDto.GameId);
            if (game == null)
            {
                return _serviceHelper.NotFound<GameDto>("Game not found");
            }

            if (!await _accessService.CanAccessTeam(game.TeamId))
            {
                return _serviceHelper.NotPermitted<GameDto>("Not permitted to interact with this team");
            }

            var goal = _goalDtoAdapter.Adapt(goalDto, game);
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

            if (!await _accessService.IsManagerOrAdmin())
            {
                return _serviceHelper.NotPermitted<GameDto>("Only managers and admins can delete games");
            }

            await _gameRepository.DeleteGame(id);

            return _serviceHelper.Success("Game deleted", game);
        }

        public async Task<ActionResultDto<GameDto>> DeleteGamePlayer(Guid gameId, Guid playerId)
        {
            var game = await GetGame(gameId);
            if (game == null)
            {
                return _serviceHelper.NotFound<GameDto>("Game not found");
            }

            if (!await _accessService.IsManagerOrAdmin())
            {
                return _serviceHelper.NotPermitted<GameDto>("Only managers and admins can update games");
            }

            await _gameRepository.DeleteGamePlayer(gameId, playerId);

            return _serviceHelper.Success("Game player deleted", await GetGame(gameId));
        }

        public async Task<ActionResultDto<GameDto>> DeleteGoal(Guid gameId, Guid goalId)
        {
            var game = await GetGame(gameId);
            if (game == null)
            {
                return _serviceHelper.NotFound<GameDto>("Game not found");
            }

            if (!await _accessService.IsManagerOrAdmin())
            {
                return _serviceHelper.NotPermitted<GameDto>("Only managers and admins can remove goals");
            }

            await _gameRepository.DeleteGoal(gameId, goalId);

            return _serviceHelper.Success("Goal deleted", await GetGame(gameId));
        }
    }
}
