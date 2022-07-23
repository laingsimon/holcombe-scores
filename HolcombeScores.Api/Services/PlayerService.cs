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

            var existingPlayer = await _playerRepository.GetByNumber(player.TeamId, player.Number);

            if (existingPlayer == null)
            {
                await _playerRepository.AddPlayer(player);
                return _serviceHelper.Success("Player created", await GetPlayerDto(player.TeamId, player.Number));
            }

            await _playerRepository.UpdatePlayer(player.TeamId, player.Number, player.Name);
            return _serviceHelper.Success("Player updated", await GetPlayerDto(player.TeamId, player.Number));
        }

        public async Task<ActionResultDto<PlayerDto>> DeletePlayer(Guid teamId, int number)
        {
            if (!await _accessService.CanAccessTeam(teamId))
            {
                return _serviceHelper.NotPermitted<PlayerDto>("Not permitted to access team");
            }

            var existingPlayer = await _playerRepository.GetByNumber(teamId, number);

            if (existingPlayer == null)
            {
                return _serviceHelper.NotFound<PlayerDto>("Player not found");
            }

            await _playerRepository.DeletePlayer(existingPlayer.TeamId, existingPlayer.Number);

            return _serviceHelper.Success("Player deleted", _playerDtoAdapter.Adapt(existingPlayer));
        }

        public async Task<ActionResultDto<PlayerDto>> TransferPlayer(TransferPlayerDto transferDto)
        {
            if (!await _accessService.CanAccessTeam(transferDto.CurrentTeamId))
            {
                return _serviceHelper.NotPermitted<PlayerDto>("Not permitted to access team");
            }

            var newTeam = await _teamRepository.Get(transferDto.NewTeamId);
            if (newTeam == null)
            {
                return _serviceHelper.NotFound<PlayerDto>("New team not found");
            }

            var playerToTransfer = await _playerRepository.GetByNumber(transferDto.CurrentTeamId, transferDto.CurrentNumber);
            if (playerToTransfer == null)
            {
                return _serviceHelper.NotFound<PlayerDto>("Player not found");
            }

            var newPlayer = await _playerRepository.GetByNumber(transferDto.CurrentTeamId, transferDto.CurrentNumber);
            newPlayer.Number = transferDto.NewNumber ?? transferDto.CurrentNumber;
            newPlayer.TeamId = transferDto.NewTeamId;

            if (await _playerRepository.GetByNumber(transferDto.NewTeamId, newPlayer.Number) != null)
            {
                return _serviceHelper.NotSuccess<PlayerDto>($"Player already exists with this number in team {newTeam.Name}");
            }

            await _playerRepository.AddPlayer(newPlayer);
            await _playerRepository.DeletePlayer(transferDto.CurrentTeamId, transferDto.CurrentNumber);

            return _serviceHelper.Success($"Player transferred to {newTeam.Name}", _playerDtoAdapter.Adapt(newPlayer));
        }

        private async Task<PlayerDto> GetPlayerDto(Guid teamId, int number)
        {
            var existingPlayer = await _playerRepository.GetByNumber(teamId, number);
            return _playerDtoAdapter.Adapt(existingPlayer);
        }
    }
}
