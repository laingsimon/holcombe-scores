﻿using Azure.Data.Tables;
using HolcombeScores.Api.Models.Dtos;
using HolcombeScores.Api.Repositories;

namespace HolcombeScores.Api.Services;

public class TestingService : ITestingService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceHelper _serviceHelper;
    private readonly ITableServiceClientFactory _tableServiceClientFactory;
    private readonly ITableClientFactory _tableClientFactory;
    private readonly ITestingContext _testingContext;

    public TestingService(
        IHttpContextAccessor httpContextAccessor,
        IServiceHelper serviceHelper,
        ITableServiceClientFactory tableServiceClientFactory,
        ITableClientFactory tableClientFactory,
        ITestingContext testingContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceHelper = serviceHelper;
        _tableServiceClientFactory = tableServiceClientFactory;
        _tableClientFactory = tableClientFactory;
        _testingContext = testingContext;
    }

    public async Task<ActionResultDto<TestingContextCreatedDto>> CreateTestingContext(CreateTestingContextRequestDto request)
    {
        var deletionResult = _testingContext.ContextId != null
            ? await EndTestingContext()
            : null;
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

        var newTestingContext = new TestingContext
        {
            ContextId = newContextId,
        };

        try
        {
            await ProvisionTables(request, newTestingContext, result);

            SetContextCookie(newTestingContext);
        }
        catch (Exception exc)
        {
            result.Errors.Add(exc.ToString());
            result.Success = false;
        }

        return result;
    }

    public async Task<ActionResultDto<DeleteTestingContextDto>> EndTestingContext()
    {
        if (_testingContext.ContextId == null)
        {
            return _serviceHelper.NotFound<DeleteTestingContextDto>("No testing context in session");
        }

        var result = new DeleteTestingContextDto
        {
            ContextId = _testingContext.ContextId.Value,
            RemovedTables = new List<string>(),
        };

        try
        {
            await DeleteTables(result);

            DeleteContextCookie();
        }
        catch (Exception exc)
        {
            return _serviceHelper.Error<DeleteTestingContextDto>(exc.ToString());
        }

        return _serviceHelper.Success("Testing context removed", result);
    }

    public Guid? GetTestingContextId()
    {
        return _testingContext.ContextId;
    }

    private async Task ProvisionTables(CreateTestingContextRequestDto request, ITestingContext newTestingContext,
        ActionResultDto<TestingContextCreatedDto> result)
    {
        if (request.Tables == null)
        {
            // copy ALL tables according to request.CopyExistingTables
            if (request.CopyExistingTables == true)
            {
                await CopyAllExistingTables(newTestingContext, result);
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
                newTestingContext,
                tableProvisionDetails.CopyExistingTable ?? request.CopyExistingTables ?? false, // default to NOT copying tables unless requested
                tableProvisionDetails.Rows,
                result);
        }
    }

    private async Task CopyAllExistingTables(ITestingContext newTestingContext, ActionResultDto<TestingContextCreatedDto> result)
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

            await tableProvisioningService.ProvisionTable(tableName, newTestingContext, true, null, result);
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
        return GetAllTableNames().WhereAsync(_testingContext.IsRealTable);
    }

    private async Task DeleteTables(DeleteTestingContextDto result)
    {
        var tablesToDelete = GetAllTableNames()
            .WhereAsync(name => _testingContext.IsTestingTable(name));

        await foreach (var tableName in tablesToDelete)
        {
            await DeleteTable(tableName, result);
        }
    }

    private async Task DeleteTable(string tableName, DeleteTestingContextDto result)
    {
        if (_testingContext.IsRealTable(tableName))
        {
            throw new ArgumentException($"Cannot delete real table: {tableName}");
        }

        var tableServiceClient = _tableServiceClientFactory.CreateTableServiceClient();
        await tableServiceClient.DeleteTableAsync(tableName);
        result.RemovedTables.Add(tableName);
    }

    private void SetContextCookie(ITestingContext newTestingContext)
    {
        var response = _httpContextAccessor.HttpContext?.Response;

        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
        };
        response?.Cookies.Append(TestingContext.ContextIdCookieName, newTestingContext.ContextId.ToString(), options);
    }

    private void DeleteContextCookie()
    {
        var response = _httpContextAccessor.HttpContext?.Response;
        response?.Cookies.Delete(TestingContext.ContextIdCookieName);
    }

    private interface ITableProvisioningService
    {
        Task ProvisionTable(string sourceTableName, ITestingContext newTestingContext, bool copyExistingTable,
            Dictionary<string,object>[] rows, ActionResultDto<TestingContextCreatedDto> result);
    }

    private class TableProvisioningService<T> : ITableProvisioningService where T : class, ITableEntity, new()
    {
        private readonly ITableClientFactory _tableClientFactory;

        public TableProvisioningService(ITableClientFactory tableClientFactory)
        {
            _tableClientFactory = tableClientFactory;
        }

        public async Task ProvisionTable(string sourceTableName, ITestingContext newTestingContext, bool copyExistingTable,
            Dictionary<string,object>[] rows, ActionResultDto<TestingContextCreatedDto> result)
        {
            var notTestingContext = new TestingContext();

            try
            {
                var sourceTableClient = _tableClientFactory.CreateTableClient(sourceTableName, notTestingContext);
                var destinationTableClient = _tableClientFactory.CreateTableClient(sourceTableName, newTestingContext);

                if (!copyExistingTable)
                {
                    result.Messages.Add($"Created table {destinationTableClient.Name}, copied no data");
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

                result.Messages.Add($"Copied {rowsCopied} rows to {destinationTableClient.Name} from {sourceTableClient.Name}");

                if (rows != null)
                {
                    await AddRows(destinationTableClient, rows, result);
                }
            }
            catch (Exception exc)
            {
                throw new InvalidOperationException($"Error provisioning table: {sourceTableName}", exc);
            }
        }

        private static async Task AddRows(TableClient destinationTableClient, Dictionary<string,object>[] rows, ActionResultDto<TestingContextCreatedDto> result)
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