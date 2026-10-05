using MohammedRaouf.Domain.Enums;

namespace MohammedRaouf.Application.Consultations;

public sealed class ConsultationStateMachine : IConsultationStateMachine
{
    public bool CanTransition(ConsultationStatus from, ConsultationStatus to) =>
        (from, to) switch
        {
            (ConsultationStatus.New, ConsultationStatus.Contacted) => true,
            (ConsultationStatus.New, ConsultationStatus.Cancelled) => true,
            (ConsultationStatus.New, ConsultationStatus.Rejected) => true,
            (ConsultationStatus.Contacted, ConsultationStatus.Scheduled) => true,
            (ConsultationStatus.Contacted, ConsultationStatus.Cancelled) => true,
            (ConsultationStatus.Contacted, ConsultationStatus.Rejected) => true,
            (ConsultationStatus.Scheduled, ConsultationStatus.Completed) => true,
            (ConsultationStatus.Scheduled, ConsultationStatus.Cancelled) => true,
            _ => false
        };
}

public interface IConsultationStateMachine
{
    bool CanTransition(ConsultationStatus from, ConsultationStatus to);
}
