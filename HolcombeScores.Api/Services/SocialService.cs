using HolcombeScores.Api.Models;
using HolcombeScores.Api.Repositories;

namespace HolcombeScores.Api.Services;

public class SocialService : ISocialService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly ISocialDetailRepository _socialDetailRepository;

    public SocialService(
        IWebHostEnvironment webHostEnvironment,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        ISocialDetailRepository socialDetailRepository)
    {
        _webHostEnvironment = webHostEnvironment;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _socialDetailRepository = socialDetailRepository;
    }

    public async Task<string> GetHtml(string pathWithoutStatic, bool redirect = true)
    {
        var pathToHtml = Path.Combine(_webHostEnvironment.ContentRootPath, _configuration["IndexHtmlPath"]);
        var html = await File.ReadAllTextAsync(pathToHtml);
        var socialDetail = await GetSocialDetail(pathWithoutStatic);

        html = html.Replace("${redirectTime}", redirect ? "0" : "-1");

        return socialDetail
            .ToVariables()
            .Aggregate(
                html,
                (current, next) => current.Replace("${" + next.Key + "}", next.Value));
    }

    private async Task<SocialDetail> GetSocialDetail(string pathWithoutStatic)
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        var uiHost = _configuration["UiHostName"] ?? request?.Host.Host;

        var socialDetail = await _socialDetailRepository.GetSocialDetail(pathWithoutStatic);
        socialDetail.Url ??= $"{request?.Scheme}://{uiHost}{pathWithoutStatic}";
        socialDetail.Type ??= "website";
        socialDetail.Image ??= $"{request?.Scheme}://{uiHost}/1024x1024.png";

        return socialDetail;
    }
}