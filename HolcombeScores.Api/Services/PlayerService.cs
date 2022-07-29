using HolcombeScores.Api.Models.Dtos;
using HolcombeScores.Api.Repositories;
using HolcombeScores.Api.Services.Adapters;

namespace HolcombeScores.Api.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IPlayerDtoAdapter _playerDtoAdapter;
        private readonly IAccessService _accessService;
        private readonly ITeamRepository _teamRepository;
        private readonly IServiceHelper _serviceHelper;

        public PlayerService(
            IPlayerRepository playerRepository,
            IPlayerDtoAdapter playerDtoAdapter,
            IAccessService accessService,
            ITeamRepository teamRepository,
            IServiceHelper serviceHelper)
        {
            _playerRepository = playerRepository;
            _playerDtoAdapter = playerDtoAdapter;
            _accessService = accessService;
            _teamRepository = teamRepository;
            _serviceHelper = serviceHelper;
        }

        public async IAsyncEnumerable<PlayerDto> GetAllPlayers()
        {
            var access = await _accessService.GetAccess();
            if (access == null || access.Revoked != null)
            {
                yield break;
            }

            await foreach (var player in _playerRepository.GetAll(access.Admin ? null : access.TeamId))
            {
                yield return _playerDtoAdapter.Adapt(player);
            }
        }

        public async IAsyncEnumerable<PlayerDto> GetPlayers(Guid teamId)
        {
            var access = await _accessService.GetAccess();
            if (access == null || access.Revoked != null)
            {
                yield break;
            }

            if (!access.Admin && access.TeamId != teamId)
            {
                // asking for players from another team, and not an admin
                yield break;
            }

            await foreach (var player in _playerRepository.GetAll(teamId))
            {
                yield return _playerDtoAdapter.Adapt(player);
            }
        }

        public async Task<ActionResultDto<PlayerDto>> CreateOrUpdatePlayer(PlayerDto playerDto)
        {
            var player = _playerDtoAdapter.Adapt(playerDto);

            if (!await _accessService.CanAccessTeam(playerDto.TeamId))
            {
                return _serviceHelper.NotPermitted<PlayerDto>("Not permitted to access team");
            }

            if (!await _accessService.IsManagerOrAdmin())
            {
                return _serviceHelper.NotPermitted<PlayerDto>("Only managers and admins can create or update players");
            }

            var existingPlayer = await _playerRepository.Get(player.Id);

            if (existingPlayer == null)
            {
                var newPlayer = await _playerRepository.AddPlayer(player);
                return _serviceHelper.Success("Player created", _playerDtoAdapter.Adapt(newPlayer));
            }

            if (!await _accessService.CanAccessTeam(existingPlayer.TeamId))
            {
                return _serviceHelper.NotPermitted<PlayerDto>("Not permitted to access team");
            }

            await _playerRepository.UpdatePlayer(player.Id, player.TeamId, player.Number, player.Name);
            return _serviceHelper.Success("Player updated", await GetPlayerDto(player.Id));
        }

        public async Task<ActionResultDto<PlayerDto>> DeletePlayer(Guid playerId)
        {
            if (!await _accessService.IsManagerOrAdmin())
            {
                return _serviceHelper.NotPermitted<PlayerDto>("Only managers and admins can remove players");
            }

            var existingPlayer = await _playerRepository.Get(playerId);

            if (existingPlayer == null)
            {
                return _serviceHelper.NotFound<PlayerDto>("Player not found");
            }

            if (!await _accessService.CanAccessTeam(existingPlayer.TeamId))
            {
                return _serviceHelper.NotPermitted<PlayerDto>("Not permitted to access team");
            }

            await _playerRepository.DeletePlayer(existingPlayer.Id);

            return _serviceHelper.Success("Player deleted", _playerDtoAdapter.Adapt(existingPlayer));
        }

        public async Task<ActionResultDto<PlayerDto>> TransferPlayer(TransferPlayerDto transferDto)
        {
            if (!await _accessService.IsAdmin())
            {
                return _serviceHelper.NotPermitted<PlayerDto>("Only admins can transfer players");
            }

            var newTeam = await _teamRepository.Get(transferDto.NewTeamId);
            if (newTeam == null)
            {
                return _serviceHelper.NotFound<PlayerDto>("New team not found");
            }

            var playerToTransfer = await _playerRepository.Get(transferDto.PlayerId);
            if (playerToTransfer == null)
            {
                return _serviceHelper.NotFound<PlayerDto>("Player not found");
            }

            var newPlayer = await _playerRepository.Get(transferDto.PlayerId);
            newPlayer.Number = transferDto.NewNumber ?? playerToTransfer.Number;
            newPlayer.TeamId = transferDto.NewTeamId;

            if (newPlayer.Number != null && await _playerRepository.GetByNumber(transferDto.NewTeamId, newPlayer.Number.Value) != null)
            {
                return _serviceHelper.NotSuccess<PlayerDto>($"Player already exists with this number in team {newTeam.Name}");
            }

            await _playerRepository.AddPlayer(newPlayer);
            await _playerRepository.DeletePlayer(playerToTransfer.Id);

            return _serviceHelper.Success($"Player transferred to {newTeam.Name}", _playerDtoAdapter.Adapt(newPlayer));
        }

        private async Task<PlayerDto> GetPlayerDto(Guid id)
        {
            var existingPlayer = await _playerRepository.Get(id);
            return _playerDtoAdapter.Adapt(existingPlayer);
        }
    }
}
