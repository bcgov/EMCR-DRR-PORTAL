using EMCR.DRR.API.Model;
using EMCR.DRR.Managers.Intake;
using Microsoft.AspNetCore.Mvc;

namespace EMCR.DRR.Controllers
{
    public partial class DRIFApplicationController
    {
        [HttpGet("eoi/{id}")]
        public async Task<ActionResult<DraftEoiApplication>> GetEOI(string id)
        {
            try
            {
                var application = (await intakeManager.Handle(new DrrApplicationsQuery { Id = id, BusinessId = GetCurrentBusinessId() })).Items.FirstOrDefault();
                if (application == null) return new NotFoundObjectResult(new ProblemDetails { Type = "NotFoundException", Title = "Not Found", Detail = "" });
                return Ok(mapper.Map<DraftEoiApplication>(application));
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpPost("eoi")]
        public async Task<ActionResult<ApplicationResult>> CreateEOIApplication(DraftEoiApplication application)
        {
            try
            {
                application.Status = SubmissionPortalStatus.Draft;
                application.AdditionalContacts = MapAdditionalContacts(application);

                var id = await intakeManager.Handle(new EoiSaveApplicationCommand { application = mapper.Map<EoiApplication>(application), UserInfo = GetCurrentUser() });
                return Ok(new ApplicationResult { Id = id });
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpPost("eoi/{id}")]
        public async Task<ActionResult<ApplicationResult>> UpdateApplication([FromBody] DraftEoiApplication application, string id)
        {
            try
            {
                application.Id = id;
                application.Status = SubmissionPortalStatus.Draft;
                application.AdditionalContacts = MapAdditionalContacts(application);

                var drr_id = await intakeManager.Handle(new EoiSaveApplicationCommand { application = mapper.Map<EoiApplication>(application), UserInfo = GetCurrentUser() });
                return Ok(new ApplicationResult { Id = drr_id });
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpPost("eoi/submit")]
        public async Task<ActionResult<ApplicationResult>> SubmitApplication([FromBody] EoiApplication application)
        {
            try
            {
                application.Status = SubmissionPortalStatus.UnderReview;
                application.AdditionalContacts = MapAdditionalContacts(application);

                var drr_id = await intakeManager.Handle(new EoiSubmitApplicationCommand { application = application, UserInfo = GetCurrentUser() });
                return Ok(new ApplicationResult { Id = drr_id });
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpPost("eoi/{id}/submit")]
        public async Task<ActionResult<ApplicationResult>> SubmitApplication([FromBody] EoiApplication application, string id)
        {
            try
            {
                application.Id = id;
                application.Status = SubmissionPortalStatus.UnderReview;
                application.AdditionalContacts = MapAdditionalContacts(application);

                var drr_id = await intakeManager.Handle(new EoiSubmitApplicationCommand { application = application, UserInfo = GetCurrentUser() });
                return Ok(new ApplicationResult { Id = drr_id });
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }
    }
}
