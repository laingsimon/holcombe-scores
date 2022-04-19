using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface IMyAccessDtoAdapter
    {
        MyAccessDto Adapt(Access access, AccessRequest accessRequest);
    }
}
