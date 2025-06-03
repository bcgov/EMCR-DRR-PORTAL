using AutoMapper;
using EMCR.DRR.API.Resources.Payments;
using EMCR.DRR.API.Services;
using EMCR.DRR.API.Services.CAS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMCR.DRR.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(Policy = "OnlySSO")]
    public class CASController : ControllerBase
    {
        private readonly ILogger<CASController> logger;
        private readonly IMapper mapper;
        private readonly ErrorParser errorParser;
        private readonly ICasGateway casGateway;
        //TODO - update client to use gateway instead

        public CASController(ILogger<CASController> logger, IMapper mapper, ICasGateway casGateway)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.errorParser = new ErrorParser();
            this.casGateway = casGateway;
        }

        [HttpGet("supplier/{supplierNumber}")]
        public async Task<ActionResult<GetSupplierResponse>> GetSupplierByNumber(string supplierNumber)
        {
            try
            {
                var ct = new CancellationTokenSource().Token;
                var res = await casGateway.GetSupplier(supplierNumber, null, ct);
                return Ok(res);
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpGet("supplier/{supplierNumber}/site/{siteCode}")]
        public async Task<ActionResult<GetSupplierResponse>> GetSupplierByNumberAndCode(string supplierNumber, string siteCode)
        {
            try
            {
                var ct = new CancellationTokenSource().Token;
                var res = await casGateway.GetSupplier(supplierNumber, siteCode, ct);
                return Ok(res);
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpGet("supplierbyname/{supplierName}/{postalCode}")]
        public async Task<ActionResult<GetSupplierResponse>> GetSupplierByName(string supplierName, string postalCode)
        {
            try
            {
                var ct = new CancellationTokenSource().Token;
                var res = await casGateway.GetSupplierByName(supplierName, postalCode, ct);
                return Ok(res);
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpPost("supplier")]
        public async Task<ActionResult<CreateSupplierResponse>> CreateSupplier([FromBody] CreateSupplierRequest supplierRequest)
        {
            try
            {
                var ct = new CancellationTokenSource().Token;
                //var res = await casGateway.QueryInvoice(supplierRequest, ct);
                await Task.CompletedTask;
                return Ok();
                //return Ok(res);
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpGet("invoice")]
        public async Task<ActionResult<GetInvoiceResponse>> GetInvoice([FromQuery] InvoicesQuery options)
        {
            try
            {
                var ct = new CancellationTokenSource().Token;
                var res = await casGateway.QueryInvoice(options.InvoiceNumber, options.SupplierNumber, options.SupplierSiteCode, ct);
                await Task.CompletedTask;
                return Ok();
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpPost("invoice")]
        public async Task<ActionResult<InvoiceResponse>> CreateInvoice([FromBody] Invoice invoice)
        {
            try
            {
                var ct = new CancellationTokenSource().Token;
                //var res = await casGateway.CreateInvoiceAsync(invoice, ct);
                await Task.CompletedTask;
                return Ok();
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }
    }

    //public class SupplierQuery
    //{
    //    public string FirstName { get; set; } = null!;
    //    public string LastName { get; set; } = null!;
    //    public string PostalCode { get; set; } = null!;
    //}

    public class InvoicesQuery
    {
        public string? InvoiceNumber { get; set; }
        public string? SupplierNumber { get; set; }
        public string? SupplierSiteCode { get; set; }
    }

    //public class InvoiceQuery
    //{
    //    public string? InvoiceNumber { get; set; }
    //    public string? SupplierNumber { get; set; }
    //    public string? SupplierSiteCode { get; set; }
    //}
}
