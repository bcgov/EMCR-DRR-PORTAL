using EMCR.DRR.API.Services.CAS;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace EMCR.Tests.Integration.DRR.CAS
{
    public class CASWebProxyTests
    {
        Alba.IAlbaHost host;
        IWebProxy client;
        public CASWebProxyTests()
        {
            host = Application.Host;
            client = host.Services.GetRequiredService<IWebProxy>();
        }

        [Test]
        public async Task CreateToken_Success()
        {
            var token = await client.CreateTokenAsync(CancellationToken.None);
            token.ShouldNotBeNull();
        }

        [Test]
        public async Task CreateInvoice_NewInvoice_Success()
        {
            var uniqueId = Guid.NewGuid().ToString().Substring(0, 4);
            var now = DateTime.UtcNow;
            var invoice = new Invoice
            {
                SupplierNumber = "2002739",
                SupplierSiteNumber = "001",
                InvoiceNumber = $"drr_unittest_{uniqueId}",
                InvoiceBatchName = "drr_unittest",
                PayGroup = "GEN EFT",
                DateInvoiceReceived = now,
                InvoiceDate = now,
                GlDate = now,
                InvoiceAmount = 100m,
                InvoiceLineDetails = new[]
                {
                    new InvoiceLineDetail
                    {
                        DefaultDistributionAccount = "105.15006.10120.5001.1500000.000000.0000",
                        InvoiceLineAmount = 100m,
                        InvoiceLineNumber = 1
                    }
                },
                InteracEmail = $"{uniqueId}drrunittest@test.gov.bc.ca",
                InteracMobileCountryCode = "1",
                InteracMobileNumber = "6040000000",
                RemittanceMessage1 = "security question",
                RemittanceMessage2 = "answer"
            };
            var response = await client.CreateInvoiceAsync(invoice, CancellationToken.None);

            response.ShouldNotBeNull().IsSuccess().ShouldBeTrue();
            response.InvoiceNumber.ShouldNotBeNull().ShouldBe(invoice.InvoiceNumber);
        }

        [Test]
        public async Task GetSupplier_New_NotFound()
        {
            var response = await client.GetSupplierByNameAsync(new GetSupplierByNameRequest { PostalCode = "V1V1V1", SupplierName = "NOTEXIST, SUPPLIER" }, CancellationToken.None);
            response.ShouldBeNull();
        }

        [Test]
        public async Task CreateSupplier_New_Created()
        {
            var uniqueId = Guid.NewGuid().ToString().Substring(0, 4);
            var name = Formatters.ToCasSupplierName($"autotest-dev-{uniqueId}", $"autotest-dev-{uniqueId}");
            var postalCode = "V1V1V1".ToCasPostalCode();

            var createResponse = await client.CreateSupplierAsync(new CreateSupplierRequest
            {
                SubCategory = "Individual",
                SupplierName = name,
                SupplierAddress = new Supplieraddress[]
                {
                    new Supplieraddress
                    {
                        ProviderId = "CAS_SU_AT_DRR",
                        AddressLine1 = "123 test st.".StripSpecialCharacters(),
                        PostalCode = postalCode,
                        City = "test city".ToCasCity(),
                        Province = "BC",
                        Country = "CA"
                    }
                }
            }, CancellationToken.None);
            createResponse.ShouldNotBeNull();
            createResponse.IsSuccess().ShouldBeTrue(createResponse.CASReturnedMessages);
            createResponse.SupplierNumber.ShouldNotBeNullOrEmpty();
            createResponse.SupplierSiteCode.ShouldNotBeNullOrEmpty();

            var getResponse = await client.GetSupplierByNameAsync(new GetSupplierByNameRequest { PostalCode = postalCode, SupplierName = name }, CancellationToken.None);
            getResponse.ShouldNotBeNull();
            getResponse.Suppliernumber.ShouldBe(createResponse.SupplierNumber);
            getResponse.SupplierAddress.ShouldHaveSingleItem().Suppliersitecode.ShouldBe(createResponse.SupplierSiteCode.StripCasSiteNumberBrackets());
        }

        [Test]
        public async Task GetSupplier_ExistingCorrectPostalCode_Found()
        {
            var mockedCas = (MockCasProxy)host.Services.GetRequiredService<IWebProxy>();
            mockedCas.AddSupplier(new GetSupplierResponse
            {
                Suppliernumber = "2005363",
                Suppliername = Formatters.ToCasSupplierName("autotest-dev-first", "autotest-dev-last"),
                Businessnumber = "123",
                SupplierAddress = new[] { new Supplieraddress { AddressLine1 = "123 test st", PostalCode = "V8Z 7X9".ToCasPostalCode(), Suppliersitecode = "001" } }
            });

            var name = Formatters.ToCasSupplierName("autotest-dev-first", "autotest-dev-last");
            var postalCode = "V8Z 7X9".ToCasPostalCode();
            var response = await client.GetSupplierByNameAsync(new GetSupplierByNameRequest { PostalCode = postalCode, SupplierName = name }, CancellationToken.None);

            response.ShouldNotBeNull();
            response.Suppliernumber.ShouldBe("2005363");
            response.SupplierAddress.ShouldHaveSingleItem().Suppliersitecode.ShouldBe("001");
        }

        [Test]
        public async Task GetSupplier_ExistingIncorrectPostalCode_NotFound()
        {
            var mockedCas = (MockCasProxy)host.Services.GetRequiredService<IWebProxy>();
            mockedCas.AddSupplier(new GetSupplierResponse
            {
                Suppliernumber = "2005363",
                Suppliername = Formatters.ToCasSupplierName("autotest-dev-first", "autotest-dev-last"),
                Businessnumber = "123",
                SupplierAddress = new[] { new Supplieraddress { AddressLine1 = "123 test st", PostalCode = "V8Z 7X9".ToCasPostalCode(), Suppliersitecode = "001" } }
            });

            var name = Formatters.ToCasSupplierName("autotest-dev-first", "autotest-dev-last");
            var postalCode = "V8Z 7X8".ToCasPostalCode();
            var response = await client.GetSupplierByNameAsync(new GetSupplierByNameRequest { PostalCode = postalCode, SupplierName = name }, CancellationToken.None);

            response.ShouldNotBeNull();
            response.Suppliernumber.ShouldBe("2005363");
            response.SupplierAddress.ShouldBeEmpty();
        }
    }
}
