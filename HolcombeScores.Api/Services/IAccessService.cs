using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HolcombeScores.Api.Models;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services
{
    public interface IAccessService
    {
        Task<MyAccessDto> GetMyAccess();
        Task<Access> GetAccess();
        Task<bool> IsAdmin();
        Task<AccessRequestedDto> RequestAccess(AccessRequestDto accessRequestDto);
        Task<ActionResultDto<AccessDto>> RespondToRequest(AccessResponseDto response);
        IAsyncEnumerable<AccessRequestDto> GetAccessRequests();
        Task<bool> CanAccessTeam(Guid teamId);
        Task<ActionResultDto<AccessDto>> RevokeAccess(AccessResponseDto accessResponseDto);
        IAsyncEnumerable<AccessDto> GetAllAccess();
        IAsyncEnumerable<RecoverAccessDto> GetAccessForRecovery();
        Task<ActionResultDto<AccessDto>> RecoverAccess(RecoverAccessDto recoverAccessDto, string adminPassCode);
        Task<ActionResultDto<AccessDto>> RemoveAccess(Guid userId);
        Task<ActionResultDto<AccessRequestDto>> RemoveAccessRequest(Guid userId);
    }
}
