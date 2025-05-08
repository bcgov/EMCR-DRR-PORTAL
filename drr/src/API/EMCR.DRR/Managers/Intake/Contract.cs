using System.ComponentModel;
using EMCR.DRR.API.Model;
using EMCR.DRR.API.Services.S3;
using EMCR.DRR.Controllers;

namespace EMCR.DRR.Managers.Intake
{
    public interface IIntakeManager
    {
        Task<DeclarationQueryResult> Handle(DeclarationQuery query);
        Task<EntitiesQueryResult> Handle(EntitiesQuery query);
        Task<string> Handle(IntakeCommand cmd);
        Task<ApplicationQueryResponse> Handle(ApplicationQuery query);
        Task<ProjectsQueryResponse> Handle(ProjectQuery query);
        Task<ReportsQueryResponse> Handle(ReportQuery query);
        Task<ClaimsQueryResponse> Handle(ClaimQuery query);
        Task<ProgressReportsQueryResponse> Handle(ProgressReportQuery query);
        Task<ForecastsQueryResponse> Handle(ForecastQuery query);
        Task<ConditionsQueryResponse> Handle(ConditionRequestQuery query);
        Task<StorageQueryResults> Handle(AttachmentQuery query);
        Task<ValidateCanCreateReportResult> Handle(ValidateCanCreateReportCommand cmd);
    }

    public class DeclarationQuery
    { }

    public class DeclarationQueryResult
    {
        public IEnumerable<DeclarationInfo> Items { get; set; } = Array.Empty<DeclarationInfo>();
    }

    public class EntitiesQuery
    { }

    public class EntitiesQueryResult
    {
        public IEnumerable<string>? FoundationalOrPreviousWorks { get; set; } = Array.Empty<string>(); //In CRM = Project Need Identifications
        public IEnumerable<string>? AffectedParties { get; set; } = Array.Empty<string>();
        public IEnumerable<Controllers.StandardInfo>? Standards { get; set; } = Array.Empty<Controllers.StandardInfo>();
        public IEnumerable<string>? CostReductions { get; set; } = Array.Empty<string>();
        public IEnumerable<string>? CoBenefits { get; set; } = Array.Empty<string>();
        public IEnumerable<string>? ComplexityRisks { get; set; } = Array.Empty<string>();
        public IEnumerable<string>? ReadinessRisks { get; set; } = Array.Empty<string>();
        public IEnumerable<string>? SensitivityRisks { get; set; } = Array.Empty<string>();
        public IEnumerable<string>? CostConsiderations { get; set; } = Array.Empty<string>();
        public IEnumerable<string>? CapacityRisks { get; set; } = Array.Empty<string>();
        public IEnumerable<string>? FiscalYears { get; set; } = Array.Empty<string>();
        public IEnumerable<string>? Professionals { get; set; } = Array.Empty<string>();
        public IEnumerable<string>? IncreasedResiliency { get; set; } = Array.Empty<string>();
        public IEnumerable<string>? ClimateAssessmentToolOptions { get; set; } = Array.Empty<string>();
        public IEnumerable<string>? ProjectActivities { get; set; } = Array.Empty<string>();
    }

    public class DeclarationInfo
    {
        public required DeclarationType Type { get; set; }
        public string? ApplicationTypeName { get; set; }
        public required FormType FormType { get; set; }
        public required string Text { get; set; }
    }

    public enum DeclarationType
    {
        AuthorizedRepresentative,
        AccuracyOfInformation
    }

    public enum FormType
    {
        Application,
        Report
    }

    public class ApplicationQueryResponse
    {
        public required IEnumerable<Application> Items { get; set; }
        public int Length { get; set; }
    }

    public class DrrApplicationsQuery : ApplicationQuery
    {
        public string? Id { get; set; }
        public string? BusinessId { get; set; }
        public QueryOptions? QueryOptions { get; set; }
    }

    public abstract class ApplicationQuery
    { }

    public class ProjectsQueryResponse
    {
        public required IEnumerable<Project> Items { get; set; }
        public int Length { get; set; }
    }

    public class DrrProjectsQuery : ProjectQuery
    {
        public string? Id { get; set; }
        public string? BusinessId { get; set; }
        public QueryOptions? QueryOptions { get; set; }
    }

    public abstract class ProjectQuery
    { }

    public class ReportsQueryResponse
    {
        public required IEnumerable<InterimReportDetails> Items { get; set; }
        public int Length { get; set; }
    }

    public class DrrReportsQuery : ReportQuery
    {
        public string? Id { get; set; }
        public string? BusinessId { get; set; }
    }

    public abstract class ReportQuery
    { }

    public class ClaimsQueryResponse
    {
        public required IEnumerable<ClaimDetails> Items { get; set; }
        public int Length { get; set; }
    }

    public class DrrClaimsQuery : ClaimQuery
    {
        public string? Id { get; set; }
        public string? BusinessId { get; set; }
    }

    public abstract class ClaimQuery
    { }

