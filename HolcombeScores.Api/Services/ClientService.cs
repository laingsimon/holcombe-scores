using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace HolcombeScores.Api.Services
{
    public class ClientService : IClientService
    {
        private const string JavaScriptContentType = "application/javascript";
        private const string ClientScriptsRelativePath = "./Scripts/Client";
        private const string TestBedRelativePath = "./Scripts/TestBed.html";

        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private byte[] _scriptContent;
        private byte[] _testBedContent;

        public ClientService(IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Stream> GetClientLibraryBundle(HttpResponse httpResponse)
        {
            if (Debugger.IsAttached)
            {
                // I.e. don't cache JS content when debugger is attached
                _scriptContent = null;
            }
            
            _scriptContent ??= await GetBundledContent(GetApiHost());
            
            if (_scriptContent == null)
            {
                httpResponse.StatusCode = StatusCodes.Status404NotFound;
                return Stream.Null;
            }

            var contentStream = new MemoryStream(_scriptContent);
            httpResponse.ContentType = JavaScriptContentType;
            return contentStream;
        }

        private string GetApiHost()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            return $"{request?.Scheme}://{request?.Host}{request?.Path.Value?.Replace("/api/Client", "/api")}";
        }

        public async Task<Stream> GetTestBed(HttpResponse httpResponse)
        {
            if (Debugger.IsAttached)
            {
                // I.e. don't cache html content when debugger is attached
                _testBedContent = null;
            }
            
            _testBedContent ??= await GetTestBedContent();

            if (_testBedContent == null)
            {
                httpResponse.StatusCode = StatusCodes.Status404NotFound;
                return Stream.Null;
            }
            
            var contentStream = new MemoryStream(_testBedContent);
            httpResponse.ContentType = MediaTypeNames.Text.Html;
            return contentStream;
        }
        
        private async Task<byte[]> GetTestBedContent()
        {
            var testBed = _webHostEnvironment.ContentRootFileProvider.GetFileInfo(TestBedRelativePath);
            if (!testBed.Exists)
            {
                return null;
            }

            var memoryStream = new MemoryStream();
            using (var fileStream = testBed.CreateReadStream())
            {
                await fileStream.CopyToAsync(memoryStream);
            }
            
            return memoryStream.ToArray();
        }

        private async Task<byte[]> GetBundledContent(string apiHost)
        {
            var memoryStream = new MemoryStream();
            var files = _webHostEnvironment.ContentRootFileProvider.GetDirectoryContents(ClientScriptsRelativePath);

            using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, -1, true))
            {
                await streamWriter.WriteLineAsync($"/*Bundle created at {DateTime.UtcNow:O}*/");
                foreach (var file in files.Where(f => f.Name.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
                             .OrderBy(f => f.Name))
                {
                    using (var fileStream = file.CreateReadStream())
                    using (var reader = new StreamReader(fileStream))
                    {
                        string line;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            await streamWriter.WriteLineAsync(line.Replace("%API_HOST%", apiHost));
                        }
                    }
                }
            }

            return memoryStream.ToArray();
        }
    }
}