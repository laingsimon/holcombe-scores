namespace HolcombeScores.Api.Repositories;

public interface IAzureRepositoryContextFactory
{
    IAzureRepositoryContext CreateContext();
}