using MohammedRaouf.Contracts.Consultations;
using MohammedRaouf.Contracts.Purchases;

namespace MohammedRaouf.Contracts.Admin;

public sealed class AdminDashboardSummaryResponse
{
    public required int PendingPurchaseRequests { get; init; }

    public required int AwaitingPaymentRequests { get; init; }

    public required int PaymentReceivedRequests { get; init; }

    public required int ActiveStudents { get; init; }

    public required int ActiveEnrollments { get; init; }

    public required int PublishedCourses { get; init; }

    public required int NewConsultations { get; init; }

    public required int UnreadContactMessages { get; init; }

    public required IReadOnlyList<AdminPurchaseRequestSummaryResponseLite> RecentPurchaseRequests { get; init; }

    public required IReadOnlyList<AdminActivationCodeSummaryResponseLite> RecentActivations { get; init; }

    public required IReadOnlyList<AdminConsultationSummaryResponse> RecentConsultations { get; init; }

    public required IReadOnlyList<AdminAuditLogSummaryResponse> RecentActivity { get; init; }
}

public sealed class AdminPurchaseRequestSummaryResponseLite
{
    public required Guid Id { get; init; }

    public required string RequestNumber { get; init; }

    public required string FullName { get; init; }

    public required string CourseTitle { get; init; }

    public required string Status { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}

public sealed class AdminActivationCodeSummaryResponseLite
{
    public required Guid Id { get; init; }

    public required string UserFullName { get; init; }

    public required string CourseTitle { get; init; }

    public required string Status { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}

public sealed class AdminStudentSummaryResponse
{
    public required Guid Id { get; init; }

    public required string FullName { get; init; }

    public required string Email { get; init; }

    public string? PhoneNumber { get; init; }

    public string? WhatsAppNumber { get; init; }

    public required string AccountStatus { get; init; }

    public required IReadOnlyList<string> Roles { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? LastLoginAt { get; init; }
}

public sealed class AdminStudentDetailResponse
{
    public required Guid Id { get; init; }

    public required string FullName { get; init; }

    public required string Email { get; init; }

    public string? PhoneNumber { get; init; }

    public string? WhatsAppNumber { get; init; }

    public string? Governorate { get; init; }

    public required string AccountStatus { get; init; }

    public required IReadOnlyList<string> Roles { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? LastLoginAt { get; init; }

    public required IReadOnlyList<AdminStudentPurchaseResponse> PurchaseRequests { get; init; }

    public required IReadOnlyList<AdminStudentEnrollmentResponse> Enrollments { get; init; }
}

public sealed class AdminStudentPurchaseResponse
{
    public required Guid Id { get; init; }

    public required string RequestNumber { get; init; }

    public required string CourseTitle { get; init; }

    public required long AmountIQD { get; init; }

    public required string Status { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}

public sealed class AdminStudentEnrollmentResponse
{
    public required Guid Id { get; init; }

    public required Guid CourseId { get; init; }

    public required string CourseTitle { get; init; }

    public required string CourseSlug { get; init; }

    public required string Status { get; init; }

    public required DateTimeOffset StartedAt { get; init; }

    public DateTimeOffset? ExpiresAt { get; init; }

    public required bool CanAccess { get; init; }

    public required int ProgressPercent { get; init; }

    public required int CompletedLessons { get; init; }

    public required int TotalLessons { get; init; }
}

public sealed class AdminEnrollmentSummaryResponse
{
    public required Guid Id { get; init; }

    public required Guid UserId { get; init; }

    public required string StudentName { get; init; }

    public required string StudentEmail { get; init; }

    public required Guid CourseId { get; init; }

    public required string CourseTitle { get; init; }

    public required string Status { get; init; }

    public required DateTimeOffset StartedAt { get; init; }

    public DateTimeOffset? ExpiresAt { get; init; }

    public required string ActivationMethod { get; init; }

    public string? PurchaseRequestNumber { get; init; }
}

public class AdminAuditLogSummaryResponse
{
    public required Guid Id { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required Guid AdminUserId { get; init; }

    public required string ActorName { get; init; }

    public required string Action { get; init; }

    public required string EntityType { get; init; }

    public Guid? EntityId { get; init; }

    public required string Description { get; init; }
}

public sealed class AdminAuditLogDetailResponse : AdminAuditLogSummaryResponse
{
    public string? MetadataJson { get; init; }

    public string? IpAddress { get; init; }
}

public sealed class UpdateRolesRequest
{
    public List<string> Roles { get; set; } = [];
}
