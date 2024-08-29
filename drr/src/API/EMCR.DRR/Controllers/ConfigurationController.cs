using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMCR.DRR.API.Controllers
{
    /// <summary>
    /// Provides configuration data for clients
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public ConfigurationController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Get configuration settings for clients
        /// </summary>
        /// <returns>Configuration settings object</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Configuration>> GetConfiguration()
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            var config = new Configuration
            {
                Oidc = new OidcConfiguration
                {
                    ClientId = configuration.GetValue<string>("oidc:clientId"),
                    Issuer = configuration.GetValue<string>("oidc:issuer"),
                    Scope = configuration.GetValue<string>("oidc:scope", OidcConfiguration.DefaultScopes),
                    PostLogoutRedirectUri = configuration.GetValue<string>("oidc:PostLogoutRedirectUri"),
                    AccountRecoveryUrl = configuration.GetValue<string>("oidc:accountRecoveryUrl"),
                },
            };
#pragma warning restore CS8601 // Possible null reference assignment.

            return Ok(await Task.FromResult(config));
        }
    }

    public class Configuration
    {
        public required OidcConfiguration Oidc { get; set; }
    }

    public class OidcConfiguration
    {
        public const string DefaultScopes = "openid profile email";
        public required string Issuer { get; set; }
        public required string ClientId { get; set; }
        public required string PostLogoutRedirectUri { get; set; }
        public string Scope { get; set; } = DefaultScopes;
        public required string AccountRecoveryUrl { get; set; }
    }
}
