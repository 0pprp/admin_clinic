using MohammedRaouf.Application.Consultations;
using MohammedRaouf.Domain.Enums;

namespace MohammedRaouf.UnitTests;

public class ConsultationStateMachineTests
{
    private readonly ConsultationStateMachine _machine = new();

    [Theory]
    [InlineData(ConsultationStatus.New, ConsultationStatus.Contacted)]
    [InlineData(ConsultationStatus.Contacted, ConsultationStatus.Scheduled)]
    [InlineData(ConsultationStatus.Scheduled, ConsultationStatus.Completed)]
    [InlineData(ConsultationStatus.New, ConsultationStatus.Cancelled)]
    [InlineData(ConsultationStatus.Contacted, ConsultationStatus.Cancelled)]
    [InlineData(ConsultationStatus.Scheduled, ConsultationStatus.Cancelled)]
    [InlineData(ConsultationStatus.New, ConsultationStatus.Rejected)]
    [InlineData(ConsultationStatus.Contacted, ConsultationStatus.Rejected)]
    public void Allows_documented_transitions(ConsultationStatus from, ConsultationStatus to)
    {
        Assert.True(_machine.CanTransition(from, to));
    }

    [Theory]
    [InlineData(ConsultationStatus.New, ConsultationStatus.Scheduled)]
    [InlineData(ConsultationStatus.New, ConsultationStatus.Completed)]
    [InlineData(ConsultationStatus.Contacted, ConsultationStatus.Completed)]
    [InlineData(ConsultationStatus.Scheduled, ConsultationStatus.Rejected)]
    [InlineData(ConsultationStatus.Completed, ConsultationStatus.Contacted)]
    [InlineData(ConsultationStatus.Cancelled, ConsultationStatus.Contacted)]
    [InlineData(ConsultationStatus.Rejected, ConsultationStatus.Scheduled)]
    public void Rejects_illegal_transitions(ConsultationStatus from, ConsultationStatus to)
    {
        Assert.False(_machine.CanTransition(from, to));
    }
}
