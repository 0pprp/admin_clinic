using MohammedRaouf.Application.Purchases;
using MohammedRaouf.Domain.Enums;

namespace MohammedRaouf.UnitTests;

public class PurchaseRequestStateMachineTests
{
    private readonly PurchaseRequestStateMachine _machine = new();

    [Theory]
    [InlineData(PurchaseRequestStatus.Pending, PurchaseRequestStatus.Contacted)]
    [InlineData(PurchaseRequestStatus.Contacted, PurchaseRequestStatus.AwaitingPayment)]
    [InlineData(PurchaseRequestStatus.AwaitingPayment, PurchaseRequestStatus.PaymentReceived)]
    [InlineData(PurchaseRequestStatus.PaymentReceived, PurchaseRequestStatus.ActivationCodeIssued)]
    [InlineData(PurchaseRequestStatus.PaymentReceived, PurchaseRequestStatus.Completed)]
    [InlineData(PurchaseRequestStatus.ActivationCodeIssued, PurchaseRequestStatus.Completed)]
    [InlineData(PurchaseRequestStatus.ActivationCodeIssued, PurchaseRequestStatus.PaymentReceived)]
    [InlineData(PurchaseRequestStatus.Pending, PurchaseRequestStatus.Rejected)]
    [InlineData(PurchaseRequestStatus.Pending, PurchaseRequestStatus.Cancelled)]
    [InlineData(PurchaseRequestStatus.Contacted, PurchaseRequestStatus.Rejected)]
    [InlineData(PurchaseRequestStatus.AwaitingPayment, PurchaseRequestStatus.Cancelled)]
    public void Allows_documented_transitions(PurchaseRequestStatus from, PurchaseRequestStatus to)
    {
        Assert.True(_machine.CanTransition(from, to));
    }

    [Theory]
    [InlineData(PurchaseRequestStatus.Pending, PurchaseRequestStatus.PaymentReceived)]
    [InlineData(PurchaseRequestStatus.Pending, PurchaseRequestStatus.Completed)]
    [InlineData(PurchaseRequestStatus.Contacted, PurchaseRequestStatus.PaymentReceived)]
    [InlineData(PurchaseRequestStatus.PaymentReceived, PurchaseRequestStatus.Pending)]
    [InlineData(PurchaseRequestStatus.Rejected, PurchaseRequestStatus.Contacted)]
    [InlineData(PurchaseRequestStatus.Cancelled, PurchaseRequestStatus.AwaitingPayment)]
    [InlineData(PurchaseRequestStatus.PaymentReceived, PurchaseRequestStatus.Rejected)]
    [InlineData(PurchaseRequestStatus.PaymentReceived, PurchaseRequestStatus.Cancelled)]
    public void Rejects_illegal_transitions(PurchaseRequestStatus from, PurchaseRequestStatus to)
    {
        Assert.False(_machine.CanTransition(from, to));
    }
}
