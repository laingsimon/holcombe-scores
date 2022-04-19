using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface IRecoverAccessDtoAdapter
    {
        RecoverAccessDto Adapt(Access access);
        RecoverAccessDto Adapt(AccessRequest accessRequest);
    }
}
