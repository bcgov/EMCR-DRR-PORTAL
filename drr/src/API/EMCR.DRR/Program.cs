using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.Json.Serialization;
using AutoMapper;
using EMCR.DRR.API.Model;
using EMCR.DRR.API.Services;
using EMCR.DRR.API.Services.S3;
using EMCR.DRR.API.Utilities.Converters;
using EMCR.DRR.API.Utilities.TestData;
using EMCR.DRR.Controllers;
using EMCR.DRR.Dynamics;
using EMCR.DRR.Managers.Intake;
using EMCR.DRR.Resources.Applications;
using EMCR.Utilities.Caching;
using Medallion.Threading;
using Medallion.Threading.Redis;
using Medallion.Threading.WaitHandles;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using NSwag;
using NSwag.AspNetCore;
using NSwag.Generation.Processors.Security;
using Serilog;
using StackExchange.Redis;
using Xrm.Tools.WebAPI;
using Xrm.Tools.WebAPI.Requests;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.None;
    logging.RequestBodyLogLimit = 12000;
    logging.ResponseBodyLogLimit = 4096;
});

#pragma warning disable CS8604 // Possible null reference argument.
builder.Host.UseSerilog((ctx, services, config) => Logging.ConfigureSerilog(ctx, services, config, configuration.GetValue("APP_NAME", string.Empty)));
#pragma warning restore CS8604 // Possible null reference argument.
services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var loggerFactory = context.HttpContext.RequestServices
    .GetRequiredService<ILoggerFactory>();
            var problemDetails = new ValidationProblemDetails(context.ModelState)
            {
                Type = "Model Validation",
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Instance = context.HttpContext.Request.Path,
                Extensions =
                            {
                                ["traceId"] = Activity.Current?.Id ?? context.HttpContext?.TraceIdentifier
                            }
            };

            var logger = loggerFactory.CreateLogger(context.ActionDescriptor.DisplayName ?? "ModelValidation");
            var errorInfo = new Dictionary<string, Collection<string>>(StringComparer.Ordinal);
            if (logger != null && context.ModelState.Count > 0)
            {
                foreach (var modelState in context.ModelState)
                {
                    if (!errorInfo.TryGetValue(modelState.Key, out Collection<string>? errorMessages))
                    {
                        errorMessages = [];
                        errorInfo.Add(modelState.Key, errorMessages);
                    }

                    foreach (var error in modelState.Value.Errors)
                    {
                        errorMessages.Add(error.ErrorMessage);
                    }
                }

                logger.LogWarning("Validation errors: {@message}", errorInfo);
            }

            return new BadRequestObjectResult(problemDetails);
        };
    })
    .AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        x.JsonSerializerOptions.Converters.Add(new NullableDecimalConverter());
        x.JsonSerializerOptions.Converters.Add(new NullableIntConverter());
    });
services.AddRouting(o => o.LowercaseUrls = true);
services.AddEndpointsApiExplorer();
services.AddIntakeManager();
services.AddRepositories();
services.AddS3Storage(configuration);
services.AddTransient<IUserService, UserService>();
services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
services.AddAutoMapper(typeof(ApplicationMapperProfile));
services.AddAutoMapper(typeof(IntakeMapperProfile));
services.AddSingleton(sp =>
{
    var dynamicsApiEndpoint = configuration.GetValue<string>("Dynamics:DynamicsApiEndpoint");
    var tokenProvider = sp.GetRequiredService<ISecurityTokenProvider>();
    return new CRMWebAPI(new CRMWebAPIConfig
    {
        APIUrl = dynamicsApiEndpoint + "/api/data/v9.0/",
        GetAccessToken = async (s) => await tokenProvider.AcquireToken()
    });
});
services.AddCors(opts => opts.AddDefaultPolicy(policy =>
{
    policy.AllowAnyHeader();
    policy.AllowAnyMethod();
    policy.AllowAnyOrigin();

    //policy.WithOrigins("https://dev-drr-emcr.silver.devops.bcgov");


    //// try to get array of origins from section array
    //var corsOrigins = configuration.GetSection("cors:origins").GetChildren().Select(c => c.Value ?? string.Empty).ToArray();
    //// try to get array of origins from value
    //if (!corsOrigins.Any()) corsOrigins = configuration.GetValue("cors:origins", string.Empty)?.Split(',') ?? Array.Empty<string>();
    //corsOrigins = corsOrigins.Where(o => !string.IsNullOrWhiteSpace(o)).ToArray();
    //if (corsOrigins.Any())
    //{
    //    policy.SetIsOriginAllowedToAllowWildcardSubdomains().WithOrigins(corsOrigins);
    //}
}));
var redisConnectionString = configuration.GetValue("REDIS_CONNECTIONSTRING", string.Empty);
var appName = configuration.GetValue("APP_NAME", "drr-api");
#pragma warning disable CS8604 // Possible null reference argument.
if (!string.IsNullOrEmpty(redisConnectionString))
{
    Console.WriteLine("Configuring Redis cache");
    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
    });
    services.AddDataProtection()
        .SetApplicationName(appName)
        .PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect(redisConnectionString), $"{appName}-data-protection-keys");

    services.AddSingleton<IDistributedSemaphoreProvider>(new RedisDistributedSynchronizationProvider(ConnectionMultiplexer.Connect(redisConnectionString).GetDatabase()));
}
else
{
    Console.WriteLine("Configuring in-memory cache");
    var dataProtectionPath = configuration.GetValue("KEY_RING_PATH", string.Empty);
    services.AddDistributedMemoryCache();
    var dpBuilder = services.AddDataProtection()
        .SetApplicationName(appName);

    if (!string.IsNullOrEmpty(dataProtectionPath)) dpBuilder.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath));

    services.AddSingleton<IDistributedSemaphoreProvider>(new WaitHandleDistributedSynchronizationProvider());
}
#pragma warning restore CS8604 // Possible null reference argument.

