using System.Web;
using EMCR.DRR.API.Services.CAS;

namespace EMCR.DRR.API.Resources.Payments
{
    public interface ICasGateway
    {
        public Task<(string SupplierNumber, string SiteCode)?> GetSupplier(string supplierNumber, string? siteCode, CancellationToken ct);
        public Task<(string SupplierNumber, string SiteCode)?> GetSupplierByName(string supplierName, string postalCode, CancellationToken ct);

        //public Task<(string SupplierNumber, string SiteCode)> CreateSupplier(contact contact, CancellationToken ct);

        public Task<IEnumerable<InvoiceItem>> QueryInvoices(string? status, DateTime? statusChangedFrom, DateTime? statusChangedTo, CancellationToken ct);

        //public Task<string> CreateInvoice(string batchName, drr_etransfertransaction payment, CancellationToken ct);

        public Task<InvoiceItem> QueryInvoice(string? invoiceNumber, string? suppliernumber, string? suppliersitecode, CancellationToken ct);
    }

    public class CasException : Exception
    {
        public CasException(string message) : base(message)
        {
        }

        public CasException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    internal class CasGateway : ICasGateway
    {
        private readonly IWebProxy casWebProxy;
        private readonly ICasSystemConfigurationProvider casSystemConfigurationProvider;

        public CasGateway(IWebProxy casWebProxy, ICasSystemConfigurationProvider casSystemConfigurationProvider)
        {
            this.casWebProxy = casWebProxy;
            this.casSystemConfigurationProvider = casSystemConfigurationProvider;
        }

        public async Task<(string SupplierNumber, string SiteCode)?> GetSupplier(string supplierNumber, string? siteCode, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(supplierNumber)) throw new ArgumentNullException(nameof(supplierNumber));

            var response = await casWebProxy.GetSupplierAsync(new GetSupplierRequest
            {
                SupplierNumber = supplierNumber,
                SiteCode = siteCode
            }, ct);
            if (response == null || !response.SupplierAddress.Any()) return null;
            return (SupplierNumber: response.Suppliernumber, SiteCode: response.SupplierAddress.First().Suppliersitecode.StripCasSiteNumberBrackets());
        }

        public async Task<(string SupplierNumber, string SiteCode)?> GetSupplierByName(string supplierName, string postalCode, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(supplierName)) throw new ArgumentNullException(nameof(supplierName));
            if (string.IsNullOrEmpty(postalCode)) throw new ArgumentNullException(nameof(postalCode));

            var response = await casWebProxy.GetSupplierByNameAsync(new GetSupplierByNameRequest
            {
                PostalCode = postalCode.ToCasPostalCode(),
                SupplierName = supplierName
            }, ct);
            if (response == null || !response.SupplierAddress.Any()) return null;
            return (SupplierNumber: response.Suppliernumber, SiteCode: response.SupplierAddress.First().Suppliersitecode.StripCasSiteNumberBrackets());
        }

        public async Task<IEnumerable<InvoiceItem>> QueryInvoices(string? status, DateTime? statusChangedFrom, DateTime? statusChangedTo, CancellationToken ct)
        {
            //if (string.IsNullOrEmpty(status)) throw new ArgumentNullException(nameof(status));
            var config = await casSystemConfigurationProvider.Get(ct);
            var response = await casWebProxy.GetInvoiceAsync(new GetInvoiceRequest
            {
                PayGroup = config.PayGroup,
                PaymentStatusDateFrom = statusChangedFrom,
                PaymentStatusDateTo = statusChangedTo,
                PaymentStatus = status
            }, ct);

            var items = new List<InvoiceItem>(response.Items);

            //get all pages
            while (response.Next != null)
            {
                var queryParams = HttpUtility.ParseQueryString(new Uri(response.Next.Ref).Query);
                if (!int.TryParse(queryParams.Get("page"), out var nextPageNumber)) break;
                response = await casWebProxy.GetInvoiceAsync(new GetInvoiceRequest
                {
                    PayGroup = config.PayGroup,
                    PaymentStatusDateFrom = statusChangedFrom,
                    PaymentStatusDateTo = statusChangedTo,
                    PaymentStatus = status,
                    PageNumber = nextPageNumber
                }, ct);
                items.AddRange(response.Items);
            }

            return items;
        }

        public async Task<InvoiceItem> QueryInvoice(string? invoiceNumber, string? suppliernumber, string? suppliersitecode, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(invoiceNumber)) throw new ArgumentNullException(nameof(invoiceNumber));
            if (string.IsNullOrEmpty(suppliernumber)) throw new ArgumentNullException(nameof(suppliernumber));
            if (string.IsNullOrEmpty(suppliersitecode)) throw new ArgumentNullException(nameof(suppliersitecode));

            var config = await casSystemConfigurationProvider.Get(ct);
            var response = await casWebProxy.GetInvoiceAsync(new GetInvoiceRequest
            {
                PayGroup = config.PayGroup,
                InvoiceNumber = invoiceNumber,
                SupplierNumber = suppliernumber,
                SupplierSiteCode = suppliersitecode,
            }, ct);

            var items = new List<InvoiceItem>(response.Items);
            if (items.Count > 0)
            {
                return items[0];
            }
            //should only be one page
#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
