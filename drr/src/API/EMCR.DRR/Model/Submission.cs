using System.ComponentModel;
using System.Text.Json.Serialization;
using EMCR.DRR.Controllers;

namespace EMCR.DRR.API.Model
{
    public class Submission
    {
        public required string Id { get; set; }
        public required ApplicationType ApplicationType { get; set; }
        public string? ExistingFpId { get; set; }
        public required string ProjectTitle { get; set; }
        public required SubmissionPortalStatus Status { get; set; }
        public required string FundingRequest { get; set; }
        public required DateTime ModifiedDate { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public required string[] PartneringProponents { get; set; }
        public FundingStream? FundingStream { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ApplicationType
    {
        [Description("EOI")]
        EOI,

        [Description("Full Proposal")]
        FP,
    }
}
