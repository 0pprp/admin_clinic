using MohammedRaouf.Domain.Enums;

namespace MohammedRaouf.Application.Purchases;

public static class PurchaseWorkflowStatuses
{
    public static readonly PurchaseRequestStatus[] BlockingNewRequest =
    [
        PurchaseRequestStatus.Pending,
        PurchaseRequestStatus.Contacted,
        PurchaseRequestStatus.AwaitingPayment,
        PurchaseRequestStatus.PaymentReceived,
        PurchaseRequestStatus.ActivationCodeIssued,
        PurchaseRequestStatus.Completed
    ];

    public static readonly PurchaseRequestStatus[] OpenForUniqueIndex =
    [
        PurchaseRequestStatus.Pending,
        PurchaseRequestStatus.Contacted,
        PurchaseRequestStatus.AwaitingPayment,
        PurchaseRequestStatus.PaymentReceived,
        PurchaseRequestStatus.ActivationCodeIssued
    ];

    public const string OpenStatusSqlFilter =
        "\"Status\" IN ('Pending', 'Contacted', 'AwaitingPayment', 'PaymentReceived', 'ActivationCodeIssued')";
}
