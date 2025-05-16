using Alba;
using Microsoft.Extensions.DependencyInjection;

namespace EMCR.Tests.Integration.DRR
{
    [SetUpFixture]
    public class Application
    {
        // Make this lazy so you don't build it out
        // when you don't need it.
        private static readonly Lazy<IAlbaHost> host = new Lazy<IAlbaHost>(() => Create());

        public static IAlbaHost Host => host.Value;

        // Make sure that NUnit will shut down the AlbaHost when
        // all the projects are finished
        [OneTimeTearDown]
        public void Teardown()
        {
            if (host.IsValueCreated)
            {
                host.Value.Dispose();
            }
        }

        private static IAlbaHost Create() => AlbaHost.For<Program>(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Remove(new ServiceDescriptor(typeof(EMCR.DRR.API.Services.CAS.IWebProxy), typeof(EMCR.DRR.API.Services.CAS.WebProxy), ServiceLifetime.Transient));
                services.AddSingleton<EMCR.DRR.API.Services.CAS.IWebProxy, MockCasProxy>();
                services.AddMvcCore();
            });
        }).GetAwaiter().GetResult();
    }
}
