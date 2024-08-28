using System.Net.Mime;
using System.Security.Claims;
using System.Text.Json;
using AutoMapper;
using EMBC.DRR.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMCR.DRR.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [Authorize]
    public class ProfileController : Controller
    {
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8604 // Possible null reference argument.
        private AccountDetails GetCurrentUserInfo() => JsonSerializer.Deserialize<AccountDetails>(User.FindFirstValue("user_info"));
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8603 // Possible null reference return.

        private readonly IMapper mapper;

        public ProfileController(IMapper mapper)
        {
            this.mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ProfileDetails>> ProfileDetails()
        {
            var userInfo = GetCurrentUserInfo();
            return Ok(await Task.FromResult(mapper.Map<ProfileDetails>(userInfo)));
        }
    }

    public class ProfileDetails
    {
        public required string BusinessName { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? Title { get; set; }
        public string? Department { get; set; }
        public string? Phone { get; set; }
        public required string Email { get; set; }
    }
}
