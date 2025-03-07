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
        public async Task<ActionResult<SubmissionResponse>> Get([FromQuery] QueryOptions? options)
        {
            try
            {
                var res = await intakeManager.Handle(new DrrApplicationsQuery { BusinessId = GetCurrentBusinessId(), QueryOptions = options });
                return Ok(new SubmissionResponse { Submissions = mapper.Map<IEnumerable<Submission>>(res.Items), Length = res.Length });
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
        public required FormType FormType { get; set; }
        public ApplicationType? ApplicationType { get; set; }
        public required string Text { get; set; }
    }

    public static class ApplicationValidators
    {
        public const int CONTACT_MAX_LENGTH = 40;
        public const int CONTACT_EMAIL_TITLE_MAX_LENGTH = 100;
        public const int CONTINGENCY_MIN_VALUE = 0;
        public const int CONTINGENCY_MAX_VALUE = 100;
        public const int ACCOUNT_MAX_LENGTH = 100;
        public const int COMMENTS_MAX_LENGTH = 2000;
        public const double FUNDING_MAX_VAL = 999999999.99;
        public const double FUNDING_MIN_VAL = -999999999.99;
    }

    public class ApplicationResult
    {
        public required string Id { get; set; }
    }

    public class QueryOptions
    {
        public int Page { get; set; } = 0;
        public int PageSize { get; set; } = 20;
        public string? OrderBy { get; set; } = "Id";
        public string? Filter { get; set; } = string.Empty;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public abstract class DraftApplication
    {
        public abstract IEnumerable<ContactDetails> AdditionalContacts { get; set; }
    }

    public class DraftEoiApplication : DraftApplication
    {
        public string? FpId { get; set; }
        public string? Id { get; set; }
        public SubmissionPortalStatus? Status { get; set; }

        //Proponent Information - 1
        public ProponentType? ProponentType { get; set; }
        public ContactDetails? Submitter { get; set; }
        [Mandatory(typeof(EoiApplication))]
        public ContactDetails? ProjectContact { get; set; }
        public override IEnumerable<ContactDetails> AdditionalContacts { get; set; }
        [CollectionStringLengthValid(ErrorMessage = "PartneringProponents have a limit of 40 characters per name")]
        public IEnumerable<string> PartneringProponents { get; set; }

        //Project Information - 2
        public FundingStream? FundingStream { get; set; }
        [Mandatory(typeof(EoiApplication))]
        public string? ProjectTitle { get; set; }
        [Mandatory(typeof(EoiApplication))]
        public ProjectType? Stream { get; set; }
        public string? ScopeStatement { get; set; }
        [Mandatory(typeof(EoiApplication))]
        public IEnumerable<Hazards>? RelatedHazards { get; set; }
        public string? OtherHazardsDescription { get; set; }
        [Mandatory(typeof(EoiApplication))]
        public DateTime? StartDate { get; set; }
        [Mandatory(typeof(EoiApplication))]
        public DateTime? EndDate { get; set; }

        //Funding Information - 3
        [Range(0, ApplicationValidators.FUNDING_MAX_VAL)]
        public decimal? EstimatedTotal { get; set; }
        [Range(0, ApplicationValidators.FUNDING_MAX_VAL)]
        public decimal? FundingRequest { get; set; }
        [Mandatory(typeof(EoiApplication))]
        public bool? HaveOtherFunding { get; set; }
        [MandatoryIf(typeof(EoiApplication), "HaveOtherFunding", true)]
        public IEnumerable<FundingInformation> OtherFunding { get; set; }
        public decimal? RemainingAmount { get; set; }
        public string? IntendToSecureFunding { get; set; }

        //Location Information - 4
        public bool? OwnershipDeclaration { get; set; }
        [MandatoryIf(typeof(EoiApplication), "OwnershipDeclaration", false)]
        public string? OwnershipDescription { get; set; }
        [Mandatory(typeof(EoiApplication))]
        public string? LocationDescription { get; set; }

        //Project Detail - 5
        public string? RationaleForFunding { get; set; }
        [Mandatory(typeof(EoiApplication))]
        public EstimatedNumberOfPeople? EstimatedPeopleImpacted { get; set; }
        [Mandatory(typeof(EoiApplication))]
        public string? CommunityImpact { get; set; }
        [Mandatory(typeof(EoiApplication))]
        public bool? IsInfrastructureImpacted { get; set; }
        [MandatoryIf(typeof(EoiApplication), "IsInfrastructureImpacted", true)]
        public IEnumerable<InfrastructureImpacted>? InfrastructureImpacted { get; set; }
        public string? DisasterRiskUnderstanding { get; set; }
        public string? AdditionalBackgroundInformation { get; set; }
        public string? AddressRisksAndHazards { get; set; }
        [Mandatory(typeof(EoiApplication))]
        public string? ProjectDescription { get; set; }
        public string? AdditionalSolutionInformation { get; set; }
        [Mandatory(typeof(EoiApplication))]
        public string? RationaleForSolution { get; set; }

        //Engagement Plan - 6
        public string? FirstNationsEngagement { get; set; }
        public string? NeighbourEngagement { get; set; }
        public string? AdditionalEngagementInformation { get; set; }

        //Other Supporting Information - 7
        public string? ClimateAdaptation { get; set; }
        public string? OtherInformation { get; set; }
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
        public string? Id { get; set; }
        public SubmissionPortalStatus? Status { get; set; }
        public ContactDetails? Submitter { get; set; }

        //Proponent & Project Information - 1
        [Mandatory(typeof(FpApplication))]
        public ContactDetails? ProjectContact { get; set; }
        [Mandatory(typeof(FpApplication))]
        public override IEnumerable<ContactDetails> AdditionalContacts { get; set; }
        [CollectionStringLengthValid(ErrorMessage = "PartneringProponents have a limit of 40 characters per name")]
        public IEnumerable<string> PartneringProponents { get; set; }
        public bool? RegionalProject { get; set; }
        [MandatoryIf(typeof(FpApplication), "RegionalProject", true)]
        public string? RegionalProjectComments { get; set; }
        [Mandatory(typeof(FpApplication))]
        public string? ProjectTitle { get; set; }
        public string? MainDeliverable { get; set; }

        //Ownership & Authorization - 2
        public bool? OwnershipDeclaration { get; set; }
        [MandatoryIf(typeof(FpApplication), "OwnershipDeclaration", false)]
        public string? OwnershipDescription { get; set; }
        public bool? HaveAuthorityToDevelop { get; set; }
        public YesNoOption? OperationAndMaintenance { get; set; }
        public string? OperationAndMaintenanceComments { get; set; }
        [Mandatory(typeof(FpApplication))]
        public YesNoOption? FirstNationsAuthorizedByPartners { get; set; }
        [Mandatory(typeof(FpApplication))]
        public YesNoOption? LocalGovernmentAuthorizedByPartners { get; set; }
        [Mandatory(typeof(FpApplication))]
        public string? AuthorizationOrEndorsementComments { get; set; }

        //Project Area - 3
        [Mandatory(typeof(FpApplication))]
        public string? LocationDescription { get; set; }
        public int? Area { get; set; }
        public AreaUnits? Units { get; set; }
        [Mandatory(typeof(FpApplication))]
        public IEnumerable<Hazards>? RelatedHazards { get; set; }
        [MandatoryIfAny(typeof(FpApplication), Values = new[] { nameof(AreaUnits.Acres), nameof(AreaUnits.Hectares), nameof(AreaUnits.SqKm) }, PropertyName = nameof(Units))]
        public string? AreaDescription { get; set; }
        public string? OtherHazardsDescription { get; set; }
        [Mandatory(typeof(FpApplication))]
        public string? CommunityImpact { get; set; }
        [Mandatory(typeof(FpApplication))]
        public EstimatedNumberOfPeopleFP? EstimatedPeopleImpactedFP { get; set; }
        [Mandatory(typeof(FpApplication))]
        public bool? IsInfrastructureImpacted { get; set; }
        [MandatoryIf(typeof(FpApplication), "IsInfrastructureImpacted", true)]
        public IEnumerable<InfrastructureImpacted>? InfrastructureImpacted { get; set; }

        //Project Plan - 4
        [Mandatory(typeof(FpApplication))]
        public DateTime? StartDate { get; set; }
        [Mandatory(typeof(FpApplication))]
        public DateTime? EndDate { get; set; }
        [Mandatory(typeof(FpApplication))]
        public string? ProjectDescription { get; set; }
        public IEnumerable<ProposedActivity>? ProposedActivities { get; set; }
        public IEnumerable<string>? FoundationalOrPreviousWorks { get; set; }
        [Mandatory(typeof(FpApplication))]
        public string? HowWasNeedIdentified { get; set; }
        public string? AddressRisksAndHazards { get; set; }
        public string? DisasterRiskUnderstanding { get; set; }
        public string? RationaleForFunding { get; set; }
        [Mandatory(typeof(FpApplication))]
        public string? ProjectAlternateOptions { get; set; }

        //Project Engagement - 5
        public bool? EngagedWithFirstNationsOccurred { get; set; }
        [Mandatory(typeof(FpApplication))]
        public string? EngagedWithFirstNationsComments { get; set; }
        public YesNoOption? OtherEngagement { get; set; }
        [MandatoryIf(typeof(FpApplication), "OtherEngagement", YesNoOption.Yes)]
        public IEnumerable<string>? AffectedParties { get; set; }
        [Mandatory(typeof(FpApplication))]
        public string? OtherEngagementComments { get; set; }
        [Mandatory(typeof(FpApplication))]
        public string? CollaborationComments { get; set; }

        //Climate Adaptation - 6
        public bool? IncorporateFutureClimateConditions { get; set; }
        public string? ClimateAdaptation { get; set; }
        public bool? ClimateAssessment { get; set; }
        [MandatoryIf(typeof(FpApplication), "ClimateAssessment", true)]
        public IEnumerable<string>? ClimateAssessmentTools { get; set; }
        [MandatoryIf(typeof(FpApplication), "ClimateAssessment", true)]
        public string? ClimateAssessmentComments { get; set; }

        //Permits Regulations & Standards - 7
        public IEnumerable<string>? Permits { get; set; }
        public YesNoOption? StandardsAcceptable { get; set; }
        public IEnumerable<StandardInfo>? Standards { get; set; }
        public string? StandardsComments { get; set; }
        [Mandatory(typeof(FpApplication))]
        public bool? ProfessionalGuidance { get; set; }
        [MandatoryIf(typeof(FpApplication), "ProfessionalGuidance", true)]
        public IEnumerable<string>? Professionals { get; set; }
        public string? ProfessionalGuidanceComments { get; set; }
        public string? KnowledgeHolders { get; set; }
        [Mandatory(typeof(FpApplication))]
        public bool? MeetsRegulatoryRequirements { get; set; }
        [MandatoryIf(typeof(FpApplication), "MeetsRegulatoryRequirements", true)]
        public string? MeetsRegulatoryComments { get; set; }
        [Mandatory(typeof(FpApplication))]
        public bool? MeetsEligibilityRequirements { get; set; }
        [MandatoryIf(typeof(FpApplication), "MeetsEligibilityRequirements", true)]
        public string? MeetsEligibilityComments { get; set; }

        //Project Outcomes - 8
        [Mandatory(typeof(FpApplication))]
        public bool? PublicBenefit { get; set; }
        [Mandatory(typeof(FpApplication))]
        public string? PublicBenefitComments { get; set; }
        [Mandatory(typeof(FpApplication))]
        public bool? FutureCostReduction { get; set; }
        [MandatoryIf(typeof(FpApplication), "FutureCostReduction", true)]
        public IEnumerable<string>? CostReductions { get; set; }
        [MandatoryIf(typeof(FpApplication), "FutureCostReduction", true)]
        public string? CostReductionComments { get; set; }
        [Mandatory(typeof(FpApplication))]
        public bool? ProduceCoBenefits { get; set; }
        [MandatoryIf(typeof(FpApplication), "ProduceCoBenefits", true)]
        public IEnumerable<string>? CoBenefits { get; set; }
        [MandatoryIf(typeof(FpApplication), "ProduceCoBenefits", true)]
        public string? CoBenefitComments { get; set; }
        [Mandatory(typeof(FpApplication))]
        public IEnumerable<string>? IncreasedResiliency { get; set; }
        [Mandatory(typeof(FpApplication))]
        public string? IncreasedResiliencyComments { get; set; }

        //Project Risks - 9
        [Mandatory(typeof(FpApplication))]
        public bool? ComplexityRiskMitigated { get; set; }
        [MandatoryIf(typeof(FpApplication), "ComplexityRiskMitigated", true)]
        public IEnumerable<string>? ComplexityRisks { get; set; }
        [MandatoryIf(typeof(FpApplication), "ComplexityRiskMitigated", true)]
        public string? ComplexityRiskComments { get; set; }
        [Mandatory(typeof(FpApplication))]
        public bool? ReadinessRiskMitigated { get; set; }
        [MandatoryIf(typeof(FpApplication), "ReadinessRiskMitigated", true)]
        public IEnumerable<string>? ReadinessRisks { get; set; }
        [MandatoryIf(typeof(FpApplication), "ReadinessRiskMitigated", true)]
        public string? ReadinessRiskComments { get; set; }
        [Mandatory(typeof(FpApplication))]
        public bool? SensitivityRiskMitigated { get; set; }
        [MandatoryIf(typeof(FpApplication), "SensitivityRiskMitigated", true)]
        public IEnumerable<string>? SensitivityRisks { get; set; }
        [MandatoryIf(typeof(FpApplication), "SensitivityRiskMitigated", true)]
        public string? SensitivityRiskComments { get; set; }
        [Mandatory(typeof(FpApplication))]
        public bool? CapacityRiskMitigated { get; set; }
        [MandatoryIf(typeof(FpApplication), "CapacityRiskMitigated", true)]
        public IEnumerable<string>? CapacityRisks { get; set; }
        [MandatoryIf(typeof(FpApplication), "CapacityRiskMitigated", true)]
        public string? CapacityRiskComments { get; set; }
        [Mandatory(typeof(FpApplication))]
        public bool? RiskTransferMigigated { get; set; }
        //[MandatoryIf(typeof(FpApplication), "RiskTransferMigigated", true)]
        public IEnumerable<IncreasedOrTransferred>? IncreasedOrTransferred { get; set; }
        [MandatoryIf(typeof(FpApplication), "RiskTransferMigigated", true)]
        public string? IncreasedOrTransferredComments { get; set; }

        //Budget - 10
        public FundingStream? FundingStream { get; set; }
        [Range(ApplicationValidators.FUNDING_MIN_VAL, ApplicationValidators.FUNDING_MAX_VAL)]
        [CurrencyNotNegativeForSubmission(typeof(FpApplication))]
        [Mandatory(typeof(FpApplication))]
        public decimal? TotalProjectCost { get; set; } //EstimatedTotal from EOI
        public string? TotalProjectCostChangeComments { get; set; }
        [Range(ApplicationValidators.FUNDING_MIN_VAL, ApplicationValidators.FUNDING_MAX_VAL)]
        [CurrencyNotNegativeForSubmission(typeof(FpApplication))]
        public decimal? EligibleFundingRequest { get; set; }
        [Range(ApplicationValidators.FUNDING_MIN_VAL, ApplicationValidators.FUNDING_MAX_VAL)]
        [CurrencyNotNegativeForSubmission(typeof(FpApplication))]
        [Mandatory(typeof(FpApplication))]
        public decimal? RemainingAmount { get; set; }
        public IEnumerable<YearOverYearFunding>? YearOverYearFunding { get; set; }
        [Range(ApplicationValidators.FUNDING_MIN_VAL, ApplicationValidators.FUNDING_MAX_VAL)]
        [CurrencyNotNegativeForSubmission(typeof(FpApplication))]
        [Mandatory(typeof(FpApplication))]
        public decimal? TotalDrifFundingRequest { get; set; }
        public string? DiscrepancyComment { get; set; }
        [Mandatory(typeof(FpApplication))]
        public bool? HaveOtherFunding { get; set; }
        [MandatoryIf(typeof(FpApplication), "HaveOtherFunding", true)]
        public IEnumerable<FundingInformation> OtherFunding { get; set; }
        public string? IntendToSecureFunding { get; set; }
        [Mandatory(typeof(FpApplication))]
        public string? CostEffectiveComments { get; set; }
        [Mandatory(typeof(FpApplication))]
        public YesNoOption? PreviousResponse { get; set; }
        [Range(ApplicationValidators.FUNDING_MIN_VAL, ApplicationValidators.FUNDING_MAX_VAL)]
        [CurrencyNotNegativeForSubmission(typeof(FpApplication))]
        [MandatoryIf(typeof(FpApplication), "PreviousResponse", YesNoOption.Yes)]
        public decimal? PreviousResponseCost { get; set; }
        [MandatoryIf(typeof(FpApplication), "PreviousResponse", YesNoOption.Yes)]
        public string? PreviousResponseComments { get; set; }
        [Mandatory(typeof(FpApplication))]
        public bool? CostConsiderationsApplied { get; set; }
        [MandatoryIf(typeof(FpApplication), "CostConsiderationsApplied", true)]
        public IEnumerable<string>? CostConsiderations { get; set; }
        [MandatoryIf(typeof(FpApplication), "CostConsiderationsApplied", true)]
        public string? CostConsiderationsComments { get; set; }

        [MandatoryIf(typeof(FpApplication), "FundingStream", Controllers.FundingStream.Stream2)]
        public CostEstimateClass? CostEstimateClass { get; set; }
        // [MandatoryIf(typeof(FpApplication), "FundingStream", Controllers.FundingStream.Stream2)]
        public IEnumerable<CostEstimate>? CostEstimates { get; set; }
        public bool? EstimatesMatchFundingRequest { get; set; }
        [Range(ApplicationValidators.CONTINGENCY_MIN_VALUE, ApplicationValidators.CONTINGENCY_MAX_VALUE)]
        public decimal? Contingency { get; set; }
        public string? ContingencyComments { get; set; }
        [Range(ApplicationValidators.FUNDING_MIN_VAL, ApplicationValidators.FUNDING_MAX_VAL)]
        [CurrencyNotNegativeForSubmission(typeof(FpApplication))]
        //For Submission - must match Updated DRIF program funding request on step 10
        public decimal? TotalEligibleCosts { get; set; }
        

        //Attachments - 11
        public bool? HaveResolution { get; set; }
        public IEnumerable<Attachment>? Attachments { get; set; }

        //Review & Declaration - 12
    }

    public class FpApplication : DraftFpApplication
    {
        public bool? AuthorizedRepresentativeStatement { get; set; }
        public bool? InformationAccuracyStatement { get; set; }
    }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public class ProposedActivity
    {
        public string? Id { get; set; }
        public ActivityType? Activity { get; set; }
        public bool? PreCreatedActivity { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Tasks { get; set; }
        public string? Deliverables { get; set; }
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

    public class CostEstimate
    {
        public string? Id { get; set; }
        public string? TaskName { get; set; }
        public CostCategory? CostCategory { get; set; }
        public string? Description { get; set; }
        public ResourceCategory? Resources { get; set; }
        public CostUnit? Units { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? UnitRate { get; set; }
        public decimal? TotalCost { get; set; }
    }

    public class InfrastructureImpacted
    {
        public string? Infrastructure { get; set; }
        public string? Impact { get; set; }
    }

    public class StandardInfo
    {
        public bool? IsCategorySelected { get; set; }
        public string? Category { get; set; }
        public IEnumerable<string>? Standards { get; set; }
    }

    public class ContactDetails
    {
        public string? Id { get; set; }
        [StringLength(ApplicationValidators.CONTACT_MAX_LENGTH)]
        public string? FirstName { get; set; }
        [StringLength(ApplicationValidators.CONTACT_MAX_LENGTH)]
        public string? LastName { get; set; }
        [StringLength(ApplicationValidators.CONTACT_EMAIL_TITLE_MAX_LENGTH)]
        public string? Title { get; set; }
        [StringLength(ApplicationValidators.CONTACT_MAX_LENGTH)]
        public string? Department { get; set; }
        //[RegularExpression("^\\d\\d\\d-\\d\\d\\d-\\d\\d\\d\\d$", ErrorMessage = "Phone number must be of the format '000-000-0000'")]
        public string? Phone { get; set; }
        [StringLength(ApplicationValidators.CONTACT_EMAIL_TITLE_MAX_LENGTH)]
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
    public enum EstimatedNumberOfPeopleFP
    {
        [Description("0 - 500")]
        ZeroToFiveHundred,

        [Description("501 - 1,000")]
        FiveHundredToOneK,

        [Description("1,001 - 5,000")]
        OneKToFiveK,

        [Description("5,001 - 10,000")]
        FiveKToTenK,

        [Description("10,001 - 50,000")]
        TenKToFiftyK,

        [Description("50,001 - 100k")]
        FiftyKToHundredK,

        [Description("100,001 +")]
        HundredKPlus,

        [Description("Unsure")]
        Unsure
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
    public enum CostCategory
    {
        [Description("Project Administration (up to 10%)")]
        ProjectAdministration,

        [Description("Project Planning")]
        ProjectPlanning,
        
        [Description("Design")]
        Design,

        [Description("Assessment")]
        Assessment,

        [Description("Mapping")]
        Mapping,

        [Description("Construction Materials/Equipment/Personnel")]
        ConstructionMaterials,

        [Description("First Nations Engagement")]
        FirstNationsEngagement,

        [Description("Neighbouring Jurisdictions and Other Impacted or Affected Parties Engagement")]
        NeighbouringJurisdictions,

        [Description("Proponent Community(ies) Engagement and Public Education")]
        ProponentCommunities,

        [Description("Incremental Staffing")]
        IncrementalStaffing,

        [Description("Short Term Interest")]
        ShortTermInterest,

        [Description("Land Acquisition/Property Purchase")]
        LandAcquisition,

        [Description("Communications")]
        Communications,

        [Description("Approvals/Permitting")]
        ApprovalsPermitting,

        [Description("Contingency")]
        Contingency,

        [Description("Other")]
        Other,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CostUnit
    {
        [Description("Hours")]
        Hours,

        [Description("Lump Sum")]
        LumpSum,

        [Description("Each")]
        Each,

        [Description("Metre")]
        Metre,

        [Description("Square Metre")]
        SquareMetre,

        [Description("Cubic Metre")]
        CubicMetre,

        [Description("Kilometer")]
        Kilometer,

        [Description("Square Kilometer")]
        SquareKilometer,

        [Description("Hectare")]
        Hectare,

        [Description("Kilogram")]
        Kilogram,

        [Description("Tonne")]
        Tonne,

        [Description("Other")]
        Other,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ResourceCategory
    {
        [Description("Project Manager")]
        ProjectManager,

        [Description("Cultural/Environmental/Indigenous Monitor")]
        CulturalMonitor,

        [Description("Elders/Traditional Knowledge Keepers")]
        Elders,

        [Description("Junior Qualified Professional")]
        JuniorQualifiedProfessional,

        [Description("Intermediate Qualified Professional")]
        IntermediateQualifiedProfessional,

        [Description("Senior Qualified Professional")]
        SeniorQualifiedProfessional,

        [Description("Principal Qualified Professional")]
        PrincipalQualifiedProfessional,

        [Description("Project Support")]
        ProjectSupport,

        [Description("Equipment")]
        Equipment,

        [Description("Other")]
        Other,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum IncreasedOrTransferred
    {
        [Description("Increased")]
        Increased,

        [Description("Transferred")]
        Transferred,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum YesNoOption
    {
        No,
        Yes,
        NotApplicable
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AreaUnits
    {
        [Description("Hectares")]
        Hectares,

        [Description("Acres")]
        Acres,

        [Description("Square Kms")]
        SqKm
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CostEstimateClass
    {
        [Description("Class A")]
        ClassA,

        [Description("Class B")]
        ClassB,
    }
}
