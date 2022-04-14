using System;
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

        [HttpDelete("/api/Access")]
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

        [HttpPost("/api/Access/Recover/{adminPassCode}")]
        public async Task<ActionResultDto<AccessDto>> RecoverAccess(string adminPassCode, RecoverAccessDto recoverAccess)
        {
            try
            {
                return await _accessService.RecoverAccess(recoverAccess, adminPassCode);
            }
            catch (Exception exc)
            {
                return new ActionResultDto<AccessDto>
                {
                    Errors =
                    {
                        exc.ToString()
                    }
                };
            }
        }

        [HttpPost("/api/Access/Request")]
        public async Task<AccessRequestedDto> RequestAccess(AccessRequestDto requestDto)
        {
            return await _accessService.RequestAccess(requestDto);
        }

        [HttpDelete("/api/Access/Request")]
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
                return new ActionResultDto<AccessDto>
                {
                    Errors =
                    {
                        exc.ToString()
                    }
                };
            }
        }

        [HttpGet("/api/Access/Request")]
        public IAsyncEnumerable<AccessRequestDto> GetRequests()
        {
            return _accessService.GetAccessRequests();
        }

        [HttpGet("/api/Access/Revoke")]
        public async Task<ActionResultDto<AccessDto>> RevokeRequest(AccessResponseDto response)
        {
            try
            {
                return await _accessService.RevokeAccess(response);
            }
            catch (Exception exc)
            {
                return new ActionResultDto<AccessDto>
                {
                    Errors =
                    {
                        exc.ToString()
                    }
                };
            }
        }
    }
}
