using EMCR.DRR.Controllers;

namespace EMCR.DRR.Managers.Intake
{
    public interface IIntakeManager
    {
        Task<DeclarationQueryResult> Handle(DeclarationQuery query);
        Task<string> Handle(IntakeCommand cmd);
        Task<IntakeQueryResponse> Handle(IntakeQuery cmd);
    }

    public class DeclarationQuery
    { }

    public class DeclarationQueryResult
    {
        public IEnumerable<DeclarationInfo> Items { get; set; } = Array.Empty<DeclarationInfo>();
    }

    public class DeclarationInfo
    {
        public required DeclarationType Type { get; set; }
        public required string Text { get; set; }
    }

    public enum DeclarationType
    {
        AuthorizedRepresentative,
        AccuracyOfInformation
    }

    public class IntakeQueryResponse
    {
        public required IEnumerable<Application> Items { get; set; }
    }

    public class DrrApplicationsQuery : IntakeQuery
    {
        public string? Id { get; set; }
        public string? BusinessId { get; set; }
    }

    public abstract class IntakeQuery
    { }

    public abstract class IntakeCommand
    { }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class DrifEoiSaveApplicationCommand : IntakeCommand
    {
        public EoiApplication application { get; set; } = null!;
        public UserInfo UserInfo { get; set; }
    }

    public class DrifEoiSubmitApplicationCommand : IntakeCommand
    {
        public EoiApplication application { get; set; } = null!;
        public UserInfo UserInfo { get; set; }
    }

    public class Application
    {
        public string? Id { get; set; }
        public string? BCeIDBusinessId { get; set; }
        //Proponent Information
        public ProponentType? ProponentType { get; set; }
        public string? ProponentName { get; set; }
        public ContactDetails? Submitter { get; set; }
        public ContactDetails? ProjectContact { get; set; }
        public ContactDetails? AdditionalContact1 { get; set; }
        public ContactDetails? AdditionalContact2 { get; set; }
        public IEnumerable<PartneringProponent> PartneringProponents { get; set; }

        //Project Information
        public FundingStream? FundingStream { get; set; }
        public string? ProjectTitle { get; set; }
        public ProjectType? ProjectType { get; set; }
        public string? ScopeStatement { get; set; }
        public IEnumerable<Hazards> RelatedHazards { get; set; }
        public string? OtherHazardsDescription { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        //Funding Information
        public decimal? EstimatedTotal { get; set; }
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
        public IEnumerable<CriticalInfrastructure> InfrastructureImpacted { get; set; }
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


        //Declaration
        public bool? AuthorizedRepresentativeStatement { get; set; }
        public bool? FOIPPAConfirmation { get; set; }
        public bool? InformationAccuracyStatement { get; set; }

        public ApplicationStatus Status { get; set; }
        public DateTime? SubmittedDate { get; set; } = null;
        public DateTime? ModifiedOn { get; set; } = null;
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

    public class CriticalInfrastructure
    {
        public required string Name { get; set; }
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

    public enum ApplicationStatus
    {
        DraftStaff,
        DraftProponent,
        Submitted,
        InReview,
        InPool,
        Invited,
        Ineligible,
        Withdrawn
    }
}
