using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services.Adapters
{
    public class AccessDtoAdapter : IAccessDtoAdapter
    {
        public AccessDto Adapt(Access access, Access impersonatedBy = null)
        {
            if (access == null)
            {
                return null;
            }

            return new AccessDto
            {
                Admin = access.Admin,
                Granted = access.Granted,
                Name = access.Name,
                Revoked = access.Revoked,
                Teams = access.Teams,
                UserId = access.UserId,
                RevokedReason = access.RevokedReason,
                Manager = access.Manager,
                ImpersonatedBy = impersonatedBy == null
                    ? null
                    : Adapt(impersonatedBy),
            };
        }
    }
}
