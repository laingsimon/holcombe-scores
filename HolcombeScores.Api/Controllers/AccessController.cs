using HolcombeScores.Api.Models.Dtos;
using HolcombeScores.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HolcombeScores.Api.Controllers
{
    [ApiController]
    public class AccessController : Controller
    {
        private readonly IAccessService _accessService;
        private readonly IServiceHelper _serviceHelper;

        public AccessController(IAccessService accessService, IServiceHelper serviceHelper)
        {
            _accessService = accessService;
            _serviceHelper = serviceHelper;
        }

        [HttpGet("/api/Access")]
        public IAsyncEnumerable<AccessDto> ListAccess()
        {
            return _accessService.GetAllAccess();
        }

        [HttpDelete("/api/Access/{userId}")]
        public async Task<ActionResultDto<AccessDto>> RequestAccess(Guid userId)
        {
            return await _accessService.RemoveAccess(userId);
        }

        [HttpPatch("/api/Access")]
        public async Task<ActionResultDto<AccessDto>> UpdateAccess(AccessDto update)
        {
            return await _accessService.UpdateAccess(update);
        }

        [HttpGet("/api/Access/Recover")]
        public IAsyncEnumerable<RecoverAccessDto> RecoverAccess()
        {
            return _accessService.GetAccessForRecovery();
        }

        [HttpPost("/api/Access/Recover")]
        public async Task<ActionResultDto<AccessDto>> RecoverAccess(RecoverAccessDto recoverAccess)
        {
            try
            {
                return await _accessService.RecoverAccess(recoverAccess);
            }
            catch (Exception exc)
            {
                return _serviceHelper.Error<AccessDto>(exc.ToString());
            }
        }

        [HttpPost("/api/Access/Request")]
        public async Task<AccessRequestedDto> RequestAccess(AccessRequestDto requestDto)
        {
            return await _accessService.RequestAccess(requestDto);
        }

        [HttpDelete("/api/Access/Request/{userId}")]
        public async Task<ActionResultDto<AccessRequestDto>> RequestAccessRequest(Guid userId)
        {
            return await _accessService.RemoveAccessRequest(userId);
        }

        [HttpPost("/api/Access/Respond")]
        public async Task<ActionResultDto<AccessDto>> Respond(AccessResponseDto response)
        {
            try
            {
                return await _accessService.RespondToRequest(response);
            }
            catch (Exception exc)
            {
                return _serviceHelper.Error<AccessDto>(exc.ToString());
            }
        }

        [HttpGet("/api/Access/Request")]
        public IAsyncEnumerable<AccessRequestDto> GetRequests()
        {
            return _accessService.GetAccessRequests();
        }

        [HttpPost("/api/Access/Revoke")]
        public async Task<ActionResultDto<AccessDto>> RevokeRequest(AccessResponseDto response)
        {
            try
            {
                return await _accessService.RevokeAccess(response);
            }
            catch (Exception exc)
            {
                return _serviceHelper.Error<AccessDto>(exc.ToString());
            }
        }

        [HttpPost("/api/Access/Logout")]
        public async Task<ActionResultDto<string>> RecoverAccess()
        {
            try
            {
                return await _accessService.Logout();
            }
            catch (Exception exc)
            {
                return _serviceHelper.Error<string>(exc.ToString());
            }
        }
    }
}
