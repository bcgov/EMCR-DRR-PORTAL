using System.ComponentModel;
using System.Text.Json.Serialization;
using EMCR.DRR.Controllers;

namespace EMCR.DRR.API.Model
{
    public class SubmissionResponse
    {
        public IEnumerable<Submission> Submissions { get; set; } = Array.Empty<Submission>();
        public int Length { get; set; } = 0;
    }

    public class Submission
    {
        public required string Id { get; set; }
        public required ApplicationType ApplicationType { get; set; }
        public required ProgramType ProgramType { get; set; }
        public string? ExistingFpId { get; set; }
        public required string ProjectTitle { get; set; }
        public required SubmissionPortalStatus Status { get; set; }
        public required string FundingRequest { get; set; }
        public required DateTime ModifiedDate { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public required string[] PartneringProponents { get; set; }
        public FundingStream? FundingStream { get; set; }
        public required IEnumerable<Actions> Actions { get; set; } = Array.Empty<Actions>();
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ApplicationType
    {
        [Description("EOI")]
        EOI,

        [Description("Full Proposal")]
        FP,

        [Description("Progress Report")]
        Progress,

        [Description("Budget Forecast Report")]
        Forecast,

        [Description("Claims Form")]
        Claim,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ProgramType
    {
        [Description("DRIF")]
        DRIF,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Actions
    {
        [Description("CreateFP")]
        CreateFP,

        [Description("Delete")]
        Delete,

        [Description("Edit")]
        Edit,

        [Description("Withdraw")]
        Withdraw,
    }
}
