﻿using System.Net.Mime;
using System.Security.Claims;
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

        //--------TEST Upload stream for larger file sizes - may get removed
        //        [HttpPost("{id}/upload-stream-multipartreader")]
        //        [ProducesResponseType(StatusCodes.Status201Created)]
        //        [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
        //        [MultipartFormData]
        //        [DisableFormValueModelBinding]
        //        public async Task<IActionResult> Upload(string id)
        //        {
        //#pragma warning disable CS8601 // Possible null reference assignment.
        //            var attachmentInfo = new AttachmentInfoStream
        //            {
        //                ApplicationId = id,
        //                FileStream = new S3FileStream
        //                {
        //                    ContentType = Request.ContentType,
        //                    FileContentStream = HttpContext.Request.Body,
        //                    FileName = "test"
        //                }
        //            };
        //#pragma warning restore CS8601 // Possible null reference assignment.
        //            var ret = await intakeManager.Handle(new UploadAttachmentStreamCommand { AttachmentInfo = attachmentInfo, UserInfo = GetCurrentUser() });
        //            return Ok(new ApplicationResult { Id = ret });
        //        }

        [HttpPost]
        [RequestSizeLimit(75_000_000)] //Payload is larger than the actualy content length - this is large enough to handle 50MB file
        public async Task<ActionResult<ApplicationResult>> UploadAttachment([FromBody] FileData attachment)
        {
            try
            {
                var contentSize = attachment.Content.Length;
                if (contentSize >= (51 * 1024 * 1024))
                {
                    throw new ContentTooLargeException("File size exceeds 50MB limit");
                }
                var attachmentInfo = mapper.Map<AttachmentInfo>(attachment);
                var ret = await intakeManager.Handle(new UploadAttachmentCommand { AttachmentInfo = attachmentInfo, UserInfo = GetCurrentUser() });
                return Ok(new ApplicationResult { Id = ret });
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AttachmentQueryResult>> DownloadAttachment(string id)
        {
            var file = (FileQueryResult)await intakeManager.Handle(new DownloadAttachment { Id = id, UserInfo = GetCurrentUser() });
            return Ok(new AttachmentQueryResult { File = file.File });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApplicationResult>> DeleteAttachment([FromBody] DeleteAttachment attachment, string id)
        {
            await intakeManager.Handle(new DeleteAttachmentCommand { Id = id, UserInfo = GetCurrentUser() });
            return Ok(new ApplicationResult { Id = id });
        }
    }

    public class AttachmentQueryResult
    {
        public required S3File File { get; set; }
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
