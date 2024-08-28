namespace EMCR.DRR.API.Resources.Cases
{
    public interface ICaseRepository
    {
        Task<QueryCaseCommandResult> Query(QueryCaseCommand query);
        Task<ManageCaseCommandResult> Manage(ManageCaseCommand cmd);
    }

    public abstract class QueryCaseCommand { }

    public class CaseQuery : QueryCaseCommand
    {
        public string? EoiId { get; set; }
        public string? FpId { get; set; }
    }

    public class QueryCaseCommandResult
    {
        public IEnumerable<Case> Items { get; set; } = Array.Empty<Case>();
    }

    public class ManageCaseCommandResult
    {
        public required string Id { get; set; }
    }

    public abstract class ManageCaseCommand
    { }

    public class GenerateFpFromEoi : ManageCaseCommand
    {
        public required string EoiId { get; set; }
    }

    public class Case
    {
        public required string Title { get; set; }
        public string? EoiId { get; set; }
        public string? FpId { get; set; }
    }
}
