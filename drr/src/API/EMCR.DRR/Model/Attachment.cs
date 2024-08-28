using System.Text.Json.Serialization;

namespace EMCR.DRR.API.Model
{
    public class Attachment
    {
        public string? Id { get; set; }
        public required string ApplicationId { get; set; }
        public required string Name { get; set; }
        public string? Body { get; set; }
        public string? Comments { get; set; }
        public DocumentType DocumentType { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DocumentType
    {
        ProponentEligibilitySupportingDocument,
        ProjectEligibilitySupportingDocument,
        ProjectWorkplan,
        ProjectSchedule,
        SitePlan,
        PreliminaryDesign,
        ProjectDetailsSupportingDocument,
        CostEstimate,
        BudgetSupportingDocument,
    }
}
