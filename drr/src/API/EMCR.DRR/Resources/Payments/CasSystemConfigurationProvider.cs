using EMCR.DRR.Dynamics;
using EMCR.Utilities.Caching;
using Microsoft.OData.Client;

namespace EMCR.DRR.API.Resources.Payments
{
    public interface ICasSystemConfigurationProvider
    {
        Task<CasSystemConfiguration> Get(CancellationToken ct);
    }

    public class CasSystemConfiguration
    {
        public required string DefaultDistributionAccount { get; set; }
        public required string InvoiceType { get; set; }
        public required string InvoiceRemittanceCode { get; set; }
        public required string InvoiceSpecialHandling { get; set; }
        public required string PayGroup { get; set; }
        public required string InvoiceLineType { get; set; }
        public required string InvoiceLineCode { get; set; }
        public required string InvoiceTerms { get; set; }
        public required string CurrencyCode { get; set; }
        public required string ProviderId { get; set; }
    }

    internal class CasSystemConfigurationProvider : ICasSystemConfigurationProvider
    {
        private readonly IDRRContextFactory dRRContextFactory;
        private readonly ICache cache;
        private const string cacheKey = nameof(CasSystemConfiguration);

        public CasSystemConfigurationProvider(IDRRContextFactory dRRContextFactory, ICache cache)
        {
            this.dRRContextFactory = dRRContextFactory;
            this.cache = cache;
        }

#pragma warning disable CS8603 // Possible null reference return.
        public async Task<CasSystemConfiguration> Get(CancellationToken ct)
        {
            return await cache.GetOrSet(cacheKey, () => GetFromDynamics(ct), TimeSpan.FromMinutes(60), ct);
        }
#pragma warning restore CS8603 // Possible null reference return.

        private async Task<CasSystemConfiguration> GetFromDynamics(CancellationToken ct)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
            //var ctx = dRRContextFactory.CreateReadOnly();
            //var configValues = (await ((DataServiceQuery<drr_systemconfigs>)ctx.drr_systemconfigs.Where(sc => sc.drr_group == "eTransfer")).GetAllPagesAsync(ct)).ToArray();

            //return new CasSystemConfiguration
            //{
            //    CurrencyCode = configValues.SingleOrDefault(cv => cv.drr_key.Equals("Currency Code", StringComparison.OrdinalIgnoreCase))?.drr_value ?? "CAD",
            //    DefaultDistributionAccount = configValues.SingleOrDefault(cv => cv.drr_key.Equals("Default Distribution Account", StringComparison.OrdinalIgnoreCase))?.drr_securevalue,
            //    InvoiceType = configValues.SingleOrDefault(cv => cv.drr_key.Equals("Invoice Type", StringComparison.OrdinalIgnoreCase))?.drr_value ?? "Standard",
            //    InvoiceLineCode = configValues.SingleOrDefault(cv => cv.drr_key.Equals("Line Code", StringComparison.OrdinalIgnoreCase))?.drr_value ?? "DR",
            //    InvoiceTerms = configValues.SingleOrDefault(cv => cv.drr_key.Equals("Terms", StringComparison.OrdinalIgnoreCase))?.drr_value ?? "IMMEDIATE",
            //    InvoiceLineType = configValues.SingleOrDefault(cv => cv.drr_key.Equals("Type", StringComparison.OrdinalIgnoreCase))?.drr_value ?? "Item",
            //    PayGroup = configValues.SingleOrDefault(cv => cv.drr_key.Equals("Pay Group", StringComparison.OrdinalIgnoreCase))?.drr_value ?? "EMB IN",
            //    InvoiceRemittanceCode = configValues.SingleOrDefault(cv => cv.drr_key.Equals("Remittance Code", StringComparison.OrdinalIgnoreCase))?.drr_value ?? "01",
            //    InvoiceSpecialHandling = configValues.SingleOrDefault(cv => cv.drr_key.Equals("Special Handling", StringComparison.OrdinalIgnoreCase))?.drr_value ?? "N",
            //    ProviderId = configValues.SingleOrDefault(cv => cv.drr_key.Equals("Provider ID", StringComparison.OrdinalIgnoreCase))?.drr_value ?? "CAS_SU_AT_ESS"
            //};
        }
    }
}
