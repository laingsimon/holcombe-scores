using System.Text.RegularExpressions;

namespace HolcombeScores.Api.Models;

public class TestingContext : ITestingContext
{
    public const string ContextIdCookieName = "HS_TestingContext";
    private const string TestTableDelimiter = "TEST";

    public string ContextId { get; set; }
    public bool TestingContextRequired { get; set; }

    public string GetTableName(string tableName)
    {
        if (ContextId == null && TestingContextRequired)
        {
            throw new InvalidOperationException("TestingContext is required but not supplied");
        }

        return ContextId == null
            ? tableName
            : GetTableName(tableName, ContextId);
    }

    public bool IsTestingTable(string tableName)
    {
        return ContextId != null
            ? tableName.EndsWith($"{TestTableDelimiter}{ContextId}")
            : tableName.Contains(TestTableDelimiter);
    }

    public bool IsRealTable(string tableName)
    {
        return !tableName.Contains(TestTableDelimiter);
    }

    public string GetContextIdFromTableName(string tableName)
    {
        if (IsRealTable(tableName))
        {
            return null;
        }

        var match = Regex.Match(tableName, $@".+{TestTableDelimiter}(?<escapedContextId>.+)$");
        return match.Success
            ? match.Groups["escapedContextId"].Value
            : null;
    }

    private static string GetTableName(string tableName, string alternativeContextId)
    {
        return $"{tableName}{TestTableDelimiter}{alternativeContextId}";
    }
}