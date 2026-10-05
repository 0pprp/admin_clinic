using MohammedRaouf.Domain.Enums;

namespace MohammedRaouf.Application.Purchases;

public sealed class PurchaseRequestStateMachine : IPurchaseRequestStateMachine
{
    private static readonly Dictionary<PurchaseRequestStatus, PurchaseRequestStatus[]> Map = new()
    {
        [PurchaseRequestStatus.Pending] =
        [
            PurchaseRequestStatus.Contacted,
            PurchaseRequestStatus.Rejected,
            PurchaseRequestStatus.Cancelled
        ],
        [PurchaseRequestStatus.Contacted] =
        [
            PurchaseRequestStatus.AwaitingPayment,
            PurchaseRequestStatus.Rejected,
            PurchaseRequestStatus.Cancelled
        ],
        [PurchaseRequestStatus.AwaitingPayment] =
        [
            PurchaseRequestStatus.PaymentReceived,
            PurchaseRequestStatus.Rejected,
            PurchaseRequestStatus.Cancelled
        ],
        [PurchaseRequestStatus.PaymentReceived] =
        [
            PurchaseRequestStatus.ActivationCodeIssued,
            PurchaseRequestStatus.Completed
        ],
        [PurchaseRequestStatus.ActivationCodeIssued] =
        [
            PurchaseRequestStatus.Completed,
            PurchaseRequestStatus.PaymentReceived
        ],
        [PurchaseRequestStatus.Completed] = [],
        [PurchaseRequestStatus.Rejected] = [],
        [PurchaseRequestStatus.Cancelled] = []
    };

    public bool CanTransition(PurchaseRequestStatus from, PurchaseRequestStatus to) =>
        AllowedTargets(from).Contains(to);

    public IReadOnlyCollection<PurchaseRequestStatus> AllowedTargets(PurchaseRequestStatus from) =>
        Map.TryGetValue(from, out var targets) ? targets : [];
}
