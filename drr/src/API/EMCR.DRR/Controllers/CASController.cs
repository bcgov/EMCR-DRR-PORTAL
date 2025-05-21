using AutoMapper;
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
        private readonly IWebProxy casClient;
        //TODO - update client to use gateway instead

        public CASController(ILogger<CASController> logger, IMapper mapper, IWebProxy casClient)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.errorParser = new ErrorParser();
            this.casClient = casClient;
        }

        [HttpGet("supplier")]
        public async Task<ActionResult<GetSupplierResponse>> GetSupplier([FromBody] GetSupplierRequest supplierRequest)
        {
            try
            {
                var ct = new CancellationTokenSource().Token;
                var res = await casClient.GetSupplierAsync(supplierRequest, ct);
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
                var res = await casClient.CreateSupplierAsync(supplierRequest, ct);
                return Ok(res);
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpGet("invoice")]
        public async Task<ActionResult<GetInvoiceResponse>> GetInvoice([FromBody] GetInvoiceRequest invoiceRequest)
        {
            try
            {
                var ct = new CancellationTokenSource().Token;
                var res = await casClient.GetInvoiceAsync(invoiceRequest, ct);
                return Ok(res);
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
                var res = await casClient.CreateInvoiceAsync(invoice, ct);
                return Ok(res);
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

    //public class InvoicesQuery
    //{
    //    public DateTime? StatusChangedFrom { get; set; }
    //    public DateTime? StatusChangedTo { get; set; }
    //    public string? Status { get; set; }
    //}

    //public class InvoiceQuery
    //{
    //    public string? InvoiceNumber { get; set; }
    //    public string? SupplierNumber { get; set; }
    //    public string? SupplierSiteCode { get; set; }
    //}
}
