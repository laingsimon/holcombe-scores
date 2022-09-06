using Azure.Data.Tables;

namespace HolcombeScores.Api.Repositories
{
    public class TableServiceClientFactory : ITableServiceClientFactory
    {
        private readonly IAzureRepositoryContext _context;

        public TableServiceClientFactory(IAzureRepositoryContext context)
        {
            _context = context;
        }

        public TableServiceClient CreateTableServiceClient()
        {
            return new TableServiceClient(_context.StorageUri, _context.GetTableSharedKeyCredential());
        }
    }
}
