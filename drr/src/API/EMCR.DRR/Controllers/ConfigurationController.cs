using EMCR.DRR.API.Services;
using EMCR.DRR.Managers.Intake;
using EMCR.Utilities.Caching;
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
    [ResponseCache(Duration = cacheDuration)]
    public class ConfigurationController : ControllerBase
    {
        private readonly ILogger<ConfigurationController> logger;
        private readonly IConfiguration configuration;
        private readonly IIntakeManager intakeManager;
        private readonly ICache cache;
        private readonly ErrorParser errorParser;
        private const int cacheDuration = 60 * 1; //1 minute

        public ConfigurationController(ILogger<ConfigurationController> logger, IConfiguration configuration, IIntakeManager intakeManager, ICache cache)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.intakeManager = intakeManager;
            this.cache = cache;
            errorParser = new ErrorParser();
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
                TestDataEndpointsEnabled = configuration.GetValue("TestDataEndpointsEnabled", false),
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

        [HttpGet("options")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<EntitiesQueryResult>> GetEntities()
        {
            try
            {
                var res = await cache.GetOrSet(
                "entities",
                async () => (await intakeManager.Handle(new EntitiesQuery())),
                TimeSpan.FromMinutes(60));

                return Ok(res);
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }
    }

    public class Configuration
    {
        public required OidcConfiguration Oidc { get; set; }
        public required bool TestDataEndpointsEnabled { get; set; } = false;
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