    public class ProgressReportsQueryResponse
    {
        public required IEnumerable<ProgressReportDetails> Items { get; set; }
        public int Length { get; set; }
    }

    public class DrrProgressReportsQuery : ProgressReportQuery
    {
        public string? Id { get; set; }
        public string? BusinessId { get; set; }
    }

    public abstract class ProgressReportQuery
    { }

    public class ForecastsQueryResponse
    {
        public required IEnumerable<ForecastDetails> Items { get; set; }
        public int Length { get; set; }
    }

    public class DrrForecastsQuery : ForecastQuery
    {
        public string? Id { get; set; }
        public string? BusinessId { get; set; }
    }

    public abstract class ForecastQuery
    { }

    public class ConditionsQueryResponse
    {
        public required IEnumerable<ConditionRequest> Items { get; set; }
        public int Length { get; set; }
    }

    public class ConditionRequestQuery : ConditionQuery
    {
        public string? Id { get; set; }
        public string? ConditionId { get; set; }
        public string? ProjectId { get; set; }
        public string? BusinessId { get; set; }
    }

    public abstract class ConditionQuery
    { }

    public abstract class IntakeCommand
    { }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class EoiSaveApplicationCommand : IntakeCommand
    {
        public EoiApplication Application { get; set; } = null!;
        public UserInfo UserInfo { get; set; }
    }

    public class EoiSubmitApplicationCommand : IntakeCommand
    {
        public EoiApplication Application { get; set; } = null!;
        public UserInfo UserInfo { get; set; }
    }

    public class CreateFpFromEoiCommand : IntakeCommand
    {
        public required string EoiId { get; set; }
        public UserInfo UserInfo { get; set; }
        public required ScreenerQuestions ScreenerQuestions { get; set; }
    }

    public class FpSaveApplicationCommand : IntakeCommand
    {
        public FpApplication Application { get; set; } = null!;
        public UserInfo UserInfo { get; set; }
    }

    public class FpSubmitApplicationCommand : IntakeCommand
    {
        public FpApplication Application { get; set; } = null!;
        public UserInfo UserInfo { get; set; }
    }

    public class WithdrawApplicationCommand : IntakeCommand
    {
        public string Id { get; set; }
        public UserInfo UserInfo { get; set; }
    }

    public class DeleteApplicationCommand : IntakeCommand
    {
        public string Id { get; set; }
        public UserInfo UserInfo { get; set; }
    }

    public class UploadAttachmentCommand : IntakeCommand
    {
        public AttachmentInfo AttachmentInfo { get; set; }
        public UserInfo UserInfo { get; set; }
    }

    public class DeleteAttachmentCommand : IntakeCommand
    {
        public required string Id { get; set; }
        public UserInfo UserInfo { get; set; }
    }

    public class SaveProjectCommand : IntakeCommand
    {
        public DrrProject Project { get; set; } = null!;
        public UserInfo UserInfo { get; set; }
    }

    public class SubmitProjectCommand : IntakeCommand
    {
        public DrrProject Project { get; set; } = null!;
        public UserInfo UserInfo { get; set; }
    }

    public class SaveProgressReportCommand : IntakeCommand
    {
        public Controllers.ProgressReport ProgressReport { get; set; } = null!;
        public UserInfo UserInfo { get; set; }
    }

    public class SubmitProgressReportCommand : IntakeCommand
    {
        public Controllers.ProgressReport ProgressReport { get; set; } = null!;
        public UserInfo UserInfo { get; set; }
    }

    public class SaveClaimCommand : IntakeCommand
    {
        public Controllers.ProjectClaim Claim { get; set; } = null!;
        public UserInfo UserInfo { get; set; }
    }

    public class SubmitClaimCommand : IntakeCommand
    {
        public Controllers.ProjectClaim Claim { get; set; } = null!;
        public UserInfo UserInfo { get; set; }
    }

    public class SaveForecastCommand : IntakeCommand
    {
        public Controllers.Forecast Forecast { get; set; } = null!;
        public UserInfo UserInfo { get; set; }
    }

    public class SubmitForecastCommand : IntakeCommand
    {
        public Controllers.Forecast Forecast { get; set; } = null!;
        public UserInfo UserInfo { get; set; }
    }

    public class CreateInvoiceCommand : IntakeCommand
    {
        public required string ClaimId { get; set; }
        public required string InvoiceId { get; set; }
        public UserInfo UserInfo { get; set; }
    }

    public class DeleteInvoiceCommand : IntakeCommand
    {
        public required string ClaimId { get; set; }
        public required string InvoiceId { get; set; }
        public UserInfo UserInfo { get; set; }
    }

    public class CreateInterimReportCommand : IntakeCommand
    {
        public string ProjectId { get; set; } = null!;
        public ReportType ReportType { get; set; }
        public UserInfo UserInfo { get; set; }
    }

    public class CreateConditionRequestCommand : IntakeCommand
    {
        public required string ConditionId { get; set; }
        public required string ProjectId { get; set; }
        public UserInfo UserInfo { get; set; }
    }

