using HolcombeScores.Api.Models.AzureTables;

namespace HolcombeScores.Api.Repositories
{
    public interface IAccessRepository
    {
        IAsyncEnumerable<Access> GetAllAccess(Guid? teamId = null);
        IAsyncEnumerable<AccessRequest> GetAllAccessRequests(Guid? teamId = null);
        Task<AccessRequest> GetAccessRequest(string token, Guid userId);
        Task<AccessRequest> GetAccessRequest(Guid userId);
        Task<Access> GetAccess(string token, Guid userId);
        Task<Access> GetAccess(Guid userId);
        Task AddAccessRequest(AccessRequest accessRequest);
        Task AddAccess(Access access);
        Task RemoveAccessRequest(Guid userId);
        Task UpdateAccess(Access access);
        Task RemoveAccess(Guid userId);
        Task UpdateAccessToken(string currentToken, string newToken);
        Task UpdateAccessRequestToken(string currentToken, string newToken);
        Task UpdateAccessRequest(AccessRequest accessRequest);
    }
}
