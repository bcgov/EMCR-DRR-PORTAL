using AutoMapper;
using EMCR.DRR.Dynamics;

namespace EMCR.DRR.API.Resources.Payments
{
    internal class PaymentRepository : IPaymentRepository
    {
        private readonly IMapper mapper;
        private readonly IDRRContextFactory dRRContextFactory;
        private readonly ICasGateway casGateway;

        private static CancellationToken CreateCancellationToken() => new CancellationTokenSource().Token;

        public PaymentRepository(IMapper mapper, IDRRContextFactory dRRContextFactory, ICasGateway casGateway)
        {
            this.mapper = mapper;
            this.dRRContextFactory = dRRContextFactory;
            this.casGateway = casGateway;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<ManagePaymentResponse> Manage(ManagePaymentRequest request) =>
            request switch
            {
                //CreatePaymentRequest r => await Handle(r, CreateCancellationToken()),
                //IssuePaymentsBatchRequest r => await Handle(r, CreateCancellationToken()),
                //ProcessCasPaymentReconciliationStatusRequest r => await Handle(r, CreateCancellationToken()),
                //CancelPaymentRequest r => await Handle(r, CreateCancellationToken()),
                //MarkPaymentAsPaidRequest r => await Handle(r, CreateCancellationToken()),
                //MarkPaymentAsIssuedRequest r => await Handle(r, CreateCancellationToken()),
                //ReconcileSupplierIdRequest r => await Handle(r, CreateCancellationToken()),
                //ReconcileEtransferRequest r => await Handle(r, CreateCancellationToken()),

                _ => throw new NotSupportedException($"type {request.GetType().Name}")
            };

        public async Task<QueryPaymentResponse> Query(QueryPaymentRequest request) =>
        request switch
        {
            //SearchPaymentRequest r => await Handle(r, CreateCancellationToken()),
            //GetCasPaymentStatusRequest r => await Handle(r, CreateCancellationToken()),

            _ => throw new NotSupportedException($"type {request.GetType().Name}")
        };
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        

        private static IEnumerable<string> MapCasStatus(CasPaymentStatus? s) =>
            s switch
            {
                CasPaymentStatus.Cleared => new[] { "RECONCILED" },
                CasPaymentStatus.Pending => new[] { "NEGOTIABLE" },
                CasPaymentStatus.Failed => new[] { "VOIDED" },

                _ => new[] { string.Empty }
            };

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        private static CasPaymentStatus ResolveCasStatus(string? s) =>
            s.ToUpperInvariant() switch
            {
                "RECONCILED" => CasPaymentStatus.Cleared,
                "CLEARED" => CasPaymentStatus.Cleared,
                "RECONCILED UNACCOUNTED" => CasPaymentStatus.Cleared,
                "VOIDED" => CasPaymentStatus.Failed,
                "NEGOTIABLE" => CasPaymentStatus.Pending,

                _ => throw new NotImplementedException($"CAS payment status {s}")
            };
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        private static PaymentStatus ResolvePaymentStatus(CasPaymentStatus status) =>
            status switch
            {
                CasPaymentStatus.Cleared => PaymentStatus.Cleared,
                CasPaymentStatus.Pending => PaymentStatus.Issued,
                CasPaymentStatus.Failed => PaymentStatus.Failed,

                _ => throw new NotImplementedException($"CAS payment status {status}")
            };
    }
}