    public class SaveConditionRequestCommand : IntakeCommand
    {
        public Controllers.ConditionRequest Request { get; set; } = null!;
        public UserInfo UserInfo { get; set; }
    }
    
    public class SubmitConditionRequestCommand : IntakeCommand
    {
        public Controllers.ConditionRequest Request { get; set; } = null!;
        public UserInfo UserInfo { get; set; }
    }

    public class ValidateCanCreateReportCommand
    {
        public string ProjectId { get; set; } = null!;
        public ReportType ReportType { get; set; }
        public UserInfo UserInfo { get; set; }
    }

    public class ValidateCanCreateReportResult
    {
        public bool CanCreate { get; set; }
        public string Description { get; set; }
    }

    public abstract class AttachmentQuery
    { }

    public class DownloadAttachment : AttachmentQuery
    {
        public required string Id { get; set; }
        public UserInfo UserInfo { get; set; }
    }

    public class AttachmentInfo
    {
        public string? Id { get; set; }
        public required string RecordId { get; set; }
        public required RecordType RecordType { get; set; }
        public required S3FileStream FileStream { get; set; }
        public DocumentType DocumentType { get; set; } = DocumentType.OtherSupportingDocument;
    }

    public enum RecordType
    {
        [Description("drr_application")]
        FullProposal,
        [Description("drr_projectprogress")]
        ProgressReport,
        [Description("drr_projectexpenditure")]
        Invoice,
        [Description("drr_projectbudgetforecast")]
        ForecastReport,
        [Description("drr_request")]
        ConditionRequest,
        None,
    }

    public enum DocumentType
    {
        [Description("Approval Letter")]
        ApprovalLetter,

        [Description("Condition Approval")]
        ConditionApproval,

        [Description("Council/Board Resolution")]
        CouncilBoardResolution,

        [Description("Forecast Report")]
        ForecastReport,

        [Description("Invoice")]
        Invoice,

        [Description("Other Supporting Documentation")]
        OtherSupportingDocument,

        [Description("Preliminary Design")]
        PreliminaryDesign,

        [Description("Progress Report")]
        ProgressReport,

        [Description("Project Completion Letter")]
        ProjectCompletionLetter,

        [Description("Proof of Payment")]
        ProofOfPayment,

        [Description("SCA")]
        SCA,

        [Description("Site Plan")]
        SitePlan,
    }

    public class Application
    {
        public string? CrmId { get; set; }
        public string? Id { get; set; }
        public string? FpId { get; set; }
        public string? EoiId { get; set; }
        public required string ApplicationTypeName { get; set; }
        public required string ProgramName { get; set; }
        public string? BCeIDBusinessId { get; set; }
        //Proponent Information - 1
        public ProponentType? ProponentType { get; set; }
        public string? ProponentName { get; set; }
        public ContactDetails? AuthorizedRepresentative { get; set; }
        public ContactDetails? ProjectContact { get; set; }
        public ContactDetails? AdditionalContact1 { get; set; }
        public ContactDetails? AdditionalContact2 { get; set; }
        public IEnumerable<PartneringProponent> PartneringProponents { get; set; }

