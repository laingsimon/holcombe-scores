using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Data.Tables;
using HolcombeScores.Models;

namespace HolcombeScores.Api.Repositories
{
    public class AccessRepository : IAccessRepository
    {
        private readonly IGenericEntityAdapter _genericEntityAdapter;
        private readonly TableClient _accessTableClient;
        private readonly TableClient _accessRequestTableClient;

        public AccessRepository(IGenericEntityAdapter genericEntityAdapter, ITableServiceClientFactory tableServiceClientFactory)
        {
            _genericEntityAdapter = genericEntityAdapter;
            _accessTableClient = tableServiceClientFactory.CreateTableClient("Access");
            _accessRequestTableClient = tableServiceClientFactory.CreateTableClient("AccessRequest");
        }

        public IAsyncEnumerable<Access> GetAllAccess()
        {
            var results = _accessTableClient.QueryAsync<GenericTableEntity<Access>>();
            return _genericEntityAdapter.AdaptAll(results);
        }

        public IAsyncEnumerable<AccessRequest> GetAllAccessRequests()
        {
            var results = _accessRequestTableClient.QueryAsync<GenericTableEntity<AccessRequest>>();
            return _genericEntityAdapter.AdaptAll(results);
        }

        public async Task<AccessRequest> GetAccessRequest(Guid userId)
        {
            return await _accessRequestTableClient.SingleOrDefaultAsync<AccessRequest>(gte => gte.Content.UserId == userId);
        }

        public async Task<Access> GetAccess(Guid userId)
        {
            return await _accessTableClient.SingleOrDefaultAsync<Access>(gte => gte.Content.UserId == userId);
        }

        public async Task AddAccessRequest(AccessRequest accessRequest)
        {
            var entity = _genericEntityAdapter.Adapt(accessRequest, accessRequest.TeamId, accessRequest.UserId);
            await _accessRequestTableClient.AddEntityAsync(entity);
        }

        public async Task AddAccess(Access access)
        {
            var entity = _genericEntityAdapter.Adapt(access, access.TeamId, access.UserId);
            await _accessTableClient.AddEntityAsync(entity);
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
            var genericEntity = _genericEntityAdapter.Adapt(access, access.TeamId, access.UserId);
            await _accessTableClient.UpdateEntityAsync(genericEntity, genericEntity.ETag);
        }
    }
}