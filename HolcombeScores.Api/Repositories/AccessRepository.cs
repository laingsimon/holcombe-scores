using Azure;
using Azure.Data.Tables;
using HolcombeScores.Api.Models.AzureTables;

namespace HolcombeScores.Api.Repositories
{
    public class AccessRepository : IAccessRepository
    {
        private readonly TypedTableClient<Access> _accessTableClient;
        private readonly TypedTableClient<AccessRequest> _accessRequestTableClient;

        public AccessRepository(ITableServiceClientFactory tableServiceClientFactory)
        {
            _accessTableClient = new TypedTableClient<Access>(tableServiceClientFactory.CreateTableClient("Access"));
            _accessRequestTableClient = new TypedTableClient<AccessRequest>(tableServiceClientFactory.CreateTableClient("AccessRequest"));
        }

        public IAsyncEnumerable<Access> GetAllAccess(Guid[] teamIds = null)
        {
            return teamIds != null
                ? UpgradeTeams(_accessTableClient.QueryAsync(a => a.Teams.Any(teamIds.Contains)))
                : UpgradeTeams(_accessTableClient.QueryAsync());
        }

        // TODO: Remove this
        private async IAsyncEnumerable<Access> UpgradeTeams(IAsyncEnumerable<Access> accessList)
        {
            await foreach (var access in accessList)
            {
                if (access.Teams == null)
                {
                    access.Teams = new[] { access.TeamId };
                    await _accessTableClient.UpdateEntityAsync(access, ETag.All);
                }
                yield return access;
            }
        }

        public IAsyncEnumerable<AccessRequest> GetAllAccessRequests(Guid[] teamIds = null)
        {
            return teamIds != null
                ? _accessRequestTableClient.QueryAsync(ar => teamIds.Contains(ar.TeamId))
                : _accessRequestTableClient.QueryAsync();
        }

        public async Task<AccessRequest> GetAccessRequest(string token, Guid userId)
        {
            return await _accessRequestTableClient.SingleOrDefaultAsync(a => a.Token == token && a.UserId == userId);
        }

        public async Task<AccessRequest> GetAccessRequest(Guid userId)
        {
            return await _accessRequestTableClient.SingleOrDefaultAsync(a => a.UserId == userId);
        }

        public async Task<Access> GetAccess(string token, Guid userId)
        {
            return await _accessTableClient.SingleOrDefaultAsync(a => a.Token == token && a.UserId == userId);
        }

        public async Task<Access> GetAccess(Guid userId)
        {
            return await _accessTableClient.SingleOrDefaultAsync(a => a.UserId == userId);
        }

        public async Task AddAccessRequest(AccessRequest accessRequest)
        {
            accessRequest.Timestamp = DateTimeOffset.UtcNow;
            accessRequest.PartitionKey = accessRequest.TeamId.ToString();
            accessRequest.RowKey = accessRequest.UserId.ToString();
            accessRequest.ETag = ETag.All;
            accessRequest.Requested = DateTime.UtcNow;

            await _accessRequestTableClient.AddEntityAsync(accessRequest);
        }

        public async Task AddAccess(Access access)
        {
            access.Timestamp = DateTimeOffset.UtcNow;
            access.PartitionKey = access.Teams.First().ToString(); // NOTE: there must be a team.
            access.RowKey = access.UserId.ToString();
            access.ETag = ETag.All;

            await _accessTableClient.AddEntityAsync(access);
        }

        public async Task RemoveAccessRequest(Guid userId)
        {
            var accessRequest = await _accessRequestTableClient.SingleOrDefaultAsync(a => a.UserId == userId);
            if (accessRequest != null)
            {
                await _accessRequestTableClient.DeleteEntityAsync(accessRequest.PartitionKey, accessRequest.RowKey);
            }
        }

        public async Task UpdateAccess(Access access)
        {
            await _accessTableClient.UpdateEntityAsync(access, ETag.All);
        }

        public async Task RemoveAccess(Guid userId)
        {
            await foreach (var access in _accessTableClient.QueryAsync(a => a.UserId == userId))
            {
                await _accessTableClient.DeleteEntityAsync(access.PartitionKey, access.RowKey, ETag.All);
            }
        }

        public async Task UpdateAccessToken(string currentToken, string newToken)
        {
            var access = await _accessTableClient.SingleOrDefaultAsync(a => a.Token == currentToken);
            access.Token = newToken;
            access.Timestamp = DateTimeOffset.UtcNow;

            await _accessTableClient.UpdateEntityAsync(access, ETag.All);
        }

        public async Task UpdateAccessRequestToken(string currentToken, string newToken)
        {
            var accessRequest = await _accessRequestTableClient.SingleOrDefaultAsync(a => a.Token == currentToken);
            accessRequest.Token = newToken;
            accessRequest.Timestamp = DateTimeOffset.UtcNow;

            await _accessRequestTableClient.UpdateEntityAsync(accessRequest, ETag.All);
        }

        public async Task UpdateAccessRequest(AccessRequest accessRequest)
        {
            await _accessRequestTableClient.UpdateEntityAsync(accessRequest, ETag.All, TableUpdateMode.Replace);
        }
    }
}
