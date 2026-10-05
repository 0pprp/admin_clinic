using Microsoft.EntityFrameworkCore;
using MohammedRaouf.Application.Admin;
using MohammedRaouf.Application.Authorization;
using MohammedRaouf.Contracts.Admin;
using MohammedRaouf.Contracts.Consultations;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Domain.Identity;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.Infrastructure.Admin;

public sealed class AdminDashboardService(ApplicationDbContext dbContext) : IAdminDashboardService
{
    public async Task<AdminDashboardSummaryResponse> GetSummaryAsync(
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default)
    {
        var payments = Has(roles, AuthorizationPolicies.ManagePayments);
        var students = Has(roles, AuthorizationPolicies.ManageStudents);
        var courses = Has(roles, AuthorizationPolicies.ManageCourses);
        var consultations = Has(roles, AuthorizationPolicies.ManageConsultations);
        var audit = Has(roles, AuthorizationPolicies.ViewAuditLogs);

        var pending = payments
            ? await dbContext.PurchaseRequests.CountAsync(item => item.Status == PurchaseRequestStatus.Pending, cancellationToken)
            : 0;
        var awaiting = payments
            ? await dbContext.PurchaseRequests.CountAsync(item => item.Status == PurchaseRequestStatus.AwaitingPayment, cancellationToken)
            : 0;
        var received = payments
            ? await dbContext.PurchaseRequests.CountAsync(item => item.Status == PurchaseRequestStatus.PaymentReceived, cancellationToken)
            : 0;
        var activeStudents = students
            ? await dbContext.Users.CountAsync(item => item.AccountStatus == AccountStatus.Active, cancellationToken)
            : 0;
        var activeEnrollments = students
            ? await dbContext.CourseEnrollments.CountAsync(
                item => item.Status == EnrollmentStatus.Active && (item.ExpiresAt == null || item.ExpiresAt > DateTimeOffset.UtcNow),
                cancellationToken)
            : 0;
        var publishedCourses = courses
            ? await dbContext.Courses.CountAsync(item => item.Status == CourseStatus.Published, cancellationToken)
            : 0;
        var newConsultations = consultations
            ? await dbContext.ConsultationRequests.CountAsync(item => item.Status == ConsultationStatus.New, cancellationToken)
            : 0;
        var unread = consultations
            ? await dbContext.ContactMessages.CountAsync(item => item.Status == ContactMessageStatus.New, cancellationToken)
            : 0;

        var recentPurchases = payments
            ? await dbContext.PurchaseRequests.AsNoTracking()
                .OrderByDescending(item => item.CreatedAt)
                .Take(5)
                .Select(item => new AdminPurchaseRequestSummaryResponseLite
                {
                    Id = item.Id,
                    RequestNumber = item.RequestNumber,
                    FullName = item.User.FullName,
                    CourseTitle = item.Course.Title,
                    Status = item.Status.ToString(),
                    CreatedAt = item.CreatedAt
                })
                .ToListAsync(cancellationToken)
            : [];

        var recentActivations = Has(roles, AuthorizationPolicies.ManageActivations)
            ? await dbContext.ActivationCodes.AsNoTracking()
                .OrderByDescending(item => item.CreatedAt)
                .Take(5)
                .Select(item => new AdminActivationCodeSummaryResponseLite
                {
                    Id = item.Id,
                    UserFullName = item.User.FullName,
                    CourseTitle = item.Course.Title,
                    Status = item.Status.ToString(),
                    CreatedAt = item.CreatedAt
                })
                .ToListAsync(cancellationToken)
            : [];

        var recentConsultations = consultations
            ? await dbContext.ConsultationRequests.AsNoTracking()
                .OrderByDescending(item => item.CreatedAt)
                .Take(5)
                .Select(item => new AdminConsultationSummaryResponse
                {
                    Id = item.Id,
                    RequestNumber = item.RequestNumber,
                    FullName = item.FullName,
                    PhoneNumber = item.PhoneNumber,
                    ConsultationType = item.ConsultationType,
                    PreferredDate = item.PreferredDate,
                    Status = item.Status.ToString(),
                    CreatedAt = item.CreatedAt
                })
                .ToListAsync(cancellationToken)
            : [];

        var recentActivity = audit
            ? await dbContext.AdminAuditLogs.AsNoTracking()
                .OrderByDescending(item => item.CreatedAt)
                .Take(8)
                .Select(item => new AdminAuditLogSummaryResponse
                {
                    Id = item.Id,
                    CreatedAt = item.CreatedAt,
                    AdminUserId = item.AdminUserId,
                    ActorName = item.AdminUser.FullName,
                    Action = item.Action,
                    EntityType = item.EntityType,
                    EntityId = item.EntityId,
                    Description = item.Description
                })
                .ToListAsync(cancellationToken)
            : [];

        return new AdminDashboardSummaryResponse
        {
            PendingPurchaseRequests = pending,
            AwaitingPaymentRequests = awaiting,
            PaymentReceivedRequests = received,
            ActiveStudents = activeStudents,
            ActiveEnrollments = activeEnrollments,
            PublishedCourses = publishedCourses,
            NewConsultations = newConsultations,
            UnreadContactMessages = unread,
            RecentPurchaseRequests = recentPurchases,
            RecentActivations = recentActivations,
            RecentConsultations = recentConsultations,
            RecentActivity = recentActivity
        };
    }

    private static bool Has(IReadOnlyCollection<string> roles, string policy) =>
        AuthorizationPolicies.RoleMap[policy].Any(roles.Contains);
}
