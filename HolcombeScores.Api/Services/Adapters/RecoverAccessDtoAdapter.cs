using HolcombeScores.Api.Models;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services.Adapters
{
    public class RecoverAccessDtoAdapter : IRecoverAccessDtoAdapter
    {
        public RecoverAccessDto Adapt(Access access)
        {
            if (access == null)
            {
                return null;
            }

            return new RecoverAccessDto
            {
                Name = access.Name,
                RecoveryId = access.UserId.ToString().Substring(0, 8),
            };
        }
    }
}
