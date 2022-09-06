using Azure.Data.Tables;
using HolcombeScores.Api.Models.Dtos;
using HolcombeScores.Api.Repositories;

namespace HolcombeScores.Api.Services;

public class TestingService : ITestingService
{
    private const string TestTableDelimiter = "TEST";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceHelper _serviceHelper;
    private readonly ITableServiceClientFactory _tableServiceClientFactory;
    private readonly ITableClientFactory _tableClientFactory;
    private const string ContextIdCookieName = "HS_TestingContext";

    public TestingService(
        IHttpContextAccessor httpContextAccessor,
        IServiceHelper serviceHelper,
        ITableServiceClientFactory tableServiceClientFactory,
        ITableClientFactory tableClientFactory)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceHelper = serviceHelper;
        _tableServiceClientFactory = tableServiceClientFactory;
        _tableClientFactory = tableClientFactory;
    }

    public async Task<ActionResultDto<TestingContextCreatedDto>> CreateTestingContext(CreateTestingContextRequestDto request)
    {
        var deletionResult = await EndTestingContext();
        var newContextId = Guid.NewGuid();
        var result = new ActionResultDto<TestingContextCreatedDto>
        {
            Outcome = new TestingContextCreatedDto
            {
                ContextId = newContextId,
                DeletionResult = deletionResult,
            },
            Success = true
        };

        try
        {
            await ProvisionTables(request, newContextId, result);

            SetContextCookie(newContextId);
        }
        catch (Exception exc)
        {
            result.Errors.Add(exc.Message);
            result.Success = false;
        }

        return result;
    }

    public async Task<ActionResultDto<DeleteTestingContextDto>> EndTestingContext()
    {
        var contextId = GetContextCookie();
        if (contextId == null)
        {
            return _serviceHelper.NotFound<DeleteTestingContextDto>("No testing context in session");
        }

        var result = new DeleteTestingContextDto
        {
            ContextId = contextId.Value,
            RemovedTables = new List<string>(),
        };

        try
        {
            await DeleteTables(contextId.Value, result);

            DeleteContextCookie();
        }
        catch (Exception exc)
        {
            return _serviceHelper.Error<DeleteTestingContextDto>(exc.Message);
        }

        return _serviceHelper.Success("Testing context removed", result);
    }

    public Guid? GetContextCookie()
    {
        var requestCookie = _httpContextAccessor.HttpContext?.Request.Cookies[ContextIdCookieName];
        if (!string.IsNullOrEmpty(requestCookie) && Guid.TryParse(requestCookie, out var contextId))
        {
            return contextId;
        }

        return null;
    }

    private async Task ProvisionTables(CreateTestingContextRequestDto request, Guid newContextId,
        ActionResultDto<TestingContextCreatedDto> result)
    {
        if (request.Tables == null)
        {
            // copy ALL tables according to request.CopyExistingTables
            if (request.CopyExistingTables == true)
            {
                await CopyAllExistingTables(newContextId, result);
            }

            return;
        }

        // copy the tables given in <Tables> using their table.CopyExistingTable and table.Rows parameters
        foreach (var (sourceTableName, tableProvisionDetails) in request.Tables)
        {
            var tableProvisioningService = CreateTableProvisioningService(sourceTableName);
            if (tableProvisioningService == null)
            {
                result.Errors.Add($"Unable to provision data for {sourceTableName}, data type cannot be resolved");
                continue;
            }

            await tableProvisioningService.ProvisionTable(
                sourceTableName,
                newContextId,
                tableProvisionDetails.CopyExistingTable ?? request.CopyExistingTables ?? false, // default to NOT copying tables unless requested
                tableProvisionDetails.Rows,
                result);
        }
    }

    private async Task CopyAllExistingTables(Guid newContextId, ActionResultDto<TestingContextCreatedDto> result)
    {
        // find all the existing tables
        await foreach (var tableName in GetExistingTableNames())
        {
            var tableProvisioningService = CreateTableProvisioningService(tableName);
            if (tableProvisioningService == null)
            {
                result.Errors.Add($"Unable to provision data for {tableName}, data type cannot be resolved");
                continue;
            }

            await tableProvisioningService.ProvisionTable(tableName, newContextId, true, null, result);
        }
    }

    private ITableProvisioningService CreateTableProvisioningService(String tableName)
    {
        var dataType = Type.GetType($"HolcombeScores.Api.Models.AzureTables.{tableName}");
        if (dataType == null)
        {
            return null;
        }

        var genericType = typeof(TableProvisioningService<>).MakeGenericType(dataType);
        return (ITableProvisioningService)Activator.CreateInstance(genericType, _tableClientFactory);
    }

    private async IAsyncEnumerable<string> GetAllTableNames()
    {
        var tableServiceClient = _tableServiceClientFactory.CreateTableServiceClient();
        await foreach (var table in tableServiceClient.QueryAsync())
        {
            yield return table.Name;
        }
    }

    private IAsyncEnumerable<string> GetExistingTableNames()
    {
        return GetAllTableNames().WhereAsync(name => !name.Contains(TestTableDelimiter));
    }

    private async Task DeleteTables(Guid contextId, DeleteTestingContextDto result)
    {
        var tablesToDelete = GetAllTableNames()
            .WhereAsync(name => name.EndsWith($"{TestTableDelimiter}{EscapeContextId(contextId)}"));

        await foreach (var tableName in tablesToDelete)
        {
            await DeleteTable(tableName, result);
        }
    }

    private async Task DeleteTable(string tableName, DeleteTestingContextDto result)
    {
        if (!tableName.Contains(TestTableDelimiter))
        {
            throw new ArgumentException($"Cannot delete real table: {tableName}");
        }

        var tableServiceClient = _tableServiceClientFactory.CreateTableServiceClient();
        await tableServiceClient.DeleteTableAsync(tableName);
        result.RemovedTables.Add(tableName);
    }

    private static string GetTableName(string tableName, Guid? contextId)
    {
        return $"{tableName}{TestTableDelimiter}{EscapeContextId(contextId)}";
    }

    private static string EscapeContextId(Guid? contextId)
    {
        if (contextId == null)
        {
            return null;
        }

        return contextId.ToString().Replace("-", "");
    }

    private void SetContextCookie(Guid contextId)
    {
        var response = _httpContextAccessor.HttpContext?.Response;

        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
        };
        response?.Cookies.Append(ContextIdCookieName, contextId.ToString(), options);
    }

    private void DeleteContextCookie()
    {
        var response = _httpContextAccessor.HttpContext?.Response;
        response?.Cookies.Delete(ContextIdCookieName);
    }

    private interface ITableProvisioningService
    {
        Task ProvisionTable(string sourceTableName, Guid newContextId, bool copyExistingTable,
            Dictionary<string,object>[] rows, ActionResultDto<TestingContextCreatedDto> result);

        Task AddRows(TableClient destinationTableClient, Dictionary<string,object>[] rows, ActionResultDto<TestingContextCreatedDto> result);
    }

    private class TableProvisioningService<T> : ITableProvisioningService where T : class, ITableEntity, new()
    {
        private readonly ITableClientFactory _tableClientFactory;
        
        public TableProvisioningService(ITableClientFactory tableClientFactory)
        {
            _tableClientFactory = tableClientFactory;
        }

        public async Task ProvisionTable(string sourceTableName, Guid newContextId, bool copyExistingTable,
            Dictionary<string,object>[] rows, ActionResultDto<TestingContextCreatedDto> result)
        {
            var sourceTableClient = _tableClientFactory.CreateTableClient(sourceTableName);
            var newTableName = GetTableName(sourceTableName, newContextId);
            var destinationTableClient = _tableClientFactory.CreateTableClient(newTableName);
            await destinationTableClient.CreateIfNotExistsAsync();

            if (!copyExistingTable)
            {
                result.Messages.Add($"Created table {newTableName}, copied no data");
                if (rows != null)
                {
                    await AddRows(destinationTableClient, rows, result);
                }
                return;
            }

            var rowsCopied = 0;
            await foreach (var sourceRow in sourceTableClient.QueryAsync<T>())
            {
                await destinationTableClient.AddEntityAsync(sourceRow);
                rowsCopied++;
            }
            result.Messages.Add($"Copied {rowsCopied} rows to {newTableName} from {sourceTableName}");

            if (rows != null)
            {
                await AddRows(destinationTableClient, rows, result);
            }
        }

        public async Task AddRows(TableClient destinationTableClient, Dictionary<string,object>[] rows, ActionResultDto<TestingContextCreatedDto> result)
        {
            var index = 0;
            foreach (var row in rows)
            {
                var instance = DeserializeRow(row, index++, result);
                await destinationTableClient.AddEntityAsync(instance);
            }

            result.Messages.Add($"Inserted {rows.Length} into {destinationTableClient.Name}");
        }

        private static T DeserializeRow(Dictionary<string,object> row, int index, ActionResultDto<TestingContextCreatedDto> result)
        {
            var instance = new T();
            var properties = typeof(T).GetProperties().ToDictionary(p => p.Name);

            foreach (var property in row)
            {
                if (!properties.TryGetValue(property.Key, out var propertyInfo))
                {
                    result.Errors.Add($"Unable to set property {instance.GetType().Name}.{property.Key} for row {index} as it cannot be found");
                    continue;
                }

                try
                {
                    propertyInfo.SetValue(instance, property.Value);
                }
                catch (Exception exc)
                {
                    result.Errors.Add($"Unable to set property {instance.GetType().Name}.{property.Key} for row {index}: {exc.Message}");
                }
            }

            return instance;
        }
    }
}