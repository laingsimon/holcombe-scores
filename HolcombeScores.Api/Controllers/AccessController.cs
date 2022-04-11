using System.Collections.Generic;
using System.Threading.Tasks;
using HolcombeScores.Api.Models;
using HolcombeScores.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HolcombeScores.Api.Controllers
{
    [ApiController]
    public class AccessController : Controller
    {
        private readonly IAccessService _accessService;

        public AccessController(IAccessService accessService)
        {
            _accessService = accessService;
        }

        [HttpGet("/api/Access")]
        public IAsyncEnumerable<AccessDto> ListAccess()
        {
            return _accessService.GetAllAccess();
        }

        [HttpGet("/api/Access/Recover")]
        public IAsyncEnumerable<RecoverAccessDto> RecoverAccess()
        {
            return _accessService.GetAccessForRecovery();
        }

        [HttpPost("/api/Access/Recover/{adminPassCode}")]
        public async Task<ActionResultDto<AccessDto>> RecoverAccess(string adminPassCode, RecoverAccessDto recoverAccess)
        {
            return _accessService.RecoverAccess(recoverAccess, adminPassCode);
        }

        [HttpPost("/api/Access/Request")]
        public async Task<AccessRequestedDto> RequestAccess(AccessRequestDto requestDto)
        {
            return await _accessService.RequestAccess(requestDto);
        }

        [HttpPost("/api/Access/Respond")]
        public async Task<ActionResultDto<AccessDto>> Respond(AccessResponseDto response)
        {
            return await _accessService.RespondToRequest(response);
        }

        [HttpGet("/api/Access/Request")]
        public IAsyncEnumerable<AccessRequestDto> GetRequests()
        {
            return _accessService.GetAccessRequests();
        }

        [HttpGet("/api/Access/Revoke")]
        public async Task<ActionResultDto<AccessDto>> RevokeRequest(AccessResponseDto response)
        {
            return await _accessService.RevokeAccess(response);
        }
    }
}
