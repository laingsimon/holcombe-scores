namespace HolcombeScores.Api.Services;

public class TestingContext : ITestingContext
{
    internal const string ContextIdCookieName = "HS_TestingContext";

    private const string TestTableDelimiter = "TEST";

    public bool IsTesting => ContextId != null;

    public Guid? ContextId { get; set; }

    public string GetTableName(string tableName)
    {
        return ContextId == null
            ? tableName
            : GetTableName(tableName, ContextId.Value);
    }

    public string GetTableName(string tableName, Guid alternativeContextId)
    {
        return $"{tableName}{TestTableDelimiter}{EscapeContextId(alternativeContextId)}";
    }

    public bool IsTestingTable(string tableName)
    {
        return ContextId != null
            ? tableName.EndsWith($"{TestTableDelimiter}{EscapeContextId(ContextId.Value)}")
            : tableName.Contains(TestTableDelimiter);
    }

    public bool IsRealTable(string tableName)
    {
        return !tableName.Contains(TestTableDelimiter);
    }

    private static string EscapeContextId(Guid? contextId)
    {
        return contextId == null
            ? null
            : contextId.ToString().Replace("-", "");
    }
}