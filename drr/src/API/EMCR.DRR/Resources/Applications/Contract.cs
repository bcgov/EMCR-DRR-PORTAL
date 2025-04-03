using EMCR.DRR.Managers.Intake;

namespace EMCR.DRR.Resources.Applications
{
    public interface IApplicationRepository
    {
        Task<ManageApplicationCommandResult> Manage(ManageApplicationCommand cmd);
        Task<ApplicationQueryResult> Query(ApplicationQuery query);
        Task<DeclarationQueryResult> Query(DeclarationQuery query);
        Task<EntitiesQueryResult> Query(EntitiesQuery query);
        Task<bool> CanAccessApplication(string id, string businessId, bool forUpdate);
        Task<bool> CanAccessApplicationFromDocumentId(string id, string businessId, bool forUpdate);
        Task<ApplicationQueryResult> QueryList(ApplicationQuery query);
    }

    public abstract class ManageApplicationCommand
    { }

    public class ManageApplicationCommandResult
    {
        public required string Id { get; set; }
    }

    public abstract class ApplicationQuery
    { }

    public class ApplicationQueryResult
    {
        public IEnumerable<Application> Items { get; set; } = Array.Empty<Application>();
        public int Length { get; set; }
    }

    public class ApplicationsQuery : ApplicationQuery
    {
        public string? Id { get; set; }
        public string? BusinessId { get; set; }
        public int Page { get; set; } = 0;
        public int Count { get; set; } = 0;
        public string? OrderBy { get; set; }
        public FilterOptions? FilterOptions { get; set; }
    }

    public class ApplicationByDocumentIdQuery : ApplicationQuery
    {
        public required string DocumentId { get; set; }
    }

    public class FilterOptions
    {
        public string? ApplicationType { get; set; }
        public string? ProgramType { get; set; }
        public List<int>? Statuses { get; set; }
    }

    public class EntitiesQuery
    { }

    public class EntitiesQueryResult
    {
        public IEnumerable<string>? FoundationalOrPreviousWorks { get; set; } = Array.Empty<string>();
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

    public class DeclarationQuery
    { }

    public class DeclarationQueryResult
    {
        public IEnumerable<DeclarationInfo> Items { get; set; } = Array.Empty<DeclarationInfo>();
    }

    public class SaveApplication : ManageApplicationCommand
    {
        public required Application Application { get; set; }
    }

    public class SubmitApplication : ManageApplicationCommand
    {
        public required string Id { get; set; }
    }

    public class DeleteApplication : ManageApplicationCommand
    {
        public required string Id { get; set; }
    }

    public class StandardSingle
    {
        public required string Name { get; set; }
        public required string Category { get; set; }
    }

    public enum DeclarationTypeOptionSet
    {
        AuthorizedRepresentative = 172580000,
        AccuracyOfInformation = 172580001
    }

    public enum FormTypeOptionSet
    {
        Application = 172580000,
        Report = 172580001
    }

    public enum ApplicantTypeOptionSet
    {
        FirstNation = 172580000,
        LocalGovernment = 172580001,
        RegionalDistrict = 172580002
    }

    public enum FundingStreamOptionSet
    {
        Stream1 = 172580000,
        Stream2 = 172580001
    }

    public enum ProjectTypeOptionSet
    {
        New = 172580000,
        Existing = 172580001,
    }


    public enum FundingTypeOptionSet
    {
        Fed = 172580000,
        FedProv = 172580001,
        Prov = 172580002,
        SelfFunding = 172580003,
        Other = 172580004,
    }

    public enum EstimatedNumberOfPeopleOptionSet
    {
        OneToTenK = 172580000,
        TenKToFiftyK = 172580001,
        FiftyKToHundredK = 172580002,
        HundredKPlus = 172580003,
        Unsure = 172580004
    }

    public enum EstimatedNumberOfPeopleFPOptionSet
    {
        ZeroToFiveHundred = 172580000,
        FiveHundredToOneK = 172580001,
        OneKToFiveK = 172580002,
        FiveKToTenK = 172580003,
        TenKToFiftyK = 172580004,
        FiftyKToHundredK = 172580005,
        HundredKPlus = 172580006,
        Unsure = 172580007,
    }

    public enum HazardsOptionSet
    {
        Drought = 172580000,
        Erosion = 172580001,
        ExtremeTemperature = 172580002,
        Flood = 172580003,
        Geohazards = 172580004,
        SeaLevelRise = 172580005,
        Seismic = 172580006,
        Storm = 172580007,
        Tsunami = 172580008,
        Other = 172580999,
    }

    public enum IncreasedOrTransferredOptionSet
    {
        Increased = 172580000,
        Transferred = 172580001,
    }

    public enum AreaUnitsOptionSet
    {
        Hectares = 172580000,
        Acres = 172580001,
        SqKm = 172580002,
    }

    public enum DRRTwoOptions
    {
        Yes = 172580000,
        No = 172580001
    }

    public enum DRRYesNoNotApplicable
    {
        Yes = 172580000,
        No = 172580001,
        NotApplicable = 172580002
    }

    public enum CostCategoryOptionSet
    {
        ProjectAdministration = 172580000,
        ProjectPlanning = 172580013,
        Design = 172580001,
        Assessment = 172580002,
        Mapping = 172580003,
        ConstructionMaterials = 172580004,
        FirstNationsEngagement = 172580005,
        NeighbouringJurisdictions = 172580014,
        ProponentCommunities = 172580015,
        IncrementalStaffing = 172580007,
        ShortTermInterest = 172580008,
        LandAcquisition = 172580009,
        Communications = 172580006,
        ApprovalsPermitting = 172580010,
        Contingency = 172580011,
        Other = 172580012,
    }

    public enum ResourceCategoryOptionSet
    {
        ProjectManager = 172580000,
        CulturalMonitor = 172580001,
        Elders = 172580002,
        JuniorQualifiedProfessional = 172580003,
        IntermediateQualifiedProfessional = 172580004,
        SeniorQualifiedProfessional = 172580005,
        PrincipalQualifiedProfessional = 172580006,
        ProjectSupport = 172580007,
        Equipment = 172580008,
        Other = 172580009,
    }

    public enum CostUnitOptionSet
    {
        Hours = 172580000,
        LumpSum = 172580001,
        Each = 172580002,
        Metre = 172580003,
        SquareMetre = 172580004,
        CubicMetre = 172580005,
        Kilometer = 172580006,
        SquareKilometer = 172580007,
        Hectare = 172580008,
        Kilogram = 172580009,
        Tonne = 172580010,
        Other = 172580011,
    }

    public enum CostEstimateClassOptionSet
    {
        ClassA = 172580000,
        ClassB = 172580001,
    }

    public enum ApplicationStatusOptionSet
    {
        DraftStaff = 1,
        DraftProponent = 172580006,
        Submitted = 172580000,
        InReview = 172580001,
        InPool = 172580002,
        Invited = 172580003,
        Ineligible = 172580004,
        Withdrawn = 172580005,
        FPSubmitted = 172580007,
        Approved = 172580008,
        ApprovedInPrinciple = 172580009,
        Closed = 172580011,
        Deleted = 2,
    }
}
