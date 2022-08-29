namespace HolcombeScores.Api.Services;

public interface ISocialService
{
    Task<string> GetHtml(string pathWithoutStatic, bool redirect = true);
}