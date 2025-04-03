using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Security.Claims;
using System.Text.Json.Serialization;
using AutoMapper;
using EMCR.DRR.API.Model;
using EMCR.DRR.API.Services;
using EMCR.DRR.API.Utilities.Extensions;
using EMCR.DRR.Managers.Intake;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMCR.DRR.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [Authorize]
    public class ProjectController : ControllerBase
    {
        private readonly ILogger<ProjectController> logger;
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

        public ProjectController(ILogger<ProjectController> logger, IIntakeManager intakeManager, IMapper mapper)
        {
            this.logger = logger;
            this.intakeManager = intakeManager;
            this.mapper = mapper;
            this.errorParser = new ErrorParser();
        }

        [HttpGet]
        //[FromQuery] QueryOptions? options
        public async Task<ActionResult<ProjectResponse>> Get()
        {
            try
            {
                //QueryOptions = options
                var res = await intakeManager.Handle(new DrrProjectsQuery { BusinessId = GetCurrentBusinessId() });
                return Ok(new ProjectResponse { Projects = mapper.Map<IEnumerable<DraftDrrProject>>(res.Items), Length = res.Length });
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DraftDrrProject>> GetProject(string id)
        {
            try
            {
                var project = (await intakeManager.Handle(new DrrProjectsQuery { Id = id, BusinessId = GetCurrentBusinessId() })).Items.FirstOrDefault();
                if (project == null) return new NotFoundObjectResult(new ProblemDetails { Type = "NotFoundException", Title = "Not Found", Detail = "" });
                return Ok(mapper.Map<DraftDrrProject>(project));
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpPost("{id}")]
        public async Task<ActionResult<ProjectResult>> UpdateProject([FromBody] DraftDrrProject project, string id)
        {
            try
            {
                project.Id = id;

                var drr_id = await intakeManager.Handle(new SaveProjectCommand { Project = mapper.Map<DrrProject>(project), UserInfo = GetCurrentUser() });
                return Ok(new ProjectResult { Id = drr_id });
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpPost("{id}/submit")]
        public async Task<ActionResult<ProjectResult>> SubmitProject([FromBody] DrrProject project, string id)
        {
            try
            {
                project.Id = id;
                var drr_id = await intakeManager.Handle(new SubmitProjectCommand { Project = project, UserInfo = GetCurrentUser() });
                return Ok(new ProjectResult { Id = drr_id });
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpPost("{projectId}/report/validate")]
        public async Task<ActionResult<CanCreateReportResult>> ValidateCanCreateReport(string projectId, [FromBody] CreateReport createReport)
        {
            try
            {
                var res = await intakeManager.Handle(new ValidateCanCreateReportCommand { ProjectId = projectId, ReportType = (Managers.Intake.ReportType)createReport.ReportType, UserInfo = GetCurrentUser() });
                return Ok(new CanCreateReportResult { CanCreate = res.CanCreate, ReportType = createReport.ReportType, Description = res.Description });
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpPost("{projectId}/report")]
        public async Task<ActionResult<CreateReportResult>> CreateReport(string projectId, [FromBody] CreateReport createReport)
        {
            try
            {
                var id = await intakeManager.Handle(new CreateInterimReportCommand { ProjectId = projectId, ReportType = (Managers.Intake.ReportType)createReport.ReportType, UserInfo = GetCurrentUser() });
                return Ok(new CreateReportResult { Id = id });
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpGet("{projectId}/interim-reports/{reportId}")]
        public async Task<ActionResult<InterimReport>> GetInterimReport(string projectId, string reportId)
        {
            try
            {
                var report = (await intakeManager.Handle(new DrrReportsQuery { Id = reportId, BusinessId = GetCurrentBusinessId() })).Items.FirstOrDefault();
                if (report == null) return new NotFoundObjectResult(new ProblemDetails { Type = "NotFoundException", Title = "Not Found", Detail = "" });
                return Ok(mapper.Map<InterimReport>(report));
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpGet("{projectId}/interim-reports/{reportId}/claims/{claimId}")]
        public async Task<ActionResult<DraftProjectClaim>> GetClaim(string projectId, string reportId, string claimId)
        {
            try
            {
                var claim = (await intakeManager.Handle(new DrrClaimsQuery { Id = claimId, BusinessId = GetCurrentBusinessId() })).Items.FirstOrDefault();
                if (claim == null) return new NotFoundObjectResult(new ProblemDetails { Type = "NotFoundException", Title = "Not Found", Detail = "" });
                return Ok(mapper.Map<DraftProjectClaim>(claim));
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpPatch("{projectId}/interim-reports/{reportId}/claims/{claimId}")]
        public async Task<ActionResult<ProjectClaimResult>> UpdateClaim([FromBody] DraftProjectClaim claim, string claimId)
        {
            try
            {
                claim.Id = claimId;
                claim.Status = ClaimStatus.Draft;

                var drr_id = await intakeManager.Handle(new SaveClaimCommand { Claim = mapper.Map<ProjectClaim>(claim), UserInfo = GetCurrentUser() });
                return Ok(new ProjectClaimResult { Id = drr_id });
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpPatch("{projectId}/interim-reports/{reportId}/claims/{claimId}/submit")]
        public async Task<ActionResult<ProjectClaimResult>> SubmitClaim([FromBody] ProjectClaim claim, string claimId)
        {
            try
            {
                claim.Id = claimId;
                claim.Status = ClaimStatus.Draft;

                var drr_id = await intakeManager.Handle(new SubmitClaimCommand { Claim = claim, UserInfo = GetCurrentUser() });
                return Ok(new ProjectClaimResult { Id = drr_id });
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpPost("{projectId}/interim-reports/{reportId}/claims/{claimId}/invoice")]
        public async Task<ActionResult<CreateInvoiceResult>> CreateInvoice([FromBody] CreateInvoice inv, string claimId)
        {
            try
            {
                var invoiceId = inv.Id ?? Guid.NewGuid().ToString();
                var drr_id = await intakeManager.Handle(new CreateInvoiceCommand { ClaimId = claimId, InvoiceId = invoiceId, UserInfo = GetCurrentUser() });
                return Ok(new ProjectClaimResult { Id = drr_id });
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpDelete("{projectId}/interim-reports/{reportId}/claims/{claimId}/invoice")]
        public async Task<ActionResult<CreateInvoiceResult>> DeleteInvoice([FromBody] CreateInvoice inv, string claimId)
        {
            try
            {
                var invoiceId = inv.Id ?? Guid.NewGuid().ToString();
                var drr_id = await intakeManager.Handle(new DeleteInvoiceCommand { ClaimId = claimId, InvoiceId = invoiceId, UserInfo = GetCurrentUser() });
                return Ok(new ProjectClaimResult { Id = drr_id });
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpGet("{projectId}/interim-reports/{reportId}/progress-reports/{progressId}")]
        public async Task<ActionResult<DraftProgressReport>> GetProgressReport(string projectId, string reportId, string progressId)
        {
            try
            {
                var pr = (await intakeManager.Handle(new DrrProgressReportsQuery { Id = progressId, BusinessId = GetCurrentBusinessId() })).Items.FirstOrDefault();
                if (pr == null) return new NotFoundObjectResult(new ProblemDetails { Type = "NotFoundException", Title = "Not Found", Detail = "" });
                return Ok(mapper.Map<DraftProgressReport>(pr));
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpPatch("{projectId}/interim-reports/{reportId}/progress-reports/{progressId}")]
        public async Task<ActionResult<ProgressReportResult>> UpdateProgressReport([FromBody] DraftProgressReport progressReport, string progressId)
        {
            try
            {
                progressReport.Id = progressId;
                progressReport.Status = ProgressReportStatus.Draft;

                var drr_id = await intakeManager.Handle(new SaveProgressReportCommand { ProgressReport = mapper.Map<ProgressReport>(progressReport), UserInfo = GetCurrentUser() });
                return Ok(new ProgressReportResult { Id = drr_id });
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpPatch("{projectId}/interim-reports/{reportId}/progress-reports/{progressId}/submit")]
        public async Task<ActionResult<ProgressReportResult>> SubmitProgressReport([FromBody] ProgressReport progressReport, string progressId)
        {
            try
            {
                progressReport.Id = progressId;
                progressReport.Status = ProgressReportStatus.Draft; //Need to set the status after final update save

                var drr_id = await intakeManager.Handle(new SubmitProgressReportCommand { ProgressReport = mapper.Map<ProgressReport>(progressReport), UserInfo = GetCurrentUser() });
                return Ok(new ProgressReportResult { Id = drr_id });
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpGet("{projectId}/interim-reports/{reportId}/forecasts/{forecastId}")]
        public async Task<ActionResult<DraftForecast>> GetForecastReport(string projectId, string reportId, string forecastId)
        {
            try
            {
                var forecast = (await intakeManager.Handle(new DrrForecastsQuery { Id = forecastId, BusinessId = GetCurrentBusinessId() })).Items.FirstOrDefault();
                if (forecast == null) return new NotFoundObjectResult(new ProblemDetails { Type = "NotFoundException", Title = "Not Found", Detail = "" });
                return Ok(mapper.Map<DraftForecast>(forecast));
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpPatch("{projectId}/interim-reports/{reportId}/forecasts/{forecastId}")]
        public async Task<ActionResult<ProgressReportResult>> UpdateForecastReport([FromBody] DraftForecast forecast, string forecastId)
        {
            try
            {
                forecast.Id = forecastId;
                forecast.Status = ForecastStatus.Draft;

                var drr_id = await intakeManager.Handle(new SaveForecastCommand { Forecast = mapper.Map<Forecast>(forecast), UserInfo = GetCurrentUser() });
                return Ok(new ProgressReportResult { Id = drr_id });
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpPatch("{projectId}/interim-reports/{reportId}/forecasts/{forecastId}/submit")]
        public async Task<ActionResult<ProgressReportResult>> SubmitForecastReport([FromBody] Forecast forecast, string forecastId)
        {
            try
            {
                forecast.Id = forecastId;
                forecast.Status = ForecastStatus.Draft; //Need to set the status after final update save

                var drr_id = await intakeManager.Handle(new SubmitForecastCommand { Forecast = mapper.Map<Forecast>(forecast), UserInfo = GetCurrentUser() });
                return Ok(new ProgressReportResult { Id = drr_id });
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }
    }

    public static class WorkplanActivityValidators
    {
        public const int COMMENT_MAX_LENGTH = 250;
    }

    public static class ProgressReportValidators
    {
        public const int PERCENTAGE_MIN = 0;
        public const int PERCENTAGE_MAX = 100;
        public const int LONG_COMMENTS_MAX = 2000;
        public const int SHORT_COMMENTS_MAX = 50;
    }

    public class DraftDrrProject : DrrProject
    {

    }

    public class DrrProject
    {
        public string? Id { get; set; }
        public string? EoiId { get; set; }
        public string? FpId { get; set; }
        public string? ProjectTitle { get; set; }
        public string? ContractNumber { get; set; }
        public string? ProponentName { get; set; }
        public FundingStream? FundingStream { get; set; }
        public string? ProjectNumber { get; set; }
        public ProgramType? ProgramType { get; set; }
        public ReportingScheduleType? ReportingScheduleType { get; set; }
        public decimal? FundingAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ProjectStatus? Status { get; set; }
        public IEnumerable<CostProjectionItem>? CostProjections { get; set; }
        public IEnumerable<PaymentCondition>? Conditions { get; set; }
        public IEnumerable<ContactDetails>? Contacts { get; set; }
        public IEnumerable<InterimReport>? InterimReports { get; set; }
        public IEnumerable<DraftProjectClaim>? Claims { get; set; }
        public IEnumerable<DraftProgressReport>? ProgressReports { get; set; }
        public IEnumerable<DraftForecast>? Forecast { get; set; }
        public IEnumerable<ProjectEvent>? Events { get; set; }
        public IEnumerable<Attachment>? Attachments { get; set; }
    }

    public class CostProjectionItem
    {
        public string? FiscalYear { get; set; }
        public decimal? OriginalForecast { get; set; }
        public decimal? CurrentForecast { get; set; }
    }
    
    public class PaymentCondition
    {
        public string? Id { get; set; }
        public string? ConditionName { get; set; }
        public decimal? Limit { get; set; }
        public bool? ConditionMet { get; set; }
        public DateTime? DateMet { get; set; }
    }

    public class InterimReport
    {
        public string? Id { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReportDate { get; set; }
        public string? Description { get; set; }
        public InterimReportStatus? Status { get; set; }
        public InterimProjectType? ProjectType { get; set; }
        public PeriodType? PeriodType { get; set; }
        public string? ReportPeriod { get; set; }
        public DraftProjectClaim? ProjectClaim { get; set; }
        public DraftProgressReport? ProgressReport { get; set; }
        public DraftForecast? Forecast { get; set; }
    }

    public class ProgressReport : DraftProgressReport
    {
        public required bool AuthorizedRepresentativeStatement { get; set; }
        public required bool InformationAccuracyStatement { get; set; }
    }

    public class DraftProgressReport
    {
        public string? Id { get; set; }
        public string? ReportPeriod { get; set; }
        public InterimProjectType? ProjectType { get; set; }
        public DateTime? ReportDate { get; set; }
        public DateTime? DateApproved { get; set; }
        public DateTime? DateSubmitted { get; set; }
        public DateTime? DueDate { get; set; }
        public Workplan? Workplan { get; set; }
        public EventInformation? EventInformation { get; set; }
        public ProgressReportStatus? Status { get; set; }
        public IEnumerable<Attachment>? Attachments { get; set; }
        public ContactDetails? AuthorizedRepresentative { get; set; }
    }

    public class Forecast : DraftForecast
    {
        public bool? AuthorizedRepresentativeStatement { get; set; }
        public bool? InformationAccuracyStatement { get; set; }
    }

    public class DraftForecast
    {
        public string? Id { get; set; }
        public string? ReportPeriod { get; set; }
        public DateTime? ReportDate { get; set; }
        public DateTime? DateSubmitted { get; set; }
        public DateTime? DateApproved { get; set; }
        public IEnumerable<ForecastItem>? ForecastItems { get; set; }
        public decimal? Total { get; set; }
        public decimal? OriginalForecast { get; set; }
        public decimal? Variance { get; set; }
        [StringLength(ProgressReportValidators.LONG_COMMENTS_MAX)]
        public string? VarianceComment { get; set; }
        public ForecastStatus? Status { get; set; }
        public IEnumerable<Attachment>? Attachments { get; set; }
        public ContactDetails? AuthorizedRepresentative { get; set; }
    }

    public class ProjectClaim : DraftProjectClaim
    {
        public bool? AuthorizedRepresentativeStatement { get; set; }
        public bool? InformationAccuracyStatement { get; set; }
    }

    public class DraftProjectClaim
    {
        public string? Id { get; set; }
        public string? ReportPeriod { get; set; }
        public string? ContractNumber { get; set; }
        public InterimProjectType? ProjectType { get; set; }
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public DateTime? ReportDate { get; set; }
        public DateTime? DateApproved { get; set; }
        public DateTime? DateSubmitted { get; set; }
        public bool? HaveClaimExpenses {  get; set; }
        public IEnumerable<Invoice>? Invoices { get; set; }
        [StringLength(500)]
        [MandatoryIf(typeof(ProjectClaim), "HaveClaimExpenses", false)]
        public string? ClaimComment { get; set; }
        public IEnumerable<PreviousClaim>? PreviousClaims { get; set; }
        public decimal? PreviousClaimTotal { get; set; }
        public decimal? TotalClaimed { get; set; }
        public decimal? TotalProjectAmount { get; set; }
        public ActiveCondition? ActiveCondition { get; set; }
        public ContactDetails? AuthorizedRepresentative { get; set; }
        public ClaimStatus? Status { get; set; }
    }

    public class ActiveCondition
    {
        public string? ConditionName { get; set; }
        public decimal? ConditionPercentage { get; set; }
        public decimal? ConditionAmount { get; set; }
    }

    public class PreviousClaim
    {
        public CostCategory? CostCategory { get; set; }
        public decimal? TotalForProject { get; set; }
        public decimal? OriginalEstimate { get; set; }
    }

    public class Invoice
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? WorkStartDate { get; set; }
        public DateTime? WorkEndDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public CostCategory? CostCategory { get; set; }
        [StringLength(100)]
        public string? SupplierName { get; set; }
        [StringLength(250)]
        public string? Description { get; set; }
        [Range(0, ApplicationValidators.FUNDING_MAX_VAL)]
        public decimal? GrossAmount { get; set; }
        [Range(0, ApplicationValidators.FUNDING_MAX_VAL)]
        public decimal? TaxRebate { get; set; }
        [Range(0, ApplicationValidators.FUNDING_MAX_VAL)]
        public decimal? ClaimAmount { get; set; }
        [Range(0, ApplicationValidators.FUNDING_MAX_VAL)]
        public decimal? TotalPST { get; set; }
        [Range(0, ApplicationValidators.FUNDING_MAX_VAL)]
        public decimal? TotalGST { get; set; }
        public IEnumerable<Attachment>? Attachments { get; set; }
    }

    public class EventInformation
    {
        public bool? EventsOccurredSinceLastReport { get; set; }
        public IEnumerable<PastEvent>? PastEvents { get; set; }
        public bool? AnyUpcomingEvents { get; set; }
        public IEnumerable<ProjectEvent>? UpcomingEvents { get; set; }
    }

    public class PastEvent
    {
        public string? Id { get; set; }
        public string? Details { get; set; }
        public DateTime? Date { get; set; }
    }

    public class ProjectEvent
    {
        public string? Id { get; set; }
        public string? Details { get; set; }
        public DateTime? Date { get; set; }
        public ContactDetails? Contact { get; set; }
        public bool? ProvincialRepresentativeRequest { get; set; }
    }

    public class Workplan
    {
        public IEnumerable<WorkplanActivity>? WorkplanActivities { get; set; }
        [Mandatory(typeof(ProgressReport))]
        public ProjectProgressStatus? ProjectProgress { get; set; }
        [MandatoryIf(typeof(ProgressReport), "ProjectProgress", ProjectProgressStatus.AheadOfSchedule)]
        [StringLength(ProgressReportValidators.LONG_COMMENTS_MAX)]
        public string? AheadOfScheduleComments { get; set; }
        [MandatoryIf(typeof(ProgressReport), "ProjectProgress", ProjectProgressStatus.BehindSchedule)]
        public Delay? DelayReason { get; set; }
        [MandatoryIf(typeof(ProgressReport), "ProjectProgress", ProjectProgressStatus.BehindSchedule)]
        [StringLength(ProgressReportValidators.SHORT_COMMENTS_MAX)]
        public string? OtherDelayReason { get; set; }
        [MandatoryIf(typeof(ProgressReport), "ProjectProgress", ProjectProgressStatus.BehindSchedule)]
        [StringLength(ProgressReportValidators.LONG_COMMENTS_MAX)]
        public string? BehindScheduleMitigatingComments { get; set; }
        [Mandatory(typeof(ProgressReport))]
        [Range(ProgressReportValidators.PERCENTAGE_MIN, ProgressReportValidators.PERCENTAGE_MAX)]
        public decimal? ProjectCompletionPercentage { get; set; }
        [Mandatory(typeof(ProgressReport))]
        [Range(ProgressReportValidators.PERCENTAGE_MIN, ProgressReportValidators.PERCENTAGE_MAX)]
        public decimal? ConstructionCompletionPercentage { get; set; }
        [Mandatory(typeof(ProgressReport))]
        public bool? SignageRequired { get; set; }
        [StringLength(ProgressReportValidators.SHORT_COMMENTS_MAX)]
        public string? SignageNotRequiredComments { get; set; }
        [MandatoryIf(typeof(ProgressReport), "SignageRequired", true)]
        public IEnumerable<FundingSignage>? FundingSignage { get; set; }
        public bool? MediaAnnouncement { get; set; }
        public DateTime? MediaAnnouncementDate { get; set; }
        public string? MediaAnnouncementComment { get; set; }
        [Mandatory(typeof(ProgressReport))]
        public bool? OutstandingIssues { get; set; }
        [StringLength(ProgressReportValidators.LONG_COMMENTS_MAX)]
        public string? OutstandingIssuesComments { get; set; }
        [Mandatory(typeof(ProgressReport))]
        public bool? FundingSourcesChanged { get; set; }
        [StringLength(ProgressReportValidators.LONG_COMMENTS_MAX)]
        public string? FundingSourcesChangedComment { get; set; }
    }

    public class WorkplanActivity
    {
        public string? Id { get; set; }
        public ActivityType? Activity { get; set; }
        public bool? PreCreatedActivity { get; set; }
        public bool? IsMandatory { get; set; }
        [StringLength(WorkplanActivityValidators.COMMENT_MAX_LENGTH)]
        public string? Comment { get; set; }
        public WorkplanStatus? Status { get; set; }
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedCompletionDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }
    }

    public class ForecastItem
    {
        public string? Id { get; set; }
        public string? FiscalYear { get; set; }
        public decimal? ForecastAmount { get; set; }
        public decimal? TotalProjectedExpenditure { get; set; }
        public decimal? ClaimsPaidToDate { get; set; }
        public decimal? ClaimsSubmittedNotPaid { get; set; }
        public decimal? ClaimsOnThisReport { get; set; }
        public decimal? RemainingClaims { get; set; }
    }

    public class FundingSignage
    {
        public string? Id { get; set; }
        public SignageType? Type { get; set; }
        public DateTime? DateInstalled { get; set; }
        public DateTime? DateRemoved { get; set; }
        public bool? BeenApproved { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ActivityType
    {
        [Description("Administration (up to 10%)")]
        Administration,

        [Description("Approvals/Permitting")]
        ApprovalsPermitting,

        [Description("Assessment")]
        Assessment,

        [Description("Communications")]
        Communications,

        [Description("Construction")]
        Construction,

        [Description("Construction Contract Award")]
        ConstructionContractAward,

        [Description("Construction Tender")]
        ConstructionTender,

        [Description("Design")]
        Design,

        [Description("First Nations Engagement")]
        FirstNationsEngagement,

        [Description("Land Acquisition/Property Purchase")]
        LandAcquisition,

        [Description("Mapping")]
        Mapping,

        [Description("Neighbouring jurisdictions and other impacted or affected parties engagement")]
        AffectedPartiesEngagement,

        [Description("Permit to Construct")]
        PermitToConstruct,

        [Description("Project")]
        Project,

        [Description("Project Planning")]
        ProjectPlanning,

        [Description("Proponent community(ies) engagement and public education")]
        CommunityEngagement,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ProjectProgressStatus
    {
        [Description("Project is on schedule")]
        OnSchedule,

        [Description("Project is ahead of schedule")]
        AheadOfSchedule,

        [Description("Project is behind schedule")]
        BehindSchedule,

        [Description("Project is complete")]
        Complete,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Delay
    {
        [Description("Tendering")]
        Tendering,

        [Description("Referendum")]
        Referendum,

        [Description("Property Aquisition")]
        PropertyAquisition,

        [Description("Negotiations")]
        Negotiations,

        [Description("Project Implementation")]
        ProjectImplementation,

        [Description("Unforeseen Complexity")]
        UnforeseenComplexity,

        [Description("Project Scope Change")]
        ProjectScopeChange,

        [Description("Ministries/Senior Government Agencies (permitting approvals, etc.)")]
        GovernmentAgencies,

        [Description("Unforeseen Contractor Delays")]
        UnforeseenContractorDelays,

        [Description("Weather")]
        Weather,

        [Description("Change in Project Manager or Project Oversight Committee")]
        ChangeProjectManager,

        [Description("Other")]
        Other,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SignageType
    {
        [Description("Temporary physical signage")]
        Temporary,

        [Description("Digital signage")]
        Digital,

        [Description("Permanent plaque")]
        Plaque,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ProjectStatus
    {
        [Description("Not Started")]
        NotStarted,

        [Description("In Progress")]
        InProgress,

        [Description("Complete")]
        Complete,

        [Description("Inactive")]
        Inactive
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ReportingScheduleType
    {
        [Description("Quarterly")]
        Quarterly,

        [Description("Monthly")]
        Monthly
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PaymentConditionStatus
    {
        [Description("Met")]
        Met,

        [Description("NotMet")]
        NotMet
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ProvincialMedia
    {
        [Description("Not Announced")]
        NotAnnounced,

        [Description("Not Applicable")]
        NotApplicable
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WorkplanStatus
    {
        [Description("Not Started")]
        NotStarted,

        [Description("In Progress")]
        InProgress,

        [Description("Completed")]
        Completed,

        [Description("Awarded")]
        Awarded,

        [Description("Not Awarded")]
        NotAwarded,

        [Description("No Longer Needed")]
        NoLongerNeeded
    }

    public enum EventType
    {
        [Description("Ground Breaking")]
        GroundBreaking,

        [Description("Ribbon Cutting/Opening")]
        RibbonCuttingOpening,

        [Description("Community Engagement")]
        CommunityEngagement,

        [Description("Other")]
        Other,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EventStatus
    {
        [Description("Not Planned")]
        NotPlanned,

        [Description("Planned, Date Unknown")]
        PlannedDateUnknown,

        [Description("Planned, Date Known")]
        PlannedDateKnown,

        [Description("Already Occurred")]
        AlreadyOccurred,

        [Description("Unknown")]
        Unknown,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum InterimReportStatus
    {
        [Description("Not Started")]
        NotStarted,

        [Description("In Progress")]
        InProgress,

        [Description("Approved")]
        Approved,

        [Description("Skipped")]
        Skipped,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum InterimProjectType
    {
        [Description("Foundational and Non-Structural")]
        Stream1,

        [Description("Structural")]
        Stream2
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PeriodType
    {
        [Description("Off Cycle")]
        OffCycle,

        [Description("Final")]
        Final,

        [Description("Interim")]
        Interim,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ClaimStatus
    {
        [Description("NotStarted")]
        NotStarted,

        [Description("Draft")]
        Draft,

        [Description("Submitted")]
        Submitted,

        [Description("Update Needed")]
        UpdateNeeded,

        [Description("Approved")]
        Approved,

        [Description("Skipped")]
        Skipped,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ProgressReportStatus
    {
        [Description("NotStarted")]
        NotStarted,

        [Description("Draft")]
        Draft,

        [Description("Submitted")]
        Submitted,

        [Description("Update Needed")]
        UpdateNeeded,

        [Description("Approved")]
        Approved,

        [Description("Skipped")]
        Skipped,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ForecastStatus
    {
        [Description("NotStarted")]
        NotStarted,

        [Description("Draft")]
        Draft,

        [Description("Submitted")]
        Submitted,

        [Description("Update Needed")]
        UpdateNeeded,

        [Description("Approved")]
        Approved,

        [Description("Skipped")]
        Skipped,
    }

    public class ProjectResult
    {
        public required string Id { get; set; }
    }

    public class ProjectResponse
    {
        public IEnumerable<DraftDrrProject> Projects { get; set; } = Array.Empty<DraftDrrProject>();
        public int Length { get; set; }
    }

    public class ReportResult
    {
        public required string Id { get; set; }
    }

    public class ProgressReportResult : ReportResult
    {
    }

    public class ProjectClaimResult : ReportResult
    {
    }

    public class CreateReportResult : ReportResult
    {
    }

    public class CreateInvoiceResult : ReportResult
    {
    }

    public class DeleteInvoiceResult : ReportResult
    {
    }

    public class CanCreateReportResult
    {
        public required bool CanCreate { get; set; }
        public required ReportType ReportType { get; set; }
        public string? Description { get; set; }
    }

    public class CreateReport
    {
        public required ReportType ReportType { get; set; }
    }

    public class CreateInvoice
    {
        public string? Id { get; set; }
    }

    public class DeleteInvoice
    {
        public string? Id { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ReportType
    {
        [Description("Off Cycle Report")]
        OffCycle,
        [Description("Interim Report")]
        Interim,
        [Description("Final Report")]
        Final
    }
}
