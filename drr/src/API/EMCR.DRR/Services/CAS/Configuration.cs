using EMCR.DRR.API.Resources.Payments;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace EMCR.DRR.API.Services.CAS
{
    public static class Configuration
    {
        public static IServiceCollection AddCAS(this IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetSection("CAS").Get<CasConfiguration>()!;
            if (options == null)
            {
                Console.WriteLine("Skipping CAS");
                return services;
            }
            services.Configure<CasConfiguration>(opts => configuration.GetSection("CAS").Bind(opts));
            var adfsTokenErrorHandlingPolicy = HttpPolicyExtensions.HandleTransientHttpError()
                .CircuitBreakerAsync(
                        options.CircuitBreakerNumberOfErrors,
                        TimeSpan.FromSeconds(options.CircuitBreakerResetInSeconds),
                        OnBreak,
                        OnReset);

            services
                .AddHttpClient("cas", (sp, client) =>
                {
                    var options = sp.GetRequiredService<IOptions<CasConfiguration>>().Value;
                    client.BaseAddress = options.BaseUrl;
                })
                .SetHandlerLifetime(TimeSpan.FromMinutes(30))
                .AddPolicyHandler((sp, request) =>
                {
                    var ctx = request.GetPolicyExecutionContext() ?? new Context();
                    ctx["_serviceprovider"] = sp;
                    ctx["_source"] = "cas-circuitbreaker";
                    request.SetPolicyExecutionContext(ctx);
                    return adfsTokenErrorHandlingPolicy;
                })
 ;

            services.TryAddTransient<IWebProxy, WebProxy>();
            services.TryAddTransient<ICasGateway, CasGateway>();
            services.TryAddTransient<ICasSystemConfigurationProvider, CasSystemConfigurationProvider>();

            return services;
        }

        private static void OnBreak(DelegateResult<HttpResponseMessage> r, TimeSpan time, Context ctx)
        {
            var source = (string)ctx["_source"];
            var sp = (IServiceProvider)ctx["_serviceprovider"];
            var logger = sp.GetRequiredService<ILogger<WebProxy>>();
            logger.LogError(r.Exception, "BREAK: {0} {1}: {2}", time, r.Result?.StatusCode, r.Result?.RequestMessage?.RequestUri);
        }

        private static void OnReset(Context ctx)
        {
            var source = (string)ctx["_source"];
            var sp = (IServiceProvider)ctx["_serviceprovider"];
            var logger = sp.GetRequiredService<ILogger<WebProxy>>();
            logger.LogInformation("RESET");
        }
    }

    public class CasConfiguration
    {
        public Uri BaseUrl { get; set; } = null!;
        public string ClientId { get; set; } = null!;
        public string ClientSecret { get; set; } = null!;
        public int CircuitBreakerNumberOfErrors { get; set; } = 3;
        public int CircuitBreakerResetInSeconds { get; set; } = 120;
    }
}
