using Azure.Data.Tables;

namespace HolcombeScores.Api.Repositories
{
    public interface ITableServiceClientFactory
    {
        TableServiceClient CreateTableServiceClient();
    }
}