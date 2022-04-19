using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface IAccessRequestedDtoAdapter
    {
        AccessRequestedDto Adapt(AccessRequest accessRequest);
    }
}