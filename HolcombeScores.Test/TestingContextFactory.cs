namespace HolcombeScores.Test;

public class TestingContextFactory
{
    private static readonly ITestingContext Default = new TestingContext
    {
        ApiAddress = "https://localhost:5001",
        UiAddress = "https://localhost:44419",
        AdminPassCode = "test",
    };

    public ITestingContext Create()
    {
        return new TestingContext
        {
            AdminPassCode = GetConfig("AdminPassCode") ?? Default.AdminPassCode,
            ApiAddress = GetConfig("ApiAddress") ?? Default.ApiAddress,
            UiAddress = GetConfig("UiAddress") ?? Default.UiAddress,
        };
    }

    private static string? GetConfig(string name)
    {
        return Environment.GetEnvironmentVariable(name);
    }
}