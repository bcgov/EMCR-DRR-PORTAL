using System.Net.Mime;
using System.Security.Claims;
using AutoMapper;
using EMCR.DRR.API.Model;
using EMCR.DRR.API.Services;
using EMCR.DRR.Controllers;
using EMCR.DRR.Managers.Intake;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<ApplicationResult>> UploadAttachment([FromBody] Attachment attachment)
        {
            await Task.CompletedTask;
            return Ok(new ApplicationResult { Id = "fileId" });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicationResult>> DownloadAttachment([FromBody] Attachment attachment, string id)
        {
            await Task.CompletedTask;
            return Ok(new { Name = "FileName", Body = "base64encodedfile" });
        }

        [HttpPost("{id}")]
        public async Task<ActionResult<ApplicationResult>> UpdateAttachment([FromBody] Attachment attachment, string id)
        {
            await Task.CompletedTask;
            return Ok(new ApplicationResult { Id = "fileId" });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApplicationResult>> DeleteAttachment([FromBody] Attachment attachment, string id)
        {
            await Task.CompletedTask;
            return Ok(new ApplicationResult { Id = "fileId" });
        }
    }
}
