using Azure.Data.Tables;

namespace HolcombeScores.Api.Repositories;

public class AzureRepositoryContext : IAzureRepositoryContext
{
    private const string StorageKeyConfigName = "AZURE_TABLE_STORAGE_KEY";
    private const string AccountNameConfigName = "AZURE_TABLE_STORAGE_ACCOUNT_NAME";
    private const string StorageUriConfigName = "AZURE_TABLE_STORAGE_URI";

    private readonly string _accountName;
    private readonly string _accountKey;
    public Uri StorageUri { get; }

    public AzureRepositoryContext(IConfiguration configuration)
    {
        _accountKey = configuration[StorageKeyConfigName];
        if (string.IsNullOrEmpty(_accountKey))
        {
            throw new ArgumentNullException(StorageKeyConfigName);
        }

        _accountName = configuration[AccountNameConfigName];
        if (string.IsNullOrEmpty(_accountName))
        {
            throw new ArgumentNullException(AccountNameConfigName);
        }

        var uriTemplate = configuration[StorageUriConfigName];
        if (string.IsNullOrEmpty(uriTemplate))
        {
            throw new ArgumentNullException(StorageUriConfigName);
        }

        StorageUri = GetAzureStorageUri(uriTemplate, _accountName);
    }

    public TableSharedKeyCredential GetTableSharedKeyCredential()
    {
        return new TableSharedKeyCredential(_accountName, _accountKey);
    }

    private static Uri GetAzureStorageUri(string uriTemplate, string accountName)
    {
        return new Uri(uriTemplate.Replace("{AccountName}", accountName), UriKind.Absolute);
    }
}