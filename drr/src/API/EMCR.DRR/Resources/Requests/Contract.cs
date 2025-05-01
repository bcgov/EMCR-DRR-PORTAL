using EMCR.DRR.Managers.Intake;

namespace EMCR.DRR.API.Resources.Requests
{
    public interface IRequestRepository
    {
        Task<ManageRequestCommandResult> Manage(ManageRequestCommand cmd);
        Task<RequestQueryResult> Query(RequestQuery query);
        Task<bool> CanAccessRequest(string id, string businessId);
        Task<bool> CanAccessRequestFromDocumentId(string id, string businessId, bool forUpdate);
    }

    public abstract class ManageRequestCommand { }

    public class ManageRequestCommandResult
    {
        public required string Id { get; set; }
    }

    public class SaveConditionRequest : ManageRequestCommand
    {
        public required ConditionRequest Condition { get; set; }
    }
    
    public class SubmitConditionRequest : ManageRequestCommand
    {
        public required string Id { get; set; }
    }

    public class CreateConditionRequest : ManageRequestCommand
    {
        public required string ConditionId { get; set; }
    }

    public class RequestQueryResult
    {
        public IEnumerable<Request> Items { get; set; } = Array.Empty<Request>();
        public int Length { get; set; }
    }

    public abstract class RequestQuery
    { }

    public class RequestsQuery : RequestQuery
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? ConditionId { get; set; }
        public string? BusinessId { get; set; }
    }

    public enum RequestStatusOptionSet
    {
        DraftProponent = 1,
        DraftStaff = 172580000,
        Submitted = 172580001,
        TechnicalReview = 172580004,
        ReadyForApproval = 172580006,
        ApprovalReview = 172580005,
        UpdateNeeded = 172580002,
        Approved = 172580003,
        Inactive = 2,
    }

    public enum RequestTypeOptionSet
    {
        Condition = 172580000,
    }
}
