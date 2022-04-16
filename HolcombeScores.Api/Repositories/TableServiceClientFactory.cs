using System;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;

namespace HolcombeScores.Api.Repositories
{
    public class TableServiceClientFactory : ITableServiceClientFactory
    {
        private readonly Uri _storageUri;
        private readonly string _accountName;
        private readonly string _accountKey;

        public TableServiceClientFactory(IConfiguration configuration)
        {
            _accountName = configuration["AZURE_TABLE_STORAGE_ACCOUNT_NAME"];
            _accountKey = configuration["AZURE_TABLE_STORAGE_KEY"];

            if (string.IsNullOrEmpty(_accountKey))
            {
                throw new ArgumentNullException("AZURE_TABLE_STORAGE_KEY");
            }

            if (string.IsNullOrEmpty(_accountName))
            {
                throw new ArgumentNullException("AZURE_TABLE_STORAGE_ACCOUNT_NAME");
            }

            _storageUri = GetAzureStorageUri(configuration["AZURE_TABLE_STORAGE_URI"], _accountName);
        }

        public TableClient CreateTableClient(string tableName)
        {
            var client = new TableClient(
                _storageUri,
                tableName,
                new TableSharedKeyCredential(_accountName, _accountKey));

            client.CreateIfNotExists().Wait();
            return client;
        }

        private static Uri GetAzureStorageUri(string uriTemplate, string accountName)
        {
            if (string.IsNullOrEmpty(uriTemplate))
            {
                throw new ArgumentNullException("AZURE_TABLE_STORAGE_URI");
            }

            return new Uri(uriTemplate.Replace("{AccountName}", accountName), UriKind.Absolute);
        }
    }
}
