namespace MohammedRaouf.Contracts.Purchases;

public sealed class PurchaseRequestCreatedResponse
{
    public required Guid Id { get; init; }

    public required string RequestNumber { get; init; }

    public required Guid CourseId { get; init; }

    public required string CourseTitle { get; init; }

    public required long AmountIQD { get; init; }

    public required string Status { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}

public sealed class StudentPurchaseRequestSummaryResponse
{
    public required Guid Id { get; init; }

    public required string RequestNumber { get; init; }

    public required Guid CourseId { get; init; }

    public required string CourseTitle { get; init; }

    public required string CourseSlug { get; init; }

    public required long AmountIQD { get; init; }

    public required string Status { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}

public sealed class StudentPurchaseRequestDetailResponse
{
    public required Guid Id { get; init; }

    public required string RequestNumber { get; init; }

    public required Guid CourseId { get; init; }

    public required string CourseTitle { get; init; }

    public required string CourseSlug { get; init; }

    public required long AmountIQD { get; init; }

    public required string Status { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? ContactedAt { get; init; }

    public DateTimeOffset? PaymentReceivedAt { get; init; }

    public string? CustomerNotes { get; init; }

    public required bool HasActiveEnrollment { get; init; }

    public required IReadOnlyList<StudentPurchaseEventResponse> Timeline { get; init; }
}

public sealed class StudentPurchaseEventResponse
{
    public required string Status { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}

public sealed class PaymentInstructionsResponse
{
    public string? PaymentMethods { get; init; }

    public string? TransferInstructions { get; init; }

    public string? SupportPhone { get; init; }

    public string? SupportWhatsApp { get; init; }
}

public sealed class AdminPurchaseRequestSummaryResponse
{
    public required Guid Id { get; init; }

    public required string RequestNumber { get; init; }

    public required string FullName { get; init; }

    public required string Email { get; init; }

    public required string PhoneNumber { get; init; }

    public required string CourseTitle { get; init; }

    public required long AmountIQD { get; init; }

    public required string Status { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}

public sealed class AdminPurchaseRequestDetailResponse
{
    public required Guid Id { get; init; }

    public required string RequestNumber { get; init; }

    public required Guid UserId { get; init; }

    public required string FullName { get; init; }

    public required string Email { get; init; }

    public required string PhoneNumber { get; init; }

    public string? WhatsAppNumber { get; init; }

    public string? Governorate { get; init; }

    public required Guid CourseId { get; init; }

    public required string CourseTitle { get; init; }

    public required string CourseSlug { get; init; }

    public required long AmountIQD { get; init; }

    public string? PaymentMethod { get; init; }

    public string? PaymentReference { get; init; }

    public required string Status { get; init; }

    public string? CustomerNotes { get; init; }

    public string? AdminNotes { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? ContactedAt { get; init; }

    public DateTimeOffset? PaymentReceivedAt { get; init; }

    public Guid? ConfirmedBy { get; init; }

    public required bool CanActivate { get; init; }

    public required IReadOnlyList<AdminPurchaseEventResponse> Timeline { get; init; }
}

public sealed class AdminPurchaseEventResponse
{
    public required Guid Id { get; init; }

    public string? FromStatus { get; init; }

    public required string ToStatus { get; init; }

    public Guid? ActorUserId { get; init; }

    public string? Note { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}
