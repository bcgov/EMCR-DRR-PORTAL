using EMCR.DRR.Managers.Intake;

namespace EMCR.DRR.API.Resources.Documents
{
    public interface IDocumentRepository
    {
        Task<ManageDocumentCommandResult> Manage(ManageDocumentCommand cmd);
        Task<QueryDocumentCommandResult> Query(QueryDocumentCommand q);
    }

    public abstract class QueryDocumentCommand { }

    public class DocumentQuery : QueryDocumentCommand
    {
        public required string Id { get; set; }
    }

    public class QueryDocumentCommandResult
    {
        public required string RecordId { get; set; }
        public required Document Document { get; set; }
    }

    public class ManageDocumentCommandResult
    {
        public required string Id { get; set; }
        public required string RecordId { get; set; }
    }

    public abstract class ManageDocumentCommand
    { }

    public class CreateApplicationDocument : ManageDocumentCommand
    {
        public required string ApplicationId { get; set; }
        public required string NewDocId { get; set; }
        public required Document Document { get; set; }
    }

    public class DeleteApplicationDocument : ManageDocumentCommand
    {
        public required string Id { get; set; }
    }

    public class CreateProgressReportDocument : ManageDocumentCommand
    {
        public required string ProgressReportId { get; set; }
        public required string NewDocId { get; set; }
        public required Document Document { get; set; }
    }

    public class DeleteProgressReportDocument : ManageDocumentCommand
    {
        public required string Id { get; set; }
    }

    public class CreateInvoiceDocument : ManageDocumentCommand
    {
        public required string InvoiceId { get; set; }
        public required string NewDocId { get; set; }
        public required Document Document { get; set; }
    }

    public class DeleteInvoiceDocument : ManageDocumentCommand
    {
        public required string Id { get; set; }
    }

    public class Document
    {
        public required string Name { get; set; }
        public required string Size { get; set; }
        public DocumentType DocumentType { get; set; }
        public RecordType RecordType { get; set; }
    }

    public enum OriginOptionSet
    {
        Web = 931490000,
        Email = 931490001,
        UserUpload = 931490002,
    }
}
