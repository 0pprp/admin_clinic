namespace MohammedRaouf.Domain.Enums;

public enum PurchaseRequestStatus
{
    Pending = 0,
    Contacted = 1,
    AwaitingPayment = 2,
    PaymentReceived = 3,
    ActivationCodeIssued = 4,
    Completed = 5,
    Rejected = 6,
    Cancelled = 7
}
