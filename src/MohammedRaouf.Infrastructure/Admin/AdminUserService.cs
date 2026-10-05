using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MohammedRaouf.Application.Admin;
using MohammedRaouf.Application.Common;
using MohammedRaouf.Contracts.Admin;
using MohammedRaouf.Contracts.Public;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Domain.Identity;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.Infrastructure.Admin;

public sealed class AdminUserService(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IAdminStudentService students,
    IAdminAuditService audit) : IAdminUserService
{
    public Task<PagedResponse<AdminStudentSummaryResponse>> ListAsync(
        int page,
        int pageSize,
        string? search,
        string? status,
        CancellationToken cancellationToken = default) =>
        students.ListAsync(page, pageSize, search, status, cancellationToken);

    public async Task<ActionResult<AdminStudentSummaryResponse>> UpdateRolesAsync(
        Guid actorUserId,
        Guid userId,
        IReadOnlyList<string> roles,
        string? ip,
        CancellationToken cancellationToken = default)
    {
        var normalized = roles
            .Select(role => role.Trim())
            .Where(role => role.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        if (normalized.Count == 0 || normalized.Any(role => !RoleNames.All.Contains(role)))
        {
            return ActionResult<AdminStudentSummaryResponse>.Fail(400, "غير صالح", "أحد الأدوار غير معروف.");
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return ActionResult<AdminStudentSummaryResponse>.Fail(404, "غير موجود", "المستخدم غير موجود.");
        }

        var current = (await userManager.GetRolesAsync(user)).ToList();
        var removingAdmin = current.Contains(RoleNames.Admin) && !normalized.Contains(RoleNames.Admin);
        if (removingAdmin && await IsLastActiveAdminAsync(user, cancellationToken))
        {
            return ActionResult<AdminStudentSummaryResponse>.Fail(409, "تعارض", "لا يمكن إزالة دور آخر مدير في النظام.");
        }

        var toRemove = current.Except(normalized).ToArray();
        var toAdd = normalized.Except(current).ToArray();
        if (toRemove.Length > 0)
        {
            await userManager.RemoveFromRolesAsync(user, toRemove);
        }

        if (toAdd.Length > 0)
        {
            await userManager.AddToRolesAsync(user, toAdd);
        }

        audit.Add(
            actorUserId,
            "UserRolesChanged",
            "ApplicationUser",
            userId,
            "تم تعديل أدوار المستخدم.",
            new { from = current, to = normalized },
            ip);
        await dbContext.SaveChangesAsync(cancellationToken);

        var updated = await students.GetAsync(userId, cancellationToken);
        return ActionResult<AdminStudentSummaryResponse>.Ok(new AdminStudentSummaryResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            WhatsAppNumber = user.WhatsAppNumber,
            AccountStatus = user.AccountStatus.ToString(),
            Roles = updated?.Roles ?? normalized,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        });
    }

    private async Task<bool> IsLastActiveAdminAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var adminIds = await (from userRole in dbContext.UserRoles
                              join role in dbContext.Roles on userRole.RoleId equals role.Id
                              where role.Name == RoleNames.Admin
                              select userRole.UserId).ToListAsync(cancellationToken);
        var activeAdmins = await dbContext.Users.CountAsync(
            item => adminIds.Contains(item.Id) && item.AccountStatus == AccountStatus.Active,
            cancellationToken);
        return await userManager.IsInRoleAsync(user, RoleNames.Admin) && activeAdmins <= 1;
    }
}
