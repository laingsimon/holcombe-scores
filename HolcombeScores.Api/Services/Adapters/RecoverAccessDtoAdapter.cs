using HolcombeScores.Api.Models;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services.Adapters
{
    public class RecoverAccessDtoAdapter : IRecoverAccessDtoAdapter
    {
        public RecoverAccessDto Adapt(Access access)
        {
            return new RecoverAccessDto
            {
                Name = access.Name,
                UserId = access.UserId.ToString().Substring(0, 8),
            };
        }
    }
}
