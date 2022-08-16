using HolcombeScores.Api.Models.AzureTables;
using HolcombeScores.Api.Models.Dtos;

namespace HolcombeScores.Api.Services.Adapters
{
    public interface IAccessDtoAdapter
    {
        AccessDto Adapt(Access access, Access impersonatedBy = null);
    }
}
