using HolcombeScores.Api.Models;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface IRecoverAccessDtoAdapter
    {
        RecoverAccessDto Adapt(Access access);
        RecoverAccessDto Adapt(AccessRequest accessRequest);
    }
}
