using Amazon.S3;

namespace EMCR.DRR.API.Services.S3
{
    public static class Configuration
    {
        public static IServiceCollection AddS3Storage(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = GetSettings(configuration);

            if (settings != null && settings.Url != null)
            {
                services.AddOptions<S3StorageProviderSettings>().Bind(configuration.GetSection("S3"));

                services.AddSingleton<IAmazonS3>(_ =>
                  new AmazonS3Client(
                    settings.AccessKey,
                    settings.SecretKey,
                    new AmazonS3Config
                    {
                        ServiceURL = settings.Url,
                        ForcePathStyle = true,
                    }));
                services.AddSingleton<IS3Provider, S3Provider>();
            }
            return services;
        }

        private static S3StorageProviderSettings? GetSettings(IConfiguration configuration)
    => configuration.GetSection("S3").Get<S3StorageProviderSettings>();
    }
}
