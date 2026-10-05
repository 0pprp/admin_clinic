using MohammedRaouf.Domain.Enums;

namespace MohammedRaouf.Application.Purchases;

public interface IPurchaseRequestStateMachine
{
    bool CanTransition(PurchaseRequestStatus from, PurchaseRequestStatus to);

    IReadOnlyCollection<PurchaseRequestStatus> AllowedTargets(PurchaseRequestStatus from);
}
