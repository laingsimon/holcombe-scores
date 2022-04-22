using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HolcombeScores.Api.Services
{
    public interface IClientService
    {
        Task<Stream> GetClientLibraryBundle(HttpResponse httpResponse);
        Task<Stream> GetTestBed(HttpResponse httpResponse);
    }
}