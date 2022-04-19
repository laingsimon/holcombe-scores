using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
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

        public IAsyncEnumerable<Access> GetAllAccess()
        {
            return _accessTableClient.QueryAsync();
        }

        public IAsyncEnumerable<AccessRequest> GetAllAccessRequests()
        {
            return _accessRequestTableClient.QueryAsync();
        }

        public async Task<AccessRequest> GetAccessRequest(string token)
        {
            return await _accessRequestTableClient.SingleOrDefaultAsync(a => a.Token == token);
        }

        public async Task<AccessRequest> GetAccessRequest(Guid userId)
        {
            return await _accessRequestTableClient.SingleOrDefaultAsync(a => a.UserId == userId);
        }

        public async Task<Access> GetAccess(string token)
        {
            return await _accessTableClient.SingleOrDefaultAsync(a => a.Token == token);
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

            await _accessRequestTableClient.AddEntityAsync(accessRequest);
        }

        public async Task AddAccess(Access access)
        {
            access.Timestamp = DateTimeOffset.UtcNow;
            access.PartitionKey = access.TeamId.ToString();
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
    }
}
