namespace EMCR.DRR.API.Resources.Payments
{
    public interface IPaymentRepository
    {
        Task<ManagePaymentResponse> Manage(ManagePaymentRequest request);

        Task<QueryPaymentResponse> Query(QueryPaymentRequest request);
    }

    public abstract class ManagePaymentRequest
    { }

    public abstract class ManagePaymentResponse
    { }

    public abstract class QueryPaymentRequest
    { }

    public abstract class QueryPaymentResponse
    { }

    public class CreatePaymentRequest : ManagePaymentRequest
    {
        public required Payment Payment { get; set; }
    }

    public class CreatePaymentResponse : ManagePaymentResponse
    {
        public required string Id { get; set; }
    }

    public abstract class Payment
    {
        public required string Id { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public string? FailureReason { get; set; }
        public required string PayeeId { get; set; }
    }

    public enum PaymentStatus
    {
        Created = 1,
        Sent = 174360001,
        Paid = 2,
        Failed = 174360002,
        Cancelled = 174360003,
        Issued = 174360004,
        Cleared = 174360005,
    }

    public enum QueueStatus
    {
        None = -1,
        Pending = 174360000,
        Processing = 174360001,
        Failure = 174360002
    }
    public class GetCasPaymentStatusRequest : QueryPaymentRequest
    {
        public DateTime? ChangedFrom { get; set; }
        public DateTime? ChangedTo { get; set; }
        public CasPaymentStatus? InStatus { get; set; }
    }

    public class GetCasPaymentStatusResponse : QueryPaymentResponse
    {
        public IEnumerable<CasPaymentDetails> Payments { get; set; } = Array.Empty<CasPaymentDetails>();
    }

    public class CasPaymentDetails
    {
        public required string PaymentId { get; set; }
        public CasPaymentStatus Status { get; set; }
        public DateTime StatusChangeDate { get; set; }
        public required string CasReferenceNumber { get; set; }
        public required string StatusDescription { get; set; }
    }

    public enum CasPaymentStatus
    {
        Pending,
        Failed,
        Cleared
    }
}
