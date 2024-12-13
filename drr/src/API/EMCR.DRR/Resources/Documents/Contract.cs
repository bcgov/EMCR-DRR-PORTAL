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
        public required string ApplicationId { get; set; }
        public required Document Document { get; set; }
    }

    public class ManageDocumentCommandResult
    {
        public required string Id { get; set; }
        public required string ApplicationId { get; set; }
    }

    public abstract class ManageDocumentCommand
    { }

    public class CreateDocument : ManageDocumentCommand
    {
        public required string ApplicationId { get; set; }
        public required string NewDocId { get; set; }
        public required Document Document { get; set; }
    }

    public class DeleteDocument : ManageDocumentCommand
    {
        public required string Id { get; set; }
    }

    public class Document
    {
        public required string Name { get; set; }
        public required string Size { get; set; }
        public DocumentType DocumentType { get; set; }
    }

    public enum DocumentTypeOptionSet
    {
        DetailedCostEstimate = 172580002,
        SitePlan = 172580003,
        PreliminaryDesign = 172580004,
        Resolution = 172580005,
        OtherSupportingDocument = 172580006
    }

    public enum OriginOptionSet
    {
        Web = 931490000,
        Email = 931490001,
        UserUpload = 931490002,
    }
}
