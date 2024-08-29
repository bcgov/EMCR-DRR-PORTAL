using System.Security.Claims;
using System.Text.Json.Serialization;
using EMBC.DRR.API.Services;
using EMBC.DRR.Managers.Intake;
using EMCR.DRR.Controllers;
using EMCR.DRR.Dynamics;
using EMCR.DRR.Managers.Intake;
using EMCR.DRR.Resources.Applications;
using EMCR.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.AspNetCore;
using NSwag.Generation.Processors.Security;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddControllers().AddJsonOptions(x =>
{
    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
services.AddRouting(o => o.LowercaseUrls = true);
services.AddEndpointsApiExplorer();
services.AddIntakeManager();
services.AddRepositories();
services.AddTransient<IUserService, UserService>();
services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
services.AddAutoMapper(typeof(ApplicationMapperProfile));
services.AddAutoMapper(typeof(IntakeMapperProfile));
services.AddCors(opts => opts.AddDefaultPolicy(policy =>
{
    policy.AllowAnyHeader();
    policy.AllowAnyMethod();
    policy.AllowAnyOrigin();

    //policy.WithOrigins("https://dev-drr-emcr.silver.devops.bcgov");
}));
services.AddCache(string.Empty)
    .AddDRRDynamics(builder.Configuration);

services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
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
});
services.AddAuthorization(options =>
{
    options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
    {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .RequireClaim("user_info");
    });
    options.DefaultPolicy = options.GetPolicy(JwtBearerDefaults.AuthenticationScheme) ?? null!;
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

services.Configure<SwaggerUi3Settings>(options =>
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
    document.GenerateAbstractProperties = true;
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
    app.UseSwaggerUi3();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
