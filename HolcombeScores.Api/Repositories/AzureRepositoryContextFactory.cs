namespace HolcombeScores.Api.Repositories;

public class AzureRepositoryContextFactory : IAzureRepositoryContextFactory
{
    private readonly IConfiguration _configuration;

    public AzureRepositoryContextFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IAzureRepositoryContext CreateContext()
    {
        return new AzureRepositoryContext(_configuration);
    }
}