services.ConfigureCache();
services.AddDRRDynamics(builder.Configuration);

var defaultScheme = "Bearer_OR_SSO";

#pragma warning disable CS8601 // Possible null reference assignment.
services.AddAuthentication(options =>
{
    options.DefaultScheme = defaultScheme;
    options.DefaultChallengeScheme = defaultScheme;
}).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.MetadataAddress = configuration.GetValue<string>("jwt:metadataAddress");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = true,
        ValidateIssuer = true,
        RequireSignedTokens = true,
        RequireAudience = true,
        RequireExpirationTime = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(60),
        NameClaimType = ClaimTypes.Upn,
        RoleClaimType = ClaimTypes.Role,
        ValidateActor = true,
        ValidateIssuerSigningKey = true,
    };

    configuration.GetSection("jwt").Bind(options);

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async c =>
        {
            var userService = c.HttpContext.RequestServices.GetRequiredService<IUserService>();
            Claim[] pathClaim =
            [
                new Claim("path", c.HttpContext.Request.Path.Value),
            ];
            c.Principal = new ClaimsPrincipal(new ClaimsIdentity(c.Principal.Identity, c.Principal.Claims.Concat(pathClaim)));

            c.Principal = await userService.GetPrincipal(c.Principal);
        }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
    };
    options.Validate();
})
.AddJwtBearer("SSO", options =>
{
    options.MetadataAddress = configuration.GetValue<string>("SSO:jwt:metadataAddress");
#pragma warning restore CS8601 // Possible null reference assignment.
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = true,
        ValidateIssuer = true,
        RequireSignedTokens = true,
        RequireAudience = true,
        RequireExpirationTime = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(60),
        NameClaimType = ClaimTypes.Upn,
        RoleClaimType = ClaimTypes.Role,
        ValidateActor = true,
        ValidateIssuerSigningKey = true,
    };

    configuration.GetSection("SSO:jwt").Bind(options);
    options.Validate();
})
.AddPolicyScheme(defaultScheme, defaultScheme, options =>
{
    options.ForwardDefaultSelector = context =>
    {
        string? authorization = context.Request.Headers[HeaderNames.Authorization];

        if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
        {
            var token = authorization["Bearer ".Length..].Trim();
            var jwtHandler = new JwtSecurityTokenHandler();

            if (jwtHandler.CanReadToken(token))
            {
                JwtSecurityToken jwtToken = jwtHandler.ReadJwtToken(token);
                var identityProviderClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "aud");
                if (identityProviderClaim != null && identityProviderClaim.Value.Equals(configuration.GetValue<string>("SSO:jwt:audience"), StringComparison.InvariantCultureIgnoreCase))
                {
                    return "SSO";
                }
                else
                    return JwtBearerDefaults.AuthenticationScheme;
            }
            return JwtBearerDefaults.AuthenticationScheme;
        }
        return JwtBearerDefaults.AuthenticationScheme;
    };
    options.Validate();
});

services.AddAuthorization(options =>
{
    options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
    {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .RequireClaim("user_info");
    });
    var ssoPolicyBuilder = new AuthorizationPolicyBuilder("SSO");
    options.AddPolicy("OnlySSO", ssoPolicyBuilder.RequireAuthenticatedUser().Build());
});

services.Configure<OpenApiDocumentMiddlewareSettings>(options =>
{
    options.Path = "/api/openapi/{documentName}/openapi.json";
    options.DocumentName = "DRR API";
    options.PostProcess = (document, req) =>
    {
        document.Info.Title = "DRR API";
    };
});

services.Configure<SwaggerUiSettings>(options =>
{
    options.Path = "/api/openapi";
    options.DocumentTitle = "DRR API Documentation";
    options.DocumentPath = "/api/openapi/{documentName}/openapi.json";
});

services.AddOpenApiDocument(document =>
{
    document.AddSecurity("bearer token", Array.Empty<string>(), new OpenApiSecurityScheme
    {
        Type = OpenApiSecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "paste token here",
        In = OpenApiSecurityApiKeyLocation.Header
    });

    document.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("bearer token"));
});

services.AddHealthChecks()
    .AddCheck($"ready hc", () => HealthCheckResult.Healthy("ready"), new[] { "ready" })
    .AddCheck($"live hc", () => HealthCheckResult.Healthy("alive"), new[] { "alive" });

var app = builder.Build();

