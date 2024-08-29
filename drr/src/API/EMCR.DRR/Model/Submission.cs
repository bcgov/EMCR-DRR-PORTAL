namespace EMCR.DRR.API.Model
{
    public class Submission
    {
        public required string Id { get; set; }
        public required string ProjectTitle { get; set; }
        public required SubmissionPortalStatus Status { get; set; }
        public required string FundingRequest { get; set; }
        public required DateTime ModifiedDate { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public required string[] PartneringProponents { get; set; }
    }
}
