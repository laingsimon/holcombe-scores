using HolcombeScores.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace HolcombeScores.Test;

public static class ApiLauncher
{
    public static IDisposable LaunchApi(string apiUrl, Dictionary<string, string> configuration)
    {
        var hostBuilder = Host.CreateDefaultBuilder(Array.Empty<string>())
            .ConfigureAppConfiguration(b => b.AddInMemoryCollection(configuration))
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseUrls(apiUrl);
            });

        var host = hostBuilder.Build();

#pragma warning disable CS4014
        // intentionally don't await here as otherwise it would block
        host.RunAsync();
#pragma warning restore CS4014

        return new RunningApi(host);
    }

    private class RunningApi : IDisposable
    {
        private readonly IHost _host;

        public RunningApi(IHost host)
        {
            _host = host;
        }

        public void Dispose()
        {
            _host.StopAsync().Wait();
        }
    }
}