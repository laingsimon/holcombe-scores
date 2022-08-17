using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface IMyAccessDtoAdapter
    {
        Task<MyAccessDto> Adapt(Access access, IAsyncEnumerable<AccessRequest> accessRequests,
            Access impersonatedBy = null);
    }
}