        //Project Information - 2
        public FundingStream? FundingStream { get; set; }
        public string? ProjectTitle { get; set; }
        public ProjectType? ProjectType { get; set; }
        public string? ScopeStatement { get; set; }
        public IEnumerable<Hazards> RelatedHazards { get; set; }
        public string? OtherHazardsDescription { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        //Funding Information - 3
        public decimal? EstimatedTotalFromEOI { get; set; } //For FP this is OriginalTotalProjectCost
        public decimal? EstimatedTotal { get; set; } //TotalProjectCost on FP
        public decimal? FundingRequest { get; set; }
        public bool? HaveOtherFunding { get; set; }
        public IEnumerable<FundingInformation> OtherFunding { get; set; }
        public decimal? RemainingAmount { get; set; }
        public string? IntendToSecureFunding { get; set; }

        //Location Information - 4
        public bool? OwnershipDeclaration { get; set; }
        public string? OwnershipDescription { get; set; }
        public string? LocationDescription { get; set; }

        //Project Detail - 5
        public string? RationaleForFunding { get; set; }
        public EstimatedNumberOfPeople? EstimatedPeopleImpacted { get; set; }
        public string? CommunityImpact { get; set; }
        public bool? IsInfrastructureImpacted { get; set; }
        public IEnumerable<CriticalInfrastructure> InfrastructureImpacted { get; set; }
        public string? DisasterRiskUnderstanding { get; set; }
        public string? AdditionalBackgroundInformation { get; set; }
        public string? AddressRisksAndHazards { get; set; }
        public string? ProjectDescription { get; set; }
        public string? AdditionalSolutionInformation { get; set; }
        public string? RationaleForSolution { get; set; }

        //Engagement Plan - 6
        public string? FirstNationsEngagement { get; set; }
        public string? NeighbourEngagement { get; set; }
        public string? AdditionalEngagementInformation { get; set; }

        //Other Supporting Information - 7
        public string? ClimateAdaptation { get; set; }
        public string? OtherInformation { get; set; }


        //Declaration - 8
        public bool? AuthorizedRepresentativeStatement { get; set; }
        public bool? FOIPPAConfirmation { get; set; }
        public bool? InformationAccuracyStatement { get; set; }

        public ApplicationStatus Status { get; set; }
        public DateTime? SubmittedDate { get; set; } = null;
        public DateTime? ModifiedOn { get; set; } = null;


        //--------------Full Proposal Only Fields--------------
        //Proponent & Project Information - 1
        public bool? RegionalProject { get; set; }
        public string? RegionalProjectComments { get; set; }
        public string? MainDeliverable { get; set; }

        //Ownership & Authorization - 2
        public bool? HaveAuthorityToDevelop { get; set; }
        public YesNoOption? OperationAndMaintenance { get; set; }
        public string? OperationAndMaintenanceComments { get; set; }
        public YesNoOption? FirstNationsAuthorizedByPartners { get; set; }
        public YesNoOption? LocalGovernmentAuthorizedByPartners { get; set; }
        public string? AuthorizationOrEndorsementComments { get; set; }

        //Project Area - 3
        public int? Area { get; set; }
        public AreaUnits? Units { get; set; }
        public string? AreaDescription { get; set; }
        public EstimatedNumberOfPeopleFP? EstimatedPeopleImpactedFP { get; set; }

        //Project Plan - 4
        public IEnumerable<ProposedActivity>? ProposedActivities { get; set; }
        public IEnumerable<FoundationalOrPreviousWork> FoundationalOrPreviousWorks { get; set; }
        public string? HowWasNeedIdentified { get; set; }
        public string? ProjectAlternateOptions { get; set; }

        //Project Engagement - 5
        public bool? EngagedWithFirstNationsOccurred { get; set; }
        public string? EngagedWithFirstNationsComments { get; set; }
        public YesNoOption? OtherEngagement { get; set; }
        public IEnumerable<AffectedParty> AffectedParties { get; set; }
        public string? OtherEngagementComments { get; set; }
        public string? CollaborationComments { get; set; }

        //Climate Adaptation - 6
        public bool? IncorporateFutureClimateConditions { get; set; }
        public bool? ClimateAssessment { get; set; }
        public IEnumerable<ClimateAssessmentToolsInfo> ClimateAssessmentTools { get; set; }
        public string? ClimateAssessmentComments { get; set; }

        //Permits Regulations & Standards - 7
        public IEnumerable<Permit> Permits { get; set; }
        public YesNoOption? StandardsAcceptable { get; set; }
        public IEnumerable<StandardInfo> Standards { get; set; }
        public string? StandardsComments { get; set; }
        public bool? ProfessionalGuidance { get; set; }
        public IEnumerable<ProfessionalInfo> Professionals { get; set; } //Missing list in CRM
        public string? ProfessionalGuidanceComments { get; set; }
        public string? KnowledgeHolders { get; set; }
        public bool? MeetsRegulatoryRequirements { get; set; }
        public string? MeetsRegulatoryComments { get; set; }
        public bool? MeetsEligibilityRequirements { get; set; }
        public string? MeetsEligibilityComments { get; set; }

        //Project Outcomes - 8
        public bool? PublicBenefit { get; set; }
        public string? PublicBenefitComments { get; set; }
        public bool? FutureCostReduction { get; set; }
        public IEnumerable<CostReduction> CostReductions { get; set; }
        public string? CostReductionComments { get; set; }
        public bool? ProduceCoBenefits { get; set; }
        public IEnumerable<CoBenefit> CoBenefits { get; set; }
        public string? CoBenefitComments { get; set; }
        public IEnumerable<IncreasedResiliency> IncreasedResiliency { get; set; } //Missing list in CRM
        public string? IncreasedResiliencyComments { get; set; }

        //Project Risks - 9
        public bool? ComplexityRiskMitigated { get; set; }
        public IEnumerable<ComplexityRisk> ComplexityRisks { get; set; }
        public string? ComplexityRiskComments { get; set; }
        public bool? ReadinessRiskMitigated { get; set; }
        public IEnumerable<ReadinessRisk> ReadinessRisks { get; set; }
        public string? ReadinessRiskComments { get; set; }
        public bool? SensitivityRiskMitigated { get; set; }
        public IEnumerable<SensitivityRisk> SensitivityRisks { get; set; }
        public string? SensitivityRiskComments { get; set; }
        public bool? CapacityRiskMitigated { get; set; }
        public IEnumerable<CapacityRisk> CapacityRisks { get; set; }
        public string? CapacityRiskComments { get; set; }
        public bool? RiskTransferMigigated { get; set; }
        public IEnumerable<IncreasedOrTransferred> IncreasedOrTransferred { get; set; }
        public string? IncreasedOrTransferredComments { get; set; }

        //Budget - 10
        public string? TotalProjectCostChangeComments { get; set; }
        public decimal? EligibleFundingRequest { get; set; }
        public decimal? EligibleAmountForFP { get; set; }
        public IEnumerable<YearOverYearFunding> YearOverYearFunding { get; set; }
        public decimal? TotalDrifFundingRequest { get; set; }
        public string? DiscrepancyComment { get; set; }
        public string? CostEffectiveComments { get; set; }
        public YesNoOption? PreviousResponse { get; set; }
        public decimal? PreviousResponseCost { get; set; }
        public string? PreviousResponseComments { get; set; }
        public bool? CostConsiderationsApplied { get; set; }
        public IEnumerable<CostConsideration> CostConsiderations { get; set; }
        public string? CostConsiderationsComments { get; set; }
        public IEnumerable<CostEstimate>? CostEstimates { get; set; }
        public CostEstimateClass? CostEstimateClass { get; set; }
        public bool? EstimatesMatchFundingRequest { get; set; }
        public decimal? Contingency { get; set; }
        public string? ContingencyComments { get; set; }
        public decimal? TotalEligibleCosts { get; set; }

        //Attachments - 11
        public bool? HaveResolution { get; set; }
        public IEnumerable<BcGovDocument>? Attachments { get; set; }

        //Review & Declaration - 12
    }

