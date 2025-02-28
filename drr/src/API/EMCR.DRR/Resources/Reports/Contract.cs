using EMCR.DRR.Managers.Intake;

namespace EMCR.DRR.API.Resources.Reports
{
    public interface IReportRepository
    {
        Task<ManageReportCommandResult> Manage(ManageReportCommand cmd);
        Task<ReportQueryResult> Query(ReportQuery query);
        Task<ClaimQueryResult> Query(ClaimQuery query);
        Task<ProgressReportQueryResult> Query(ProgressReportQuery query);
        Task<ForecastQueryResult> Query(ForecastQuery query);
        Task<InvoiceQueryResult> Query(InvoiceQuery query);
        Task<bool> CanAccessReport(string id, string businessId);
        Task<bool> CanAccessProgressReport(string id, string businessId);
        Task<bool> CanAccessProgressReportFromDocumentId(string id, string businessId);
        Task<bool> CanAccessClaim(string id, string businessId);
        Task<bool> CanAccessForecast(string id, string businessId);
        Task<bool> CanAccessInvoiceFromDocumentId(string id, string businessId);
    }

    public abstract class ManageReportCommand
    { }

    public class ManageReportCommandResult
    {
        public required string Id { get; set; }
    }

    public class CreateProjectReport : ManageReportCommand
    {
        public required string ProjectId { get; set; }
        public required string ReportPeriodName { get; set; }
        public ReportType ReportType { get; set; }
    }

    public class SaveProgressReport : ManageReportCommand
    {
        public required ProgressReportDetails ProgressReport { get; set; }
    }

    public class SubmitProgressReport : ManageReportCommand
    {
        public required string Id { get; set; }
    }

    public class SaveClaim : ManageReportCommand
    {
        public required ClaimDetails Claim { get; set; }
    }

    public class SubmitClaim : ManageReportCommand
    {
        public required string Id { get; set; }
    }

    public class CreateInvoice : ManageReportCommand
    {
        public required string ClaimId { get; set; }
        public required string InvoiceId { get; set; }
    }

    public class DeleteInvoice : ManageReportCommand
    {
        public required string InvoiceId { get; set; }
    }

    public abstract class ReportQuery
    { }

    public class ReportQueryResult
    {
        public IEnumerable<InterimReportDetails> Items { get; set; } = Array.Empty<InterimReportDetails>();
        public int Length { get; set; }
    }

    public class ReportsQuery : ReportQuery
    {
        public string? Id { get; set; }
        public string? BusinessId { get; set; }
    }

    public abstract class ClaimQuery
    { }

    public class ClaimQueryResult
    {
        public IEnumerable<ClaimDetails> Items { get; set; } = Array.Empty<ClaimDetails>();
        public int Length { get; set; }
    }

    public class ClaimsQuery : ClaimQuery
    {
        public string? Id { get; set; }
        public string? BusinessId { get; set; }
    }

    public abstract class ProgressReportQuery
    { }

    public class ProgressReportQueryResult
    {
        public IEnumerable<ProgressReportDetails> Items { get; set; } = Array.Empty<ProgressReportDetails>();
        public int Length { get; set; }
    }

    public class ProgressReportsQuery : ProgressReportQuery
    {
        public string? Id { get; set; }
        public string? BusinessId { get; set; }
    }

    public abstract class ForecastQuery
    { }

    public class ForecastQueryResult
    {
        public IEnumerable<ForecastDetails> Items { get; set; } = Array.Empty<ForecastDetails>();
        public int Length { get; set; }
    }

    public class ForecastsQuery : ForecastQuery
    {
        public string? Id { get; set; }
        public string? BusinessId { get; set; }
    }

    public class InvoiceQueryResult
    {
        public IEnumerable<Invoice> Items { get; set; } = Array.Empty<Invoice>();
        public int Length { get; set; }
    }

    public abstract class InvoiceQuery
    { }

    public class InvoicesQuery : InvoiceQuery
    {
        public string? Id { get; set; }
        public string? BusinessId { get; set; }
    }

    public enum ProvincialMediaOptionSet
    {
        NotAnnounced = 172580000,
        NotApplicable = 172580001
    }
}
