using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface IAccessRequestDtoAdapter
    {
        AccessRequest Adapt(AccessRequestDto accessRequest);
        AccessRequestDto Adapt(AccessRequest accessRequest);
    }
}