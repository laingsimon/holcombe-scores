using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Repositories
{
    public class AccessRepository : IAccessRepository
    {
        private readonly TableClient _accessTableClient;
        private readonly TableClient _accessRequestTableClient;

        public AccessRepository(ITableServiceClientFactory tableServiceClientFactory)
        {
            _accessTableClient = tableServiceClientFactory.CreateTableClient("Access");
            _accessRequestTableClient = tableServiceClientFactory.CreateTableClient("AccessRequest");
        }

        public IAsyncEnumerable<Access> GetAllAccess()
        {
            return _accessTableClient.QueryAsync<Access>();
        }

        public IAsyncEnumerable<AccessRequest> GetAllAccessRequests()
        {
            return _accessRequestTableClient.QueryAsync<AccessRequest>();
        }

        public async Task<AccessRequest> GetAccessRequest(Guid userId)
        {
            return await _accessRequestTableClient.SingleOrDefaultAsync<AccessRequest>(gte => gte.UserId == userId);
        }

        public async Task<Access> GetAccess(Guid userId)
        {
            return await _accessTableClient.SingleOrDefaultAsync<Access>(gte => gte.UserId == userId);
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
            var accessRequest = await GetAccessRequest(userId);
            if (accessRequest != null)
            {
                await _accessRequestTableClient.DeleteEntityAsync(accessRequest.TeamId.ToString(), accessRequest.UserId.ToString());
            }
        }

        public async Task UpdateAccess(Access access)
        {
            access.Timestamp = DateTimeOffset.UtcNow;
            access.PartitionKey = access.TeamId.ToString();
            access.RowKey = access.UserId.ToString();
            access.ETag = ETag.All;

            await _accessTableClient.UpdateEntityAsync(access, ETag.All);
        }
    }
}