namespace HolcombeScores.Test.Configuration;

// ReSharper disable once ClassNeverInstantiated.Global
public class TestingConfigurationFactory
{
    public ITestingConfiguration Create()
    {
        var random = new Random();
        var apiHttpsPort = random.Next(5002, 6000);
        var uiHttpsPort = random.Next(7000, 8000);
        var fallback = new TestingConfiguration
        {
            ApiAddress = $"https://localhost:{apiHttpsPort}",
            UiAddress = $"https://localhost:{uiHttpsPort}",
            AdminPassCode = "test",
        };

        return new TestingConfiguration
        {
            AdminPassCode = GetConfig("AdminPassCode") ?? fallback.AdminPassCode,
            ApiAddress = GetConfig("ApiAddress") ?? fallback.ApiAddress,
            UiAddress = GetConfig("UiAddress") ?? fallback.UiAddress,
        };
    }

    private static string? GetConfig(string name)
    {
        return Environment.GetEnvironmentVariable(name);
    }
}