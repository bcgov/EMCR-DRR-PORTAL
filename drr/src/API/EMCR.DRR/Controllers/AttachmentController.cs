using System.Globalization;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using EMCR.DRR.API.Model;
using EMCR.DRR.API.Services;
using EMCR.DRR.API.Services.S3;
using EMCR.DRR.Controllers;
using EMCR.DRR.Managers.Intake;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EMCR.DRR.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [Authorize]
    public class AttachmentController : ControllerBase
    {
        private readonly ILogger<AttachmentController> logger;
        private readonly IIntakeManager intakeManager;
        private readonly IMapper mapper;
        private readonly ErrorParser errorParser;

#pragma warning disable CS8603 // Possible null reference return.
        private string GetCurrentBusinessId() => User.FindFirstValue("bceid_business_guid");
        private string GetCurrentBusinessName() => User.FindFirstValue("bceid_business_name");
        private string GetCurrentUserId() => User.FindFirstValue("bceid_user_guid");
        private UserInfo GetCurrentUser()
        {
            return new UserInfo { BusinessId = GetCurrentBusinessId(), BusinessName = GetCurrentBusinessName(), UserId = GetCurrentUserId() };
        }
#pragma warning restore CS8603 // Possible null reference return.

        public AttachmentController(ILogger<AttachmentController> logger, IIntakeManager intakeManager, IMapper mapper)
        {
            this.logger = logger;
            this.intakeManager = intakeManager;
            this.mapper = mapper;
            this.errorParser = new ErrorParser();
        }

        [HttpPost]
        [RequestSizeLimit(100_000_000)] // 100MB
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAttachment([FromForm] FileUploadModel attachment)
        {
            if (attachment.File == null || attachment.File.Length == 0)
                return BadRequest("No file uploaded.");

            //if (attachment.File.Length >= 51 * 1024 * 1024)
            //    throw new ContentTooLargeException("File size exceeds 50MB limit");

            var attachmentInfo = mapper.Map<AttachmentInfo>(attachment);
            var ret = await intakeManager.Handle(new UploadAttachmentCommand { AttachmentInfo = attachmentInfo, UserInfo = GetCurrentUser() });
            return Ok(new { Message = "File uploaded successfully." });
        }

#pragma warning disable ASP0019 // Suggest using IHeaderDictionary.Append or the indexer
        [HttpGet("{id}")]
        public async Task<IActionResult> DownloadAttachment(string id)
        {
            var file = (FileQueryResult)await intakeManager.Handle(new DownloadAttachment { Id = id, UserInfo = GetCurrentUser() });
            var fileName = file.File.FileName;
            // Encode filename per RFC 5987
            var encodedFileName = Uri.EscapeDataString(fileName);
            var fallbackName = GenerateSafeAsciiFileName(fileName);
            var contentDisposition = $"attachment; filename=\"{fallbackName}\"; filename*=UTF-8''{encodedFileName}";

            HttpContext.Response.Headers.Add("Content-Disposition", contentDisposition);

            return new FileStreamResult(file.File.ContentStream, file.File.ContentType);
        }
#pragma warning restore ASP0019 // Suggest using IHeaderDictionary.Append or the indexer

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApplicationResult>> DeleteAttachment([FromBody] DeleteAttachment attachment, string id)
        {
            await intakeManager.Handle(new DeleteAttachmentCommand { Id = id, UserInfo = GetCurrentUser() });
            return Ok(new ApplicationResult { Id = id });
        }

        private static string GenerateSafeAsciiFileName(string fileName)
        {
            // Normalize to decompose accents (é -> e + ´)
            string normalized = fileName.Normalize(NormalizationForm.FormD);

            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark && c < 128)
                {
                    sb.Append(c);
                }
            }

            // Replace unsafe characters (spaces, symbols, etc.)
            string cleaned = sb.ToString();
            //cleaned = Regex.Replace(cleaned, @"[^a-zA-Z0-9_\.-]", "_");

            return cleaned.Trim();
        }
    }

    public class AttachmentQueryResult
    {
        public required S3FileStreamResult File { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
    {
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            var factories = context.ValueProviderFactories;
            factories.RemoveType<FormValueProviderFactory>();
            factories.RemoveType<FormFileValueProviderFactory>();
            factories.RemoveType<JQueryFormValueProviderFactory>();
        }
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class MultipartFormDataAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            if (request.HasFormContentType
                && request.ContentType.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            context.Result = new StatusCodeResult(StatusCodes.Status415UnsupportedMediaType);
        }
    }
}
