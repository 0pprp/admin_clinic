using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Domain.Identity;

namespace MohammedRaouf.Domain.Entities;

public class PurchaseRequestEvent
{
    public Guid Id { get; set; }

    public Guid PurchaseRequestId { get; set; }

    public PurchaseRequestStatus? FromStatus { get; set; }

    public PurchaseRequestStatus ToStatus { get; set; }

    public Guid? ActorUserId { get; set; }

    public string? Note { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public PurchaseRequest PurchaseRequest { get; set; } = null!;

    public ApplicationUser? ActorUser { get; set; }
}
