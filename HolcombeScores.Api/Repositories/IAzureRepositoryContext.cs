using Azure.Data.Tables;

namespace HolcombeScores.Api.Repositories;

public interface IAzureRepositoryContext
{
    Uri StorageUri { get; }
    TableSharedKeyCredential GetTableSharedKeyCredential();
}