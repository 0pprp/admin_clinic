namespace MohammedRaouf.Contracts.Purchases;

public sealed class CreatePurchaseRequestRequest
{
    public Guid CourseId { get; set; }

    public string? CustomerNotes { get; set; }
}

public sealed class AdminPurchaseNoteRequest
{
    public string? Note { get; set; }
}

public sealed class AdminAwaitingPaymentRequest
{
    public string? PaymentMethod { get; set; }

    public string? PaymentReference { get; set; }

    public string? AdminNote { get; set; }
}

public sealed class AdminConfirmPaymentRequest
{
    public string PaymentMethod { get; set; } = string.Empty;

    public string? PaymentReference { get; set; }

    public string? AdminNotes { get; set; }
}

public sealed class AdminPurchaseReasonRequest
{
    public string Reason { get; set; } = string.Empty;
}