    public class Project
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
        public string? FirstReportPeriod { get; set; }
        public decimal? FundingAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ProjectStatus Status { get; set; }
        public IEnumerable<CostProjectionItem>? CostProjections { get; set; }
        public IEnumerable<PaymentCondition>? Conditions { get; set; }
        public IEnumerable<ContactDetails>? Contacts { get; set; }
        public IEnumerable<InterimReport>? InterimReports { get; set; }
        public IEnumerable<ProjectClaim>? Claims { get; set; }
        public IEnumerable<ProgressReport>? ProgressReports { get; set; }
        public IEnumerable<Forecast>? Forecast { get; set; }
        public IEnumerable<ProjectEvent>? Events { get; set; }
        public IEnumerable<Request>? Requests { get; set; }
        public IEnumerable<BcGovDocument>? Attachments { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public class UserInfo
    {
        public required string UserId { get; set; }
        public required string BusinessName { get; set; }
        public required string BusinessId { get; set; }
    }

    public class FundingInformation
    {
        public string? Name { get; set; }
        public required FundingType? Type { get; set; }
        public decimal? Amount { get; set; }
        public string? OtherDescription { get; set; }

    }

    public class LocationInformation
    {
        public string? Description { get; set; }
        public string? Area { get; set; }
        public string? Ownership { get; set; }
    }

    public class ContactDetails
    {
        public string? Id { get; set; }
        public string? BCeId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Title { get; set; }
        public string? Department { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }

    public class PartneringProponent
    {
        public required string Name { get; set; }
    }

    public class Permit
    {
        public required string Name { get; set; }
    }

    public class CriticalInfrastructure
    {
        public required string Name { get; set; }
        public required string Impact { get; set; }
    }

    public class ProfessionalInfo
    {
        public required string Name { get; set; }
    }

    public class StandardInfo
    {
        public bool? IsCategorySelected { get; set; }
        public required string Category { get; set; }
        public required IEnumerable<ProvincialStandard> Standards { get; set; }
    }

    public class ProvincialStandard
    {
        public required string Name { get; set; }
    }

    public class CostReduction
    {
        public required string Name { get; set; }
    }

    public class CoBenefit
    {
        public required string Name { get; set; }
    }

    public class IncreasedResiliency
    {
        public required string Name { get; set; }
    }

    public class FoundationalOrPreviousWork
    {
        public required string Name { get; set; }
    }

    public class AffectedParty
    {
        public required string Name { get; set; }
    }

    public class ComplexityRisk
    {
        public required string Name { get; set; }
    }

    public class ReadinessRisk
    {
        public required string Name { get; set; }
    }

    public class SensitivityRisk
    {
        public required string Name { get; set; }
    }

    public class CapacityRisk
    {
        public required string Name { get; set; }
    }

    public class TransferRisks
    {
        public required string Name { get; set; }
    }

    public class ClimateAssessmentToolsInfo
    {
        public required string Name { get; set; }
    }

    public class CostConsideration
    {
        public required string Name { get; set; }
    }

    public class YearOverYearFunding
    {
        public string? Year { get; set; }
        public decimal? Amount { get; set; }
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
        public int? TaskNumber { get; set; }
    }

    public class ProposedActivity
    {
        public string? Id { get; set; }
        public ActivityType? ActivityType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Tasks { get; set; }
        public string? Deliverables { get; set; }
        public int? ActivityNumber { get; set; }
    }

    public class ScreenerQuestions
    {
        public bool? ProjectWorkplan { get; set; }
        public bool? ProjectSchedule { get; set; }
        public bool? CostEstimate { get; set; }
        public YesNoOption? SitePlan { get; set; }
        public bool? HaveAuthorityToDevelop { get; set; }
        public YesNoOption? FirstNationsAuthorizedByPartners { get; set; }
        public YesNoOption? LocalGovernmentAuthorizedByPartners { get; set; }
        public YesNoOption? FoundationWorkCompleted { get; set; }
        public bool? EngagedWithFirstNationsOccurred { get; set; }
        public bool? IncorporateFutureClimateConditions { get; set; }
        public bool? MeetsRegulatoryRequirements { get; set; }
        public bool? MeetsEligibilityRequirements { get; set; }
    }

    public class BcGovDocument
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public DocumentType DocumentType { get; set; }
        public string? Comments { get; set; }
        public DateTime? CreatedOn { get; set; }
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
    
    public class ConditionRequest
    {
        public string? Id { get; set; }
        public string? ConditionId { get; set; }
        public string? ConditionName { get; set; }
        public string? Explanation { get; set; }
        public decimal? Limit { get; set; }
        public bool? ConditionMet { get; set; }
        public DateTime? DateMet { get; set; }
        public IEnumerable<BcGovDocument>? Attachments { get; set; }
        public ContactDetails? AuthorizedRepresentative { get; set; }
        public bool? AuthorizedRepresentativeStatement { get; set; }
        public bool? InformationAccuracyStatement { get; set; }
        public RequestStatus? Status { get; set; }
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
        public ProjectClaim? ProjectClaim { get; set; }
        public ProgressReport? ProgressReport { get; set; }
        public Forecast? Forecast { get; set; }
    }

    public class ProjectClaim
    {
        public string? Id { get; set; }
        public string? ReportPeriod { get; set; }
        public string? ContractNumber { get; set; }
        public DateTime? ReportDate { get; set; }
        public DateTime? DateApproved { get; set; }
        public DateTime? DateSubmitted { get; set; }
        public ClaimStatus? Status { get; set; }
    }

    public class ClaimDetails : ProjectClaim
    {
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public InterimProjectType? ProjectType { get; set; }
        public bool? HaveClaimExpenses { get; set; }
        public IEnumerable<Invoice>? Invoices { get; set; }
        public string? ClaimComment { get; set; }
        public ContactDetails? AuthorizedRepresentative { get; set; }
        public bool? AuthorizedRepresentativeStatement { get; set; }
        public bool? InformationAccuracyStatement { get; set; }
        public decimal? PreviousClaimTotal { get; set; } = 0;
        public decimal? TotalClaimed { get; set; }
        public ClaimProject? Project { get; set; }
        public ActiveCondition? ActiveCondition { get; set; }
    }

    public class ClaimProject
    {
        public string? Id { get; set; }
        public decimal? TotalDRIFFundingRequest { get; set; }
        public IEnumerable<ClaimDetails>? Claims { get; set; }
        public ProjectApplication? FullProposal { get; set; }
    }

    public class ActiveCondition
    {
        public string? ConditionName { get; set; }
        public decimal? ConditionPercentage { get; set; }
        public decimal? ConditionAmount { get; set; }
    }

    public class ProjectApplication
    {
        public string? Id { get; set; }
        public IEnumerable<CostEstimate>? CostEstimates { get; set; }
    }

    public class ProgressReport
    {
        public string? Id { get; set; }
        public string? ReportPeriod { get; set; }
        public string? CrmId { get; set; }
        public DateTime? ReportDate { get; set; }
        public DateTime? DateSubmitted { get; set; }
        public DateTime? DateApproved { get; set; }
        public DateTime? DueDate { get; set; }
        public ProgressReportStatus? Status { get; set; }
    }

    public class ProgressReportDetails : ProgressReport
    {
        public InterimProjectType? ProjectType { get; set; }
        public WorkplanDetails? Workplan { get; set; }
        public EventInformationDetails? EventInformation { get; set; }
        public IEnumerable<BcGovDocument>? Attachments { get; set; }
        public ContactDetails? AuthorizedRepresentative { get; set; }
        public bool? AuthorizedRepresentativeStatement { get; set; }
        public bool? InformationAccuracyStatement { get; set; }
    }

    public class ProjectEvent
    {
        public EventStatus? Status { get; set; }
    }

    public class Request
    {
        public string? Id { get; set; }
        public string? CrmId { get; set; }
        public RequestType Type { get; set; }
        public PaymentCondition? Condition { get; set; }
        public string? Description { get; set; }
        public RequestStatus Status { get; set; }
        public IEnumerable<BcGovDocument>? Attachments { get; set; }
        public ContactDetails? AuthorizedRepresentative { get; set; }
        public bool? AuthorizedRepresentativeStatement { get; set; }
        public bool? InformationAccuracyStatement { get; set; }
    }

    public class EventInformationDetails
    {
        public bool? EventsOccurredSinceLastReport { get; set; }
        public IEnumerable<PastEventDetails>? PastEvents { get; set; }
        public bool? AnyUpcomingEvents { get; set; }
        public IEnumerable<ProjectEventDetails>? UpcomingEvents { get; set; }
    }

    public class ProjectEventDetails : ProjectEvent
    {
        public string? Id { get; set; }
        public string? Details { get; set; }
        public DateTime? Date { get; set; }
        public ContactDetails? Contact { get; set; }
        public bool? ProvincialRepresentativeRequest { get; set; }
        public EventType? Type { get; set; }
    }

    public class PastEventDetails
    {
        public string? Id { get; set; }
        public string? Details { get; set; }
        public DateTime? Date { get; set; }
    }

    public class Forecast
    {
        public string? Id { get; set; }
        public string? ReportPeriod { get; set; }
        public string? CrmId { get; set; }
        public DateTime? ReportDate { get; set; }
        public DateTime? DateSubmitted { get; set; }
        public DateTime? DateApproved { get; set; }
        public decimal? Total { get; set; }
        public decimal? OriginalForecast { get; set; }
        public decimal? Variance { get; set; }
        public ForecastStatus? Status { get; set; }
    }

    public class ForecastDetails : Forecast
    {
        public IEnumerable<ForecastItem>? ForecastItems { get; set; }
        public string? VarianceComment { get; set; }
        public IEnumerable<BcGovDocument>? Attachments { get; set; }
        public ContactDetails? AuthorizedRepresentative { get; set; }
        public bool? AuthorizedRepresentativeStatement { get; set; }
        public bool? InformationAccuracyStatement { get; set; }
    }

    public class InterimReportDetails : InterimReport
    {
    }

    public class WorkplanDetails
    {
        public IEnumerable<WorkplanActivityDetails>? WorkplanActivities { get; set; }
        public ProjectProgress? ProjectProgress { get; set; }
        public string? AheadOfScheduleComments { get; set; }
        public DelayReason? DelayReason { get; set; }
        public string? OtherDelayReason { get; set; }
        public string? BehindScheduleMitigatingComments { get; set; }
        public decimal? ProjectCompletionPercentage { get; set; }
        public decimal? ConstructionCompletionPercentage { get; set; }
        public bool? SignageRequired { get; set; }
        public string? SignageNotRequiredComments { get; set; }
        public IEnumerable<FundingSignage>? FundingSignage { get; set; }
        public bool? MediaAnnouncement { get; set; }
        public DateTime? MediaAnnouncementDate { get; set; }
        public string? MediaAnnouncementComment { get; set; }
        public bool? OutstandingIssues { get; set; }
        public string? OutstandingIssuesComments { get; set; }
        public bool? FundingSourcesChanged { get; set; }
        public string? FundingSourcesChangedComment { get; set; }
    }

    public class WorkplanActivityDetails
    {
        public string? Id { get; set; }
        public string? OriginalReportId { get; set; }
        public bool? CopiedFromActivity { get; set; }
        public ActivityType? ActivityType { get; set; }
        public string? Comment { get; set; }
        public ConstructionContractStatus? ConstructionContractStatus { get; set; }
        public PermitToConstructStatus? PermitToConstructStatus { get; set; }
        public WorkplanProgress? ProgressStatus { get; set; }
        public WorkplanStatus? Status { get; set; }
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedCompletionDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualCompletionDate { get; set; }
        public DateTime? ContractAwardDate { get; set; }
        public DateTime? PlannedContractAwardDate { get; set; }
        public DateTime? AwardPermitToConstructDate { get; set; }
        public DateTime? PlannedAwardPermitToConstructDate { get; set; }
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
        public string? SupplierName { get; set; }
        public string? Description { get; set; }
        public decimal? GrossAmount { get; set; }
        public decimal? TaxRebate { get; set; }
        public decimal? ClaimAmount { get; set; }
        public decimal? TotalPST { get; set; }
        public decimal? TotalGST { get; set; }
        public IEnumerable<BcGovDocument>? Attachments { get; set; }
    }

    public class ActivityType
    {
        public string? Name { get; set; }
        public bool? PreCreatedActivity { get; set; }
    }

    public class FundingSignage
    {
        public string? Id { get; set; }
        public SignageType? Type { get; set; }
        public DateTime? DateInstalled { get; set; }
        public DateTime? DateRemoved { get; set; }
        public bool? BeenApproved { get; set; }
    }

    public enum SignageType
    {
        Temporary,
        Digital,
        Plaque,
    }

    public enum CostCategory
    {
        ProjectAdministration,
        ProjectPlanning,
        Design,
        Assessment,
        Mapping,
        ConstructionMaterials,
        FirstNationsEngagement,
        NeighbouringJurisdictions,
        ProponentCommunities,
        IncrementalStaffing,
        ShortTermInterest,
        LandAcquisition,
        Communications,
        ApprovalsPermitting,
        Contingency,
        Other,
    }


    public enum CostUnit
    {
        Hours,
        LumpSum,
        Each,
        Metre,
        SquareMetre,
        CubicMetre,
        Kilometer,
        SquareKilometer,
        Hectare,
        Kilogram,
        Tonne,
        Other,
    }

    public enum CostEstimateClass
    {
        ClassA,
        ClassB,
    }

    public enum ResourceCategory
    {
        ProjectManager,
        CulturalMonitor,
        Elders,
        JuniorQualifiedProfessional,
        IntermediateQualifiedProfessional,
        SeniorQualifiedProfessional,
        PrincipalQualifiedProfessional,
        ProjectSupport,
        Equipment,
        Other,
    }

    public enum ProjectStatus
    {
        NotStarted,
        InProgress,
        Complete,
        Inactive
    }

    public enum InterimProjectType
    {
        Stream1,
        Stream2,
    }

    public enum PeriodType
    {
        OffCycle,
        Final,
        Interim,
    }

    public enum ReportingScheduleType
    {
        Quarterly,
        Monthly
    }

    public enum PaymentConditionStatus
    {
        Met,
        NotMet
    }

    public enum ProvincialMedia
    {
        NotAnnounced,
        NotApplicable
    }

    public enum WorkplanProgress
    {
        NotStarted,
        InProgress,
        Completed
    }

    public enum ProjectProgress
    {
        OnSchedule,
        AheadOfSchedule,
        BehindSchedule,
        Complete,
    }

    public enum ConstructionContractStatus
    {
        Awarded,
        NotAwarded,
    }

    public enum PermitToConstructStatus
    {
        Awarded,
        NotAwarded,
    }

    public enum DelayReason
    {
        Tendering,
        Referendum,
        PropertyAquisition,
        Negotiations,
        ProjectImplementation,
        UnforeseenComplexity,
        ProjectScopeChange,
        GovernmentAgencies,
        UnforeseenContractorDelays,
        Weather,
        ChangeProjectManager,
        Other,
    }

    public enum WorkplanStatus
    {
        Active,
        NoLongerNeeded
    }

    public enum EventStatus
    {
        NotPlanned,
        PlannedDateUnknown,
        PlannedDateKnown,
        AlreadyOccurred,
        Unknown,
    }

    public enum EventType
    {
        GroundBreaking,
        RibbonCuttingOpening,
        CommunityEngagement,
        Other,
    }

    public enum InterimReportStatus
    {
        NotStarted,
        InProgress,
        Approved,
        Skipped,
        Inactive,
    }

    public enum ClaimStatus
    {
        NotStarted,
        DraftStaff,
        DraftProponent,
        Submitted,
        UpdateNeeded,
        Approved,
        Skipped,
        Inactive,
    }

    public enum ProgressReportStatus
    {
        NotStarted,
        DraftStaff,
        DraftProponent,
        Submitted,
        UpdateNeeded,
        Approved,
        Skipped,
        Inactive,
    }

    public enum ForecastStatus
    {
        NotStarted,
        DraftStaff,
        DraftProponent,
        Submitted,
        UpdateNeeded,
        Approved,
        Skipped,
        Inactive,
    }
    
    public enum RequestType
    {
        Condition,
    }
    
    public enum RequestStatus
    {
        DraftProponent,
        DraftStaff,
        Submitted,
        TechnicalReview,
        ReadyForApproval,
        ApprovalReview,
        UpdateNeeded,
        Approved,
        Inactive,
    }

    public enum ProponentType
    {
        FirstNation,
        LocalGovernment,
        RegionalDistrict
    }

    public enum FundingStream
    {
        Stream1,
        Stream2
    }

    public enum EstimatedNumberOfPeople
    {
        OneToTenK,
        TenKToFiftyK,
        FiftyKToHundredK,
        HundredKPlus,
        Unsure
    }

    public enum EstimatedNumberOfPeopleFP
    {
        ZeroToFiveHundred,
        FiveHundredToOneK,
        OneKToFiveK,
        FiveKToTenK,
        TenKToFiftyK,
        FiftyKToHundredK,
        HundredKPlus,
        Unsure
    }

    public enum ProjectType
    {
        New,
        Existing
    }


    public enum FundingType
    {
        Fed,
        FedProv,
        Prov,
        SelfFunding,
        Other,
    }

    public enum Hazards
    {
        Drought,
        ExtremeTemperature,
        Flood,
        Geohazards,
        SeaLevelRise,
        Seismic,
        Tsunami,
        Other,
    }

    public enum IncreasedOrTransferred
    {
        Increased,
        Transferred,
    }

    public enum ApplicationStatus
    {
        DraftStaff,
        DraftProponent,
        Submitted,
        InReview,
        InPool,
        Invited,
        Ineligible,
        Withdrawn,
        FPSubmitted,
        Approved,
        ApprovedInPrinciple,
        Closed,
        Deleted
    }

    public enum ReportType
    {
        OffCycle,
        Interim,
        Final,
    }

    public enum YesNoOption
    {
        No,
        Yes,
        NotApplicable
    }

    public enum AreaUnits
    {
        Hectares,
        Acres,
        SqKm,
    }
}
