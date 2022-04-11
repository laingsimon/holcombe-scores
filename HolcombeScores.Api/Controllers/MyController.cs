using System.Collections.Generic;
using System.Threading.Tasks;
using HolcombeScores.Api.Models;
using HolcombeScores.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HolcombeScores.Api.Controllers
{
    [ApiController]
    public class MyController : Controller
    {
        private readonly IAccessService _accessService;

        public MyController(IAccessService accessService)
        {
            _accessService = accessService;
        }

        [HttpGet("/api/My/Access")]
        public async Task<MyAccessDto> ListAccess()
        {
            return await _accessService.GetMyAccess();
        }
    }
}
