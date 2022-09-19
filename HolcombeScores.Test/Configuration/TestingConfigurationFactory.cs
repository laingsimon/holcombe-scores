namespace HolcombeScores.Test.Configuration;

public class TestingConfigurationFactory
{
    private static readonly ITestingConfiguration Default = new TestingConfiguration
    {
        ApiAddress = "https://localhost:5001",
        UiAddress = "https://localhost:44419",
        AdminPassCode = "test",
    };

    public ITestingConfiguration Create()
    {
        return new TestingConfiguration
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