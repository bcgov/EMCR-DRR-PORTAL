using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using EMCR.DRR.Controllers;

namespace EMCR.DRR.API.Model
{
    public class FileData
    {
        public required string RecordId { get; set; }
        public required RecordType RecordType { get; set; }
        public required string ContentType { get; set; }
        public required string Name { get; set; }
        public required byte[] Content { get; set; }
        public DocumentType DocumentType { get; set; }
    }

    public class FileUploadModel
    {
        public required string RecordId { get; set; }
        public required RecordType RecordType { get; set; }
        public required string ContentType { get; set; }
        public DocumentType DocumentType { get; set; }
        public required IFormFile File { get; set; }
    }

    public class Attachment
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public DocumentType DocumentType { get; set; } = DocumentType.OtherSupportingDocument;
        [StringLength(ApplicationValidators.COMMENTS_MAX_LENGTH)]
        public string? Comments { get; set; }
    }

    public class DeleteAttachment
    {
        public required string Id { get; set; }
        public required string RecordId { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DocumentType
    {
        OtherSupportingDocument,
        Resolution,
        DetailedCostEstimate,
        PreliminaryDesign,
        SitePlan,
        ProgressReport,
        Invoice,
        ProofOfPayment,
        ForecastReport
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RecordType
    {
        FullProposal,
        ProgressReport,
        Invoice,
        ForecastReport
    }
}
