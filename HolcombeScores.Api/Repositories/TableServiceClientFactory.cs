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

        public TableClient CreateTableClient(string tableName)
        {
            var client = new TableClient(
                _context.StorageUri,
                tableName,
                _context.GetTableSharedKeyCredential());

            client.CreateIfNotExists();
            return client;
        }
    }
}
