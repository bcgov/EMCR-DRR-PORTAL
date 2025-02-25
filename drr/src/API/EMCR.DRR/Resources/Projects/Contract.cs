using EMCR.DRR.Managers.Intake;
using EMCR.DRR.Resources.Applications;

namespace EMCR.DRR.API.Resources.Projects
{
    public interface IProjectRepository
    {
        Task<ProjectQueryResult> Query(ProjectQuery query);
        Task<bool> CanAccessProject(string id, string businessId);
    }

    public abstract class ProjectQuery
    { }

    public class ProjectQueryResult
    {
        public IEnumerable<Project> Items { get; set; } = Array.Empty<Project>();
        public int Length { get; set; }
    }

    public class ProjectsQuery : ProjectQuery
    {
        public string? Id { get; set; }
        public string? BusinessId { get; set; }
        public int Page { get; set; } = 0;
        public int Count { get; set; } = 0;
        public string? OrderBy { get; set; }
        public FilterOptions? FilterOptions { get; set; }
    }

    //public class ProjectFilterOptions
    //{
    //    public string? ProejctType { get; set; }
    //    public string? ProgramType { get; set; }
    //    public List<int>? Statuses { get; set; }
    //}

    public enum ProjectStatusOptionSet
    {
        NotStarted = 172580001,
        InProgress = 1,
        Complete = 172580000,
        Inactive = 2,
    }

    public enum ProjectReportStatusOptionSet
    {
        NotStarted = 1,
        InProgress = 172580001,
        Approved = 172580000,
        Skipped = 172580002,
        Inactive = 2,
    }

    public enum ProjectClaimStatusOptionSet
    {
        Approved = 1,
        Rejected = 172580000,
        Invalid = 172580001,
        InProgress = 172580002,
        Submitted = 172580003,
        Inactive = 2,
    }

    public enum ProjectProgressReportStatusOptionSet
    {
        NotStarted = 172580000,
        DraftProponent = 1,
        DraftStaff = 172580004,
        Submitted = 172580001,
        UpdateNeeded = 1172580002,
        Approved = 172580003,
        Skipped = 172580005,
        Inactive = 2,
    }

    public enum ForecastStatusOptionSet
    {
        NotStarted = 1,
        Draft = 172580000,
        Submitted = 172580001,
        UpdateNeeded = 1172580002,
        Approved = 172580003,
        Inactive = 2,
    }

    public enum PeriodTypeOptionSet
    {
        OffCycle = 172580000,
        Final = 172580001,
        Interim = 172580002,
    }

    public enum WorkplanProgressOptionSet
    {
        NotStarted = 172580000,
        InProgress = 172580001,
        Completed = 172580002
    }

    public enum ProjectProgressOptionSet
    {
        OnSchedule = 172580000,
        AheadOfSchedule = 172580001,
        BehindSchedule = 172580002,
        Complete = 172580003,
    }

    public enum SignageTypeOptionSet
    {
        Temporary = 172580000,
        Digital = 172580001,
        Plaque = 172580002,
    }

    public enum ConstructionContractOptionSet
    {
        Awarded = 172580000,
        NotAwarded = 172580001,
    }

    public enum PermitToConstructOptionSet
    {
        Awarded = 172580000,
        NotAwarded = 172580001,
    }

    public enum WorkplanStatusOptionSet
    {
        Active = 1,
        NoLongerNeeded = 2
    }

    public enum EventTypeOptionSet
    {
        GroundBreaking = 172580000,
        RibbonCuttingOpening = 172580001,
        CommunityEngagement = 172580002,
        Other = 172580003,
    }

    public enum EventStatusOptionSet
    {
        NotPlanned = 172580000,
        PlannedDateUnknown = 172580001,
        PlannedDateKnown = 172580002,
        AlreadyOccurred = 172580003,
        Unknown = 172580004,
    }

    public enum DelayReasonOptionSet
    {
        Tendering = 172580000,
        Referendum = 172580001,
        PropertyAquisition = 172580002,
        Negotiations = 172580003,
        ProjectImplementation = 172580004,
        UnforeseenComplexity = 172580005,
        ProjectScopeChange = 172580006,
        GovernmentAgencies = 172580007,
        UnforeseenContractorDelays = 172580008,
        Weather = 172580009,
        ChangeProjectManager = 172580010,
        Other = 172580011,
    }
}
