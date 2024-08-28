using EMCR.DRR.Managers.Intake;

namespace EMCR.DRR.Resources.Applications
{
    public interface IApplicationRepository
    {
        Task<ManageApplicationCommandResult> Manage(ManageApplicationCommand cmd);
        Task<ApplicationQueryResult> Query(ApplicationQuery query);
        Task<DeclarationQueryResult> Query(DeclarationQuery query);
        Task<EntitiesQueryResult> Query(EntitiesQuery query);
        Task<bool> CanAccessApplication(string id, string businessId);
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
    }

    public class ApplicationsQuery : ApplicationQuery
    {
        public string? Id { get; set; }
        public string? BusinessId { get; set; }
    }

    public class EntitiesQuery
    { }

    public class EntitiesQueryResult
    {
        public IEnumerable<string>? VerificationMethods { get; set; } = Array.Empty<string>();
        public IEnumerable<string>? AffectedParties { get; set; } = Array.Empty<string>();
        public IEnumerable<string>? Standards { get; set; } = Array.Empty<string>();
        public IEnumerable<string>? CostReductions { get; set; } = Array.Empty<string>();
        public IEnumerable<string>? CoBenefits { get; set; } = Array.Empty<string>();
        public IEnumerable<string>? ComplexityRisks { get; set; } = Array.Empty<string>();
        public IEnumerable<string>? ReadinessRisks { get; set; } = Array.Empty<string>();
        public IEnumerable<string>? SensitivityRisks { get; set; } = Array.Empty<string>();
        public IEnumerable<string>? CostConsiderations { get; set; } = Array.Empty<string>();
        public IEnumerable<string>? CapacityRisks { get; set; } = Array.Empty<string>();
        public IEnumerable<string>? FiscalYears { get; set; } = Array.Empty<string>();
    }

    public class DeclarationQuery
    { }

    public class DeclarationQueryResult
    {
        public IEnumerable<DeclarationInfo> Items { get; set; } = Array.Empty<DeclarationInfo>();
    }

    public class SubmitApplication : ManageApplicationCommand
    {
        public required Application Application { get; set; }
    }

    public enum DeclarationTypeOptionSet
    {
        AuthorizedRepresentative = 172580000,
        AccuracyOfInformation = 172580001
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
    }
}
