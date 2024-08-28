using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Security.Claims;
using System.Text.Json.Serialization;
using AutoMapper;
using EMCR.DRR.API.Model;
using EMCR.DRR.API.Services;
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
    public partial class DRIFApplicationController : ControllerBase
    {
        private readonly ILogger<DRIFApplicationController> logger;
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

        public DRIFApplicationController(ILogger<DRIFApplicationController> logger, IIntakeManager intakeManager, IMapper mapper)
        {
            this.logger = logger;
            this.intakeManager = intakeManager;
            this.mapper = mapper;
            this.errorParser = new ErrorParser();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Submission>>> Get()
        {
            try
            {
                var applications = (await intakeManager.Handle(new DrrApplicationsQuery { BusinessId = GetCurrentBusinessId() })).Items;
                return Ok(mapper.Map<IEnumerable<Submission>>(applications));
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        [HttpGet("declarations")]
        public async Task<ActionResult<DeclarationResult>> GetDeclarations()
        {
            try
            {
                var res = await intakeManager.Handle(new DeclarationQuery());
                return Ok(new DeclarationResult { Items = mapper.Map<IEnumerable<DeclarationInfo>>(res.Items) });
            }
            catch (Exception e)
            {
                return errorParser.Parse(e, logger);
            }
        }

        //Prevent empty additional contact 1, but populated additional contact 2
        private IEnumerable<ContactDetails> MapAdditionalContacts(DraftApplication application)
        {
            var additionalContact1 = application.AdditionalContacts.FirstOrDefault();
            var additionalContact2 = application.AdditionalContacts.ElementAtOrDefault(1);
            if (IsEmptyContact(additionalContact1))
            {
                additionalContact1 = additionalContact2;
                additionalContact2 = null;
            }
            return [additionalContact1 ?? new ContactDetails(), additionalContact2 ?? new ContactDetails()];
        }

        private bool IsEmptyContact(ContactDetails? contact)
        {
            if (contact == null) return true;
            if (string.IsNullOrEmpty(contact.FirstName)
                && string.IsNullOrEmpty(contact.LastName)
                && string.IsNullOrEmpty(contact.Title)
                && string.IsNullOrEmpty(contact.Department)
                && string.IsNullOrEmpty(contact.Phone)
                && string.IsNullOrEmpty(contact.Email)) return true;
            return false;
        }
    }

    public class DeclarationResult
    {
        public IEnumerable<DeclarationInfo> Items { get; set; } = Array.Empty<DeclarationInfo>();
    }

    public class DeclarationInfo
    {
        public required DeclarationType Type { get; set; }
        public required ApplicationType ApplicationType { get; set; }
        public required string Text { get; set; }
    }

    public static class ApplicationValidators
    {
        public const int CONTACT_MAX_LENGTH = 40;
        public const int ACCOUNT_MAX_LENGTH = 100;
        public const double FUNDING_MAX_VAL = 999999999.99;
    }

    public class ApplicationResult
    {
        public required string Id { get; set; }
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class DraftApplication
    {
        public string? Id { get; set; }
        public SubmissionPortalStatus? Status { get; set; }

        //Proponent Information
        public ProponentType? ProponentType { get; set; }
        public ContactDetails? Submitter { get; set; }
        public ContactDetails? ProjectContact { get; set; }
        public IEnumerable<ContactDetails> AdditionalContacts { get; set; }
        [CollectionStringLengthValid(ErrorMessage = "PartneringProponents have a limit of 40 characters per name")]
        public IEnumerable<string> PartneringProponents { get; set; }

        //Project Information
        public FundingStream? FundingStream { get; set; }
        public string? ProjectTitle { get; set; }
        public ProjectType? ProjectType { get; set; }
        public string? ScopeStatement { get; set; }
        public IEnumerable<Hazards>? RelatedHazards { get; set; }
        public string? OtherHazardsDescription { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        //Funding Information
        [Range(0, ApplicationValidators.FUNDING_MAX_VAL)]
        public decimal? EstimatedTotal { get; set; }
        [Range(0, ApplicationValidators.FUNDING_MAX_VAL)]
        public decimal? FundingRequest { get; set; }
        public bool? HaveOtherFunding { get; set; }
        public IEnumerable<FundingInformation> OtherFunding { get; set; }
        public decimal? RemainingAmount { get; set; }
        public string? IntendToSecureFunding { get; set; }

        //Location Information
        public bool? OwnershipDeclaration { get; set; }
        public string? OwnershipDescription { get; set; }
        public string? LocationDescription { get; set; }

        //Project Detail
        public string? RationaleForFunding { get; set; }
        public EstimatedNumberOfPeople? EstimatedPeopleImpacted { get; set; }
        public string? CommunityImpact { get; set; }
        public IEnumerable<string>? InfrastructureImpacted { get; set; }
        public string? DisasterRiskUnderstanding { get; set; }
        public string? AdditionalBackgroundInformation { get; set; }
        public string? AddressRisksAndHazards { get; set; }
        public string? DRIFProgramGoalAlignment { get; set; }
        public string? AdditionalSolutionInformation { get; set; }
        public string? RationaleForSolution { get; set; }

        //Engagement Plan
        public string? FirstNationsEngagement { get; set; }
        public string? NeighbourEngagement { get; set; }
        public string? AdditionalEngagementInformation { get; set; }

        //Other Supporting Information
        public string? ClimateAdaptation { get; set; }
        public string? OtherInformation { get; set; }
    }

    public class DraftEoiApplication : DraftApplication
    {
        public string? FpId { get; set; }
    }

    public class EoiApplication : DraftEoiApplication
    {
        //Declaration
        public bool? AuthorizedRepresentativeStatement { get; set; }
        public bool? FOIPPAConfirmation { get; set; }
        public bool? InformationAccuracyStatement { get; set; }
    }

    public class DraftFpApplication : DraftApplication
    {
        public string? EoiId { get; set; }

        //Proponent & Project Information - 1
        public bool? RegionalProject { get; set; }
        public string? RegionalProjectComments { get; set; }

        //Ownership & Authorization - 2
        public bool? ProjectAuthority { get; set; }
        public string? ProjectAuthorityComments { get; set; }
        public YesNoOption? OperationAndMaintenance { get; set; }
        public string? OperationAndMaintenanceComments { get; set; }
        public YesNoOption? FirstNationsEndorsement { get; set; }
        public YesNoOption? LocalGovernmentEndorsement { get; set; }
        public string? AuthorizationOrEndorsementComments { get; set; }

        //Project Area - 3

        //Project Plan - 4
        public string? ProjectDescription { get; set; }
        public IEnumerable<ProposedActivity>? ProposedActivities { get; set; }
        public IEnumerable<string>? VerificationMethods { get; set; }
        public string? VerificationMethodsComments { get; set; }
        public string? ProjectAlternateOptions { get; set; }

        //Project Engagement - 5
        public bool? EngagedWithFirstNations { get; set; }
        public string? EngagedWithFirstNationsComments { get; set; }
        public YesNoOption? OtherEngagement { get; set; }
        public IEnumerable<string>? AffectedParties { get; set; }
        public string? OtherEngagementComments { get; set; }
        public string? CollaborationComments { get; set; }

        //Climate Adaptation - 6
        public bool? ClimateAdaptationScreener { get; set; }

        //Permits Regulations & Standards - 7
        public bool? Approvals { get; set; }
        public string? ApprovalsComments { get; set; }
        public bool? ProfessionalGuidance { get; set; }
        public IEnumerable<string>? Professionals { get; set; } //Missing list in CRM
        public string? ProfessionalGuidanceComments { get; set; }
        public YesNoOption? StandardsAcceptable { get; set; }
        public IEnumerable<string>? Standards { get; set; }
        public string? StandardsComments { get; set; }
        public bool? Regulations { get; set; }
        public string? RegulationsComments { get; set; }

        //Project Outcomes - 8
        public bool? PublicBenefit { get; set; }
        public string? PublicBenefitComments { get; set; }
        public bool? FutureCostReduction { get; set; }
        public IEnumerable<string>? CostReductions { get; set; }
        public string? CostReductionComments { get; set; }
        public bool? ProduceCoBenefits { get; set; }
        public IEnumerable<string>? CoBenefits { get; set; }
        public string? CoBenefitComments { get; set; }
        public IEnumerable<string>? IncreasedResiliency { get; set; } //Missing list in CRM
        public string? IncreasedResiliencyComments { get; set; }

        //Project Risks - 9
        public bool? ComplexityRiskMitigated { get; set; }
        public IEnumerable<string>? ComplexityRisks { get; set; }
        public string? ComplexityRiskComments { get; set; }
        public bool? ReadinessRiskMitigated { get; set; }
        public IEnumerable<string>? ReadinessRisks { get; set; }
        public string? ReadinessRiskComments { get; set; }
        public bool? SensitivityRiskMitigated { get; set; }
        public IEnumerable<string>? SensitivityRisks { get; set; }
        public string? SensitivityRiskComments { get; set; }
        public bool? CapacityRiskMitigated { get; set; }
        public IEnumerable<string>? CapacityRisks { get; set; }
        public string? CapacityRiskComments { get; set; }
        public bool? RiskTransferMigigated { get; set; }
        public IEnumerable<string>? TransferRisks { get; set; } //Missing list in CRM
        public string? TransferRisksComments { get; set; }

        //Budget - 10
        public IEnumerable<YearOverYearFunding>? YearOverYearFunding { get; set; }
        [Range(0, ApplicationValidators.FUNDING_MAX_VAL)]
        public decimal? TotalDrifFundingRequest { get; set; }
        public string? DiscrepancyComment { get; set; }
        public bool? CostEffective { get; set; }
        public string? CostEffectiveComments { get; set; }
        public YesNoOption? PreviousResponse { get; set; }
        public decimal? PreviousResponseCost { get; set; }
        public string? PreviousResponseComments { get; set; }
        public string? ActivityCostEffectiveness { get; set; }
        public bool? CostConsiderationsApplied { get; set; }
        public IEnumerable<string>? CostConsiderations { get; set; }
        public string? CostConsiderationsComments { get; set; }

        //Attachments - 11
        public IEnumerable<Attachment>? Attachments { get; set; }

        //Review & Declaration - 12
    }

    public class FpApplication : DraftFpApplication
    {

    }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public class ProposedActivity
    {
        public string? Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? RelatedMilestone { get; set; }
    }

    public class YearOverYearFunding
    {
        public string? Year { get; set; }
        [Range(0, ApplicationValidators.FUNDING_MAX_VAL)]
        public decimal? Amount { get; set; }
    }

    public class FundingInformation
    {
        public string? Name { get; set; }
        public FundingType? Type { get; set; }
        [Range(0, ApplicationValidators.FUNDING_MAX_VAL)]
        public decimal? Amount { get; set; }
        public string? OtherDescription { get; set; }

    }

    public class ContactDetails
    {
        [StringLength(ApplicationValidators.CONTACT_MAX_LENGTH)]
        public string? FirstName { get; set; }
        [StringLength(ApplicationValidators.CONTACT_MAX_LENGTH)]
        public string? LastName { get; set; }
        [StringLength(ApplicationValidators.CONTACT_MAX_LENGTH)]
        public string? Title { get; set; }
        [StringLength(ApplicationValidators.CONTACT_MAX_LENGTH)]
        public string? Department { get; set; }
        //[RegularExpression("^\\d\\d\\d-\\d\\d\\d-\\d\\d\\d\\d$", ErrorMessage = "Phone number must be of the format '000-000-0000'")]
        public string? Phone { get; set; }
        [StringLength(ApplicationValidators.CONTACT_MAX_LENGTH)]
        public string? Email { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ProponentType
    {
        [Description("First Nation")]
        FirstNation,

        [Description("Local Government")]
        LocalGovernment,

        [Description("Regional District")]
        RegionalDistrict
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FundingStream
    {
        [Description("Foundational and Non-Structural")]
        Stream1,

        [Description("Structural")]
        Stream2
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EstimatedNumberOfPeople
    {
        [Description("1 - 10,000")]
        OneToTenK,

        [Description("10,001 - 50,000")]
        TenKToFiftyK,

        [Description("50,001 - 100k")]
        FiftyKToHundredK,

        [Description("100,001 +")]
        HundredKPlus,

        [Description("Unsure")]
        Unsure,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ProjectType
    {
        [Description("New Project")]
        New,

        [Description("New Phase of Existing Project")]
        Existing
    }


    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FundingType
    {
        [Description("Federal")]
        Fed,

        [Description("Federal/Provincial")]
        FedProv,

        [Description("Provincial")]
        Prov,

        [Description("Self-funded")]
        SelfFunding,

        [Description("Other Grants")]
        OtherGrants,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Hazards
    {
        [Description("Drought and water scarcity")]
        Drought,

        [Description("Extreme Temperature")]
        ExtremeTemperature,

        [Description("Flood")]
        Flood,

        [Description("Geohazards (e.g., avalanche, landslide)")]
        Geohazards,

        [Description("Sea Level Rise")]
        SeaLevelRise,

        [Description("Seismic")]
        Seismic,

        [Description("Tsunami")]
        Tsunami,

        [Description("Other")]
        Other,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum YesNoOption
    {
        No,
        Yes,
        NotApplicable
    }

#pragma warning disable CS8765 // nullability
    public class CollectionStringLengthValid : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (!(value is IList)) return false;
            foreach (string item in (IList)value)
            {
                if (item?.Length > ApplicationValidators.ACCOUNT_MAX_LENGTH) return false;
            }
            return true;
        }
    }
#pragma warning restore CS8765
}
