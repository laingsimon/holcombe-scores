using HolcombeScores.Api.Models;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services.Adapters
{
    public class AccessDtoAdapter : IAccessDtoAdapter
    {
        public Access Adapt(AccessDto access)
        {
            return new Access
            {
                Admin = access.Admin,
                Granted = access.Granted,
                Name = access.Name,
                Revoked = access.Revoked,
                TeamId = access.TeamId,
                UserId = access.UserId,
                RevokedReason = access.RevokedReason,
            };
        }

        public AccessDto Adapt(Access access)
        {
            return new AccessDto
            {
                Admin = access.Admin,
                Granted = access.Granted,
                Name = access.Name,
                Revoked = access.Revoked,
                TeamId = access.TeamId,
                UserId = access.UserId,
                RevokedReason = access.RevokedReason,
            };
        }
    }
}