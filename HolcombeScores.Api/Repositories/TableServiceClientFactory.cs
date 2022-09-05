using Azure.Data.Tables;

namespace HolcombeScores.Api.Repositories
{
    public class TableServiceClientFactory : ITableServiceClientFactory
    {
        private const string StorageKeyConfigName = "AZURE_TABLE_STORAGE_KEY";
        private const string AccountNameConfigName = "AZURE_TABLE_STORAGE_ACCOUNT_NAME";
        private const string StorageUriConfigName = "AZURE_TABLE_STORAGE_URI";

        private readonly Uri _storageUri;
        private readonly string _accountName;
        private readonly string _accountKey;

        public TableServiceClientFactory(IConfiguration configuration)
        {
            _accountName = configuration[AccountNameConfigName];
            _accountKey = configuration[StorageKeyConfigName];

            if (string.IsNullOrEmpty(_accountKey))
            {
                throw new ArgumentNullException(StorageKeyConfigName);
            }

            if (string.IsNullOrEmpty(_accountName))
            {
                throw new ArgumentNullException(AccountNameConfigName);
            }

            _storageUri = GetAzureStorageUri(configuration[StorageUriConfigName], _accountName);
        }

        public TableClient CreateTableClient(string tableName)
        {
            var client = new TableClient(
                _storageUri,
                tableName,
                new TableSharedKeyCredential(_accountName, _accountKey));

            client.CreateIfNotExists();
            return client;
        }

        private static Uri GetAzureStorageUri(string uriTemplate, string accountName)
        {
            if (string.IsNullOrEmpty(uriTemplate))
            {
                throw new ArgumentNullException(StorageUriConfigName);
            }

            return new Uri(uriTemplate.Replace("{AccountName}", accountName), UriKind.Absolute);
        }
    }
}
