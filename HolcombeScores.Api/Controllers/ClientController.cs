using HolcombeScores.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HolcombeScores.Api.Controllers
{
    [ApiController]
    public class ClientController : Controller
    {
        private readonly IClientService _clientService;

        public ClientController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpGet("/api/Client/")]
        public async Task<Stream> GetContent()
        {
            return await _clientService.GetClientLibraryBundle(Response);
        }
        
        [HttpGet("/api/Client/TestBed")]
        public async Task<Stream> GetTestBed()
        {
            return await _clientService.GetTestBed(Response);
        }
    }
}