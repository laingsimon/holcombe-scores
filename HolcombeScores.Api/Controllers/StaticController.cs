using System.Net.Mime;
using HolcombeScores.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HolcombeScores.Api.Controllers
{
    public class StaticController : Controller
    {
        private const string UrlPrefix = "/static";
        private readonly ISocialService _socialService;

        public StaticController(ISocialService socialService)
        {
            _socialService = socialService;
        }

        [HttpGet(UrlPrefix + "/{url1}")]
        // ReSharper disable UnusedParameter.Global
        public async Task<ContentResult> Html(string url1)
            // ReSharper restore UnusedParameter.Global
        {
            return await GenerateHtml();
        }

        [HttpGet(UrlPrefix + "/{url1}/{url2}")]
        // ReSharper disable UnusedParameter.Global
        public async Task<ContentResult> Html(string url1, string url2)
            // ReSharper restore UnusedParameter.Global
        {
            return await GenerateHtml();
        }

        [HttpGet(UrlPrefix + "/{url1}/{url2}/{url3}")]
        // ReSharper disable UnusedParameter.Global
        public async Task<ContentResult> Html(string url1, string url2, string url3)
            // ReSharper restore UnusedParameter.Global
        {
            return await GenerateHtml();
        }

        private async Task<ContentResult> GenerateHtml()
        {
            var path = Request.Path.ToUriComponent();
            var redirect = true;
            if (Request.Query.TryGetValue("hold", out var hold))
            {
                redirect = !hold.Equals("true");
            }

            return new ContentResult
            {
                Content = await _socialService.GetHtml(path.Substring(UrlPrefix.Length), redirect),
                ContentType = MediaTypeNames.Text.Html,
            };
        }
    }
}