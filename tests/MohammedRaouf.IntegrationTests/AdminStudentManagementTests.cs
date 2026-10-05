using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MohammedRaouf.Contracts.Admin;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Domain.Identity;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.IntegrationTests;

[Collection("Postgres")]
public class AdminStudentManagementTests : IClassFixture<AuthApiFactory>
{
    private readonly AuthApiFactory _factory;

    public AdminStudentManagementTests(AuthApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Suspend_restore_and_revoke_affect_only_the_targeted_course()
    {
        var courseA = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var courseB = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var (student, userId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        await CourseTestHelpers.SeedEnrollmentAsync(_factory.Services, userId, courseA.Id);
        await CourseTestHelpers.SeedEnrollmentAsync(_factory.Services, userId, courseB.Id);

        var now = DateTimeOffset.UtcNow;
        var previewLessonId = courseA.Sections.First().Lessons.First().Id;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.LessonProgress.Add(new Domain.Entities.LessonProgress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LessonId = previewLessonId,
                WatchedSeconds = 20,
                CreatedAt = now,
                UpdatedAt = now
            });
            await db.SaveChangesAsync();
        }

        var (admin, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Admin);
        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsync($"/api/admin/students/{userId}/courses/{courseA.Id}/suspend", null)).StatusCode);

        Assert.Equal(EnrollmentStatus.Suspended, await StatusAsync(userId, courseA.Id));
        Assert.Equal(EnrollmentStatus.Active, await StatusAsync(userId, courseB.Id));

        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsync($"/api/admin/students/{userId}/courses/{courseA.Id}/restore", null)).StatusCode);
        Assert.Equal(EnrollmentStatus.Active, await StatusAsync(userId, courseA.Id));
        Assert.Equal(EnrollmentStatus.Active, await StatusAsync(userId, courseB.Id));

        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsync($"/api/admin/students/{userId}/courses/{courseA.Id}/revoke", null)).StatusCode);
        Assert.Equal(EnrollmentStatus.Revoked, await StatusAsync(userId, courseA.Id));
        Assert.Equal(EnrollmentStatus.Active, await StatusAsync(userId, courseB.Id));

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.True(await db.LessonProgress.AnyAsync(item => item.UserId == userId && item.LessonId == previewLessonId));
            Assert.True(await db.AdminAuditLogs.AnyAsync(item => item.Action == "CourseEnrollmentSuspended"));
            Assert.True(await db.AdminAuditLogs.AnyAsync(item => item.Action == "CourseEnrollmentRevoked"));
        }

        _ = student;
    }

    [Fact]
    public async Task Restore_of_expired_enrollment_is_rejected()
    {
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var (_, userId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        await CourseTestHelpers.SeedEnrollmentAsync(
            _factory.Services,
            userId,
            course.Id,
            EnrollmentStatus.Suspended,
            DateTimeOffset.UtcNow.AddDays(-1));

        var (admin, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Admin);
        var restore = await admin.PostAsync($"/api/admin/students/{userId}/courses/{course.Id}/restore", null);
        Assert.Equal(HttpStatusCode.Conflict, restore.StatusCode);
        Assert.Equal(EnrollmentStatus.Suspended, await StatusAsync(userId, course.Id));
    }

    [Fact]
    public async Task Account_suspend_revokes_sessions_without_changing_enrollments()
    {
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var studentClient = _factory.CreateAuthClient();
        var register = AuthTestHelpers.NewRegisterRequest();
        var user = await AuthTestHelpers.RegisterAsync(studentClient, register);
        (await AuthTestHelpers.LoginAsync(studentClient, register.Email, register.Password, rememberMe: true)).EnsureSuccessStatusCode();
        await CourseTestHelpers.SeedEnrollmentAsync(_factory.Services, user.Id, course.Id);

        var (admin, adminId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Admin);
        Assert.Equal(HttpStatusCode.Conflict, (await admin.PostAsync($"/api/admin/students/{adminId}/suspend-account", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsync($"/api/admin/students/{user.Id}/suspend-account", null)).StatusCode);

        Assert.Equal(HttpStatusCode.Forbidden, (await AuthTestHelpers.LoginAsync(studentClient, register.Email, register.Password)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await studentClient.PostAsync("/api/auth/refresh", null)).StatusCode);
        Assert.Equal(EnrollmentStatus.Active, await StatusAsync(user.Id, course.Id));

        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsync($"/api/admin/students/{user.Id}/restore-account", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await AuthTestHelpers.LoginAsync(studentClient, register.Email, register.Password)).StatusCode);
        Assert.Equal(EnrollmentStatus.Active, await StatusAsync(user.Id, course.Id));
    }

    [Fact]
    public async Task Last_active_admin_cannot_be_suspended_or_stripped_of_admin_role()
    {
        var (adminA, idA) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Admin);
        var (adminB, idB) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Admin);
        var allowed = await adminA.PostAsJsonAsync($"/api/admin/users/{idB}/roles", new UpdateRolesRequest
        {
            Roles = [RoleNames.Student]
        });
        Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);

        var (adminC, idC) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Admin);
        var (support, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Support);
        var suspended = await SuspendOtherActiveAdminsAsync(idC);
        try
        {
            Assert.Equal(HttpStatusCode.Conflict, (await support.PostAsync($"/api/admin/students/{idC}/suspend-account", null)).StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, (await adminC.PostAsJsonAsync($"/api/admin/users/{idC}/roles", new UpdateRolesRequest
            {
                Roles = [RoleNames.Student]
            })).StatusCode);
        }
        finally
        {
            await RestoreUsersAsync(suspended);
        }

        using var scope = _factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var stillAdmin = await users.FindByIdAsync(idC.ToString());
        Assert.True(await users.IsInRoleAsync(stillAdmin!, RoleNames.Admin));
        _ = idA;
    }

    private async Task<EnrollmentStatus> StatusAsync(Guid userId, Guid courseId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.CourseEnrollments
            .Where(item => item.UserId == userId && item.CourseId == courseId)
            .Select(item => item.Status)
            .SingleAsync();
    }

    private async Task<List<Guid>> SuspendOtherActiveAdminsAsync(Guid keepId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var adminRoleId = await db.Roles.Where(role => role.Name == RoleNames.Admin).Select(role => role.Id).SingleAsync();
        var ids = await db.UserRoles.Where(item => item.RoleId == adminRoleId && item.UserId != keepId)
            .Select(item => item.UserId)
            .ToListAsync();
        var users = await db.Users.Where(user => ids.Contains(user.Id) && user.AccountStatus == AccountStatus.Active).ToListAsync();
        foreach (var user in users)
        {
            user.AccountStatus = AccountStatus.Suspended;
        }

        await db.SaveChangesAsync();
        return users.Select(user => user.Id).ToList();
    }

    private async Task RestoreUsersAsync(IReadOnlyCollection<Guid> ids)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var users = await db.Users.Where(user => ids.Contains(user.Id)).ToListAsync();
        foreach (var user in users)
        {
            user.AccountStatus = AccountStatus.Active;
        }

        await db.SaveChangesAsync();
    }
}