app.MapHealthChecks("/hc/ready", new HealthCheckOptions() { Predicate = check => check.Tags.Contains("ready") });
app.MapHealthChecks("/hc/live", new HealthCheckOptions() { Predicate = check => check.Tags.Contains("live") });

// Configure the HTTP request pipeline.
if (!app.Environment.IsProduction())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseHttpsRedirection();
app.UseHttpLogging();
app.UseSerilogRequestLogging();
app.UseCors();
app.UseAuthorization();
app.MapControllers();



//Conditional endpoints to create test EOI and test FP
//To help testers save time setting up test data
var testDataEndpointsEnabled = configuration.GetValue("TestDataEndpointsEnabled", false);
if (testDataEndpointsEnabled)
{
    app.MapPost("/test-data/eoi", [Authorize] async (HttpContext ctx) =>
    {
#pragma warning disable CS8603 // Possible null reference return.
        string GetCurrentBusinessId() => ctx.User.FindFirstValue("bceid_business_guid");
        string GetCurrentBusinessName() => ctx.User.FindFirstValue("bceid_business_name");
        string GetCurrentUserId() => ctx.User.FindFirstValue("bceid_user_guid");
#pragma warning restore CS8603 // Possible null reference return.
        UserInfo GetCurrentUser()
        {
            return new UserInfo { BusinessId = GetCurrentBusinessId(), BusinessName = GetCurrentBusinessName(), UserId = GetCurrentUserId() };
        }

        var application = TestHelper.CreateNewTestEOIApplication();
        var manager = ctx.RequestServices.GetRequiredService<IIntakeManager>();
        var mapper = ctx.RequestServices.GetRequiredService<IMapper>();
        var id = await manager.Handle(new EoiSaveApplicationCommand { Application = mapper.Map<EoiApplication>(application), UserInfo = GetCurrentUser() });

        ctx.Response.StatusCode = (int)HttpStatusCode.Created;
        await ctx.Response.WriteAsJsonAsync(new { id = id });
    }).WithName("Create Test EOI");

    app.MapPost("/test-data/fp", async ctx =>
    {

#pragma warning disable CS8603 // Possible null reference return.
        string GetCurrentBusinessId() => ctx.User.FindFirstValue("bceid_business_guid");
        string GetCurrentBusinessName() => ctx.User.FindFirstValue("bceid_business_name");
        string GetCurrentUserId() => ctx.User.FindFirstValue("bceid_user_guid");
#pragma warning restore CS8603 // Possible null reference return.
        UserInfo GetCurrentUser()
        {
            return new UserInfo { BusinessId = GetCurrentBusinessId(), BusinessName = GetCurrentBusinessName(), UserId = GetCurrentUserId() };
        }
        var manager = ctx.RequestServices.GetRequiredService<IIntakeManager>();
        var mapper = ctx.RequestServices.GetRequiredService<IMapper>();

        EMCR.DRR.Managers.Intake.ScreenerQuestions CreateScreenerQuestions()
        {
            return new EMCR.DRR.Managers.Intake.ScreenerQuestions
            {
                ProjectWorkplan = true,
                ProjectSchedule = true,
                CostEstimate = true,
                SitePlan = EMCR.DRR.Managers.Intake.YesNoOption.Yes,
                HaveAuthorityToDevelop = true,
                FirstNationsAuthorizedByPartners = EMCR.DRR.Managers.Intake.YesNoOption.Yes,
                LocalGovernmentAuthorizedByPartners = EMCR.DRR.Managers.Intake.YesNoOption.Yes,
                FoundationWorkCompleted = EMCR.DRR.Managers.Intake.YesNoOption.Yes,
                EngagedWithFirstNationsOccurred = true,
                IncorporateFutureClimateConditions = true,
                MeetsRegulatoryRequirements = true,
                MeetsEligibilityRequirements = true,
            };
        }

        var eoi = mapper.Map<EoiApplication>(TestHelper.CreateNewTestEOIApplication());
        eoi.Status = SubmissionPortalStatus.EligibleInvited;
        eoi.AuthorizedRepresentativeStatement = true;
        eoi.FOIPPAConfirmation = true;
        eoi.InformationAccuracyStatement = true;
        var eoiId = await manager.Handle(new EoiSaveApplicationCommand { Application = eoi, UserInfo = GetCurrentUser() });

        var screenerQuestions = CreateScreenerQuestions();
        screenerQuestions.FirstNationsAuthorizedByPartners = EMCR.DRR.Managers.Intake.YesNoOption.NotApplicable;
        screenerQuestions.LocalGovernmentAuthorizedByPartners = EMCR.DRR.Managers.Intake.YesNoOption.NotApplicable;
        screenerQuestions.EngagedWithFirstNationsOccurred = false;

        var fpId = await manager.Handle(new CreateFpFromEoiCommand { EoiId = eoiId, UserInfo = GetCurrentUser(), ScreenerQuestions = screenerQuestions });

        ctx.Response.StatusCode = (int)HttpStatusCode.Created;
        await ctx.Response.WriteAsJsonAsync(new { id = fpId });
    }).WithName("Create Test FP");
}

app.Run();
