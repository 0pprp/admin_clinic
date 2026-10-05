using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MohammedRaouf.Application.Admin;
using MohammedRaouf.Application.Auth;
using MohammedRaouf.Application.Common;
using MohammedRaouf.Application.Learning;
using MohammedRaouf.Contracts.Admin;
using MohammedRaouf.Contracts.Public;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Domain.Identity;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.Infrastructure.Admin;

public sealed class AdminStudentService(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IRefreshTokenService refreshTokens,
    IStudentLearningService learning,
    IAdminAuditService audit,
    TimeProvider timeProvider) : IAdminStudentService
{
    public async Task<PagedResponse<AdminStudentSummaryResponse>> ListAsync(
        int page,
        int pageSize,
        string? search,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var (safePage, safeSize) = Paging.Normalize(page, pageSize);
        var query = dbContext.Users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AccountStatus>(status, true, out var parsed))
        {
            query = query.Where(user => user.AccountStatus == parsed);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(user =>
                user.FullName.ToLower().Contains(term) ||
                user.Email!.ToLower().Contains(term) ||
                (user.PhoneNumber != null && user.PhoneNumber.Contains(term)) ||
                (user.WhatsAppNumber != null && user.WhatsAppNumber.Contains(term)));
        }

        var total = await query.CountAsync(cancellationToken);
        var users = await query
            .OrderByDescending(user => user.CreatedAt)
            .Skip((safePage - 1) * safeSize)
            .Take(safeSize)
            .ToListAsync(cancellationToken);

        var items = new List<AdminStudentSummaryResponse>();
        foreach (var user in users)
        {
            items.Add(await MapSummaryAsync(user));
        }

        return new PagedResponse<AdminStudentSummaryResponse>
        {
            Items = items,
            Page = safePage,
            PageSize = safeSize,
            TotalCount = total
        };
    }

    public async Task<AdminStudentDetailResponse?> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        var purchases = await dbContext.PurchaseRequests.AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.CreatedAt)
            .Select(item => new AdminStudentPurchaseResponse
            {
                Id = item.Id,
                RequestNumber = item.RequestNumber,
                CourseTitle = item.Course.Title,
                AmountIQD = item.AmountIQD,
                Status = item.Status.ToString(),
                CreatedAt = item.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var courses = await learning.ListMyCoursesAsync(userId, cancellationToken);
        var enrollments = courses.Select(item => new AdminStudentEnrollmentResponse
        {
            Id = item.Id,
            CourseId = item.CourseId,
            CourseTitle = item.CourseTitle,
            CourseSlug = item.CourseSlug,
            Status = item.Status,
            StartedAt = item.StartedAt,
            ExpiresAt = item.ExpiresAt,
            CanAccess = item.CanAccess,
            ProgressPercent = item.ProgressPercent,
            CompletedLessons = item.CompletedLessons,
            TotalLessons = item.TotalLessons
        }).ToList();

        return new AdminStudentDetailResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            WhatsAppNumber = user.WhatsAppNumber,
            Governorate = user.Governorate,
            AccountStatus = user.AccountStatus.ToString(),
            Roles = roles.ToList(),
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            PurchaseRequests = purchases,
            Enrollments = enrollments
        };
    }

    public async Task<ActionResult> SuspendCourseAsync(
        Guid actorUserId,
        Guid userId,
        Guid courseId,
        string? ip,
        CancellationToken cancellationToken = default)
    {
        var enrollment = await FindEnrollmentAsync(userId, courseId, cancellationToken);
        if (enrollment is null)
        {
            return ActionResult.Fail(404, "غير موجود", "الاشتراك غير موجود.");
        }

        enrollment.Status = EnrollmentStatus.Suspended;
        enrollment.UpdatedAt = timeProvider.GetUtcNow();
        audit.Add(actorUserId, "CourseEnrollmentSuspended", "CourseEnrollment", enrollment.Id, "تم تعليق الوصول إلى الدورة.", new { userId, courseId }, ip);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult.Success();
    }

    public async Task<ActionResult> RestoreCourseAsync(
        Guid actorUserId,
        Guid userId,
        Guid courseId,
        string? ip,
        CancellationToken cancellationToken = default)
    {
        var enrollment = await FindEnrollmentAsync(userId, courseId, cancellationToken);
        if (enrollment is null)
        {
            return ActionResult.Fail(404, "غير موجود", "الاشتراك غير موجود.");
        }

        if (enrollment.Status != EnrollmentStatus.Suspended)
        {
            return ActionResult.Fail(409, "تعارض", "يمكن استعادة الاشتراك المعلّق فقط.");
        }

        if (enrollment.ExpiresAt is { } expires && expires <= timeProvider.GetUtcNow())
        {
            return ActionResult.Fail(409, "انتهت المدة", "انتهت مدة الوصول. يلزم طلب شراء جديد لإعادة التفعيل.");
        }

        enrollment.Status = EnrollmentStatus.Active;
        enrollment.UpdatedAt = timeProvider.GetUtcNow();
        audit.Add(actorUserId, "CourseEnrollmentRestored", "CourseEnrollment", enrollment.Id, "تم استعادة الوصول إلى الدورة.", new { userId, courseId }, ip);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult.Success();
    }

    public async Task<ActionResult> RevokeCourseAsync(
        Guid actorUserId,
        Guid userId,
        Guid courseId,
        string? ip,
        CancellationToken cancellationToken = default)
    {
        var enrollment = await FindEnrollmentAsync(userId, courseId, cancellationToken);
        if (enrollment is null)
        {
            return ActionResult.Fail(404, "غير موجود", "الاشتراك غير موجود.");
        }

        enrollment.Status = EnrollmentStatus.Revoked;
        enrollment.UpdatedAt = timeProvider.GetUtcNow();
        audit.Add(actorUserId, "CourseEnrollmentRevoked", "CourseEnrollment", enrollment.Id, "تم سحب الدورة من الطالب.", new { userId, courseId }, ip);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult.Success();
    }

    public async Task<ActionResult> SuspendAccountAsync(
        Guid actorUserId,
        Guid userId,
        string? ip,
        CancellationToken cancellationToken = default)
    {
        if (actorUserId == userId)
        {
            return ActionResult.Fail(409, "تعارض", "لا يمكنك تعليق حسابك بنفسك.");
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return ActionResult.Fail(404, "غير موجود", "المستخدم غير موجود.");
        }

        if (await IsLastActiveAdminAsync(user, cancellationToken))
        {
            return ActionResult.Fail(409, "تعارض", "لا يمكن تعليق حساب آخر مدير في النظام.");
        }

        user.AccountStatus = AccountStatus.Suspended;
        user.UpdatedAt = timeProvider.GetUtcNow();
        await userManager.UpdateSecurityStampAsync(user);
        await userManager.UpdateAsync(user);
        await refreshTokens.RevokeAllForUserAsync(userId, ip, cancellationToken);
        audit.Add(actorUserId, "AccountSuspended", "ApplicationUser", userId, "تم تعليق الحساب.", null, ip);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult.Success();
    }

    public async Task<ActionResult> RestoreAccountAsync(
        Guid actorUserId,
        Guid userId,
        string? ip,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return ActionResult.Fail(404, "غير موجود", "المستخدم غير موجود.");
        }

        user.AccountStatus = AccountStatus.Active;
        user.UpdatedAt = timeProvider.GetUtcNow();
        await userManager.UpdateAsync(user);
        audit.Add(actorUserId, "AccountRestored", "ApplicationUser", userId, "تم إعادة تفعيل الحساب.", null, ip);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult.Success();
    }

    public async Task<PagedResponse<AdminEnrollmentSummaryResponse>> ListEnrollmentsAsync(
        int page,
        int pageSize,
        string? status,
        Guid? courseId,
        Guid? userId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var (safePage, safeSize) = Paging.Normalize(page, pageSize);
        var query = dbContext.CourseEnrollments.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<EnrollmentStatus>(status, true, out var parsed))
        {
            query = query.Where(item => item.Status == parsed);
        }

        if (courseId is Guid course)
        {
            query = query.Where(item => item.CourseId == course);
        }

        if (userId is Guid user)
        {
            query = query.Where(item => item.UserId == user);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(item =>
                item.User.FullName.ToLower().Contains(term) ||
                item.User.Email!.ToLower().Contains(term) ||
                item.Course.Title.ToLower().Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(item => item.StartedAt)
            .Skip((safePage - 1) * safeSize)
            .Take(safeSize)
            .Select(item => new AdminEnrollmentSummaryResponse
            {
                Id = item.Id,
                UserId = item.UserId,
                StudentName = item.User.FullName,
                StudentEmail = item.User.Email ?? string.Empty,
                CourseId = item.CourseId,
                CourseTitle = item.Course.Title,
                Status = item.Status.ToString(),
                StartedAt = item.StartedAt,
                ExpiresAt = item.ExpiresAt,
                ActivationMethod = item.ActivationCodeId != null ? "ActivationCode" : item.PurchaseRequestId != null ? "Direct" : "Manual",
                PurchaseRequestNumber = item.PurchaseRequest != null ? item.PurchaseRequest.RequestNumber : null
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<AdminEnrollmentSummaryResponse>
        {
            Items = items,
            Page = safePage,
            PageSize = safeSize,
            TotalCount = total
        };
    }

    private Task<Domain.Entities.CourseEnrollment?> FindEnrollmentAsync(Guid userId, Guid courseId, CancellationToken cancellationToken) =>
        dbContext.CourseEnrollments.FirstOrDefaultAsync(item => item.UserId == userId && item.CourseId == courseId, cancellationToken);

    private async Task<AdminStudentSummaryResponse> MapSummaryAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        return new AdminStudentSummaryResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            WhatsAppNumber = user.WhatsAppNumber,
            AccountStatus = user.AccountStatus.ToString(),
            Roles = roles.ToList(),
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    private async Task<bool> IsLastActiveAdminAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (!await userManager.IsInRoleAsync(user, RoleNames.Admin))
        {
            return false;
        }

        var adminIds = await (from userRole in dbContext.UserRoles
                              join role in dbContext.Roles on userRole.RoleId equals role.Id
                              where role.Name == RoleNames.Admin
                              select userRole.UserId).ToListAsync(cancellationToken);
        var activeAdmins = await dbContext.Users.CountAsync(
            item => adminIds.Contains(item.Id) && item.AccountStatus == AccountStatus.Active,
            cancellationToken);
        return activeAdmins <= 1;
    }
}
