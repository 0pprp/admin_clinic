using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MohammedRaouf.Application.Activation;
using MohammedRaouf.Contracts.Activation;
using MohammedRaouf.Contracts.Purchases;
using MohammedRaouf.Domain.Entities;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Domain.Identity;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.IntegrationTests;

[Collection("Postgres")]
public class ActivationWorkflowTests : IClassFixture<AuthApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly AuthApiFactory _factory;

    public ActivationWorkflowTests(AuthApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Student_can_own_two_courses_independently_after_code_redemption()
    {
        var (student, studentId, admin, _) = await CreateActorsAsync();
        var courseA = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var courseB = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);

        var purchaseA = await CreatePaidPurchaseAsync(student, admin, courseA.Id);
        var issuedA = await IssueCodeAsync(admin, purchaseA.Id);
        var redeemA = await student.PostAsJsonAsync("/api/activation/redeem", new RedeemActivationCodeRequest { Code = issuedA.ActivationCode });
        var bodyA = await redeemA.Content.ReadFromJsonAsync<RedeemActivationCodeResponse>(JsonOptions);

        Assert.Equal(HttpStatusCode.OK, redeemA.StatusCode);
        Assert.Equal(courseA.Id, bodyA?.CourseId);

        DateTimeOffset startedA;
        DateTimeOffset createdA;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var enrollmentA = await db.CourseEnrollments.SingleAsync(item => item.UserId == studentId && item.CourseId == courseA.Id);
            Assert.Equal(EnrollmentStatus.Active, enrollmentA.Status);
            startedA = enrollmentA.StartedAt;
            createdA = enrollmentA.CreatedAt;
        }

        var purchaseB = await CreatePaidPurchaseAsync(student, admin, courseB.Id);
        var issuedB = await IssueCodeAsync(admin, purchaseB.Id);
        var redeemB = await student.PostAsJsonAsync("/api/activation/redeem", new RedeemActivationCodeRequest { Code = issuedB.ActivationCode });

        Assert.Equal(HttpStatusCode.OK, redeemB.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var enrollments = await db.CourseEnrollments.Where(item => item.UserId == studentId).ToListAsync();
            Assert.Equal(2, enrollments.Count);
            var stillA = enrollments.Single(item => item.CourseId == courseA.Id);
            var newB = enrollments.Single(item => item.CourseId == courseB.Id);
            Assert.Equal(EnrollmentStatus.Active, stillA.Status);
            Assert.Equal(EnrollmentStatus.Active, newB.Status);
            Assert.Equal(startedA, stillA.StartedAt);
            Assert.Equal(createdA, stillA.CreatedAt);
            Assert.Equal(purchaseA.Id, stillA.PurchaseRequestId);
        }

        var mine = await student.GetFromJsonAsync<IReadOnlyList<StudentEnrollmentResponse>>("/api/enrollments", JsonOptions);
        Assert.Equal(2, mine!.Count(item => item.CanAccess));
    }

    [Fact]
    public async Task Mixed_code_and_direct_activation_leave_both_courses_active()
    {
        var (student, studentId, admin, _) = await CreateActorsAsync();
        var courseA = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var courseB = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);

        var purchaseA = await CreatePaidPurchaseAsync(student, admin, courseA.Id);
        var issued = await IssueCodeAsync(admin, purchaseA.Id);
        Assert.Equal(HttpStatusCode.OK, (await student.PostAsJsonAsync("/api/activation/redeem", new RedeemActivationCodeRequest { Code = issued.ActivationCode })).StatusCode);

        var purchaseB = await CreatePaidPurchaseAsync(student, admin, courseB.Id);
        var direct = await admin.PostAsync($"/api/admin/purchase-requests/{purchaseB.Id}/direct-activate", null);
        Assert.Equal(HttpStatusCode.OK, direct.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var enrollments = await db.CourseEnrollments.Where(item => item.UserId == studentId).ToListAsync();
        Assert.Equal(2, enrollments.Count);
        Assert.All(enrollments, item => Assert.Equal(EnrollmentStatus.Active, item.Status));
        Assert.Equal(0, await db.ActivationCodes.CountAsync(item => item.PurchaseRequestId == purchaseB.Id));
    }

    [Fact]
    public async Task Redeem_grants_course_access_immediately_without_relogin()
    {
        var (student, _, admin, _) = await CreateActorsAsync();
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var paid = course.Sections.First().Lessons.Single(lesson => !lesson.IsFreePreview);
        var purchase = await CreatePaidPurchaseAsync(student, admin, course.Id);
        var issued = await IssueCodeAsync(admin, purchase.Id);

        Assert.Equal(HttpStatusCode.Forbidden, (await student.GetAsync($"/api/lessons/{paid.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await student.PostAsJsonAsync("/api/activation/redeem", new RedeemActivationCodeRequest { Code = issued.ActivationCode })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await student.GetAsync($"/api/lessons/{paid.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await student.GetAsync($"/api/courses/{course.Id}/outline")).StatusCode);
    }

    [Fact]
    public async Task Lifetime_enrollment_has_null_expiry()
    {
        var (student, studentId, admin, _) = await CreateActorsAsync();
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var purchase = await CreatePaidPurchaseAsync(student, admin, course.Id);
        var issued = await IssueCodeAsync(admin, purchase.Id);
        await student.PostAsJsonAsync("/api/activation/redeem", new RedeemActivationCodeRequest { Code = issued.ActivationCode });

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var enrollment = await db.CourseEnrollments.SingleAsync(item => item.UserId == studentId && item.CourseId == course.Id);
        Assert.Null(enrollment.ExpiresAt);
    }

    [Fact]
    public async Task Limited_duration_sets_expiry_from_clock()
    {
        var (student, studentId, admin, _) = await CreateActorsAsync();
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(
            _factory.Services,
            accessType: CourseAccessType.LimitedDuration,
            accessDurationDays: 90);
        var before = DateTimeOffset.UtcNow;
        var purchase = await CreatePaidPurchaseAsync(student, admin, course.Id);
        await admin.PostAsync($"/api/admin/purchase-requests/{purchase.Id}/direct-activate", null);
        var after = DateTimeOffset.UtcNow;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var enrollment = await db.CourseEnrollments.SingleAsync(item => item.UserId == studentId && item.CourseId == course.Id);
        Assert.NotNull(enrollment.ExpiresAt);
        Assert.InRange(enrollment.ExpiresAt!.Value, before.AddDays(90).AddSeconds(-5), after.AddDays(90).AddSeconds(5));
    }

    [Fact]
    public async Task Wrong_user_cannot_redeem_and_code_stays_active()
    {
        var (studentA, _, admin, _) = await CreateActorsAsync();
        var (studentB, studentBId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var purchase = await CreatePaidPurchaseAsync(studentA, admin, course.Id);
        var issued = await IssueCodeAsync(admin, purchase.Id);

        var attempt = await studentB.PostAsJsonAsync("/api/activation/redeem", new RedeemActivationCodeRequest { Code = issued.ActivationCode });
        var problem = await attempt.Content.ReadFromJsonAsync<ProblemBody>(JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, attempt.StatusCode);
        Assert.Equal("كود التفعيل غير صحيح أو غير متاح للاستخدام.", problem?.Detail);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var code = await db.ActivationCodes.SingleAsync(item => item.PurchaseRequestId == purchase.Id);
        Assert.Equal(ActivationCodeStatus.Active, code.Status);
        Assert.Null(code.UsedAt);
        Assert.Equal(0, await db.CourseEnrollments.CountAsync(item => item.UserId == studentBId));
    }

    [Fact]
    public async Task Used_code_cannot_be_redeemed_again()
    {
        var (student, studentId, admin, _) = await CreateActorsAsync();
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var purchase = await CreatePaidPurchaseAsync(student, admin, course.Id);
        var issued = await IssueCodeAsync(admin, purchase.Id);
        Assert.Equal(HttpStatusCode.OK, (await student.PostAsJsonAsync("/api/activation/redeem", new RedeemActivationCodeRequest { Code = issued.ActivationCode })).StatusCode);

        var second = await student.PostAsJsonAsync("/api/activation/redeem", new RedeemActivationCodeRequest { Code = issued.ActivationCode });
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(1, await db.CourseEnrollments.CountAsync(item => item.UserId == studentId && item.CourseId == course.Id));
    }

    [Fact]
    public async Task Expired_code_fails_without_enrollment_and_reveals_expiry_only_to_owner()
    {
        var (student, studentId, admin, _) = await CreateActorsAsync();
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var purchase = await CreatePaidPurchaseAsync(student, admin, course.Id);
        var issued = await IssueCodeAsync(admin, purchase.Id);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var code = await db.ActivationCodes.SingleAsync(item => item.PurchaseRequestId == purchase.Id);
            code.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1);
            await db.SaveChangesAsync();
        }

        var response = await student.PostAsJsonAsync("/api/activation/redeem", new RedeemActivationCodeRequest { Code = issued.ActivationCode });
        var problem = await response.Content.ReadFromJsonAsync<ProblemBody>(JsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("انتهت صلاحية كود التفعيل، يرجى التواصل مع الدعم.", problem?.Detail);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.Equal(0, await db.CourseEnrollments.CountAsync(item => item.UserId == studentId && item.CourseId == course.Id));
        }
    }

    [Fact]
    public async Task Revoked_code_cannot_be_redeemed_and_new_code_can_be_issued()
    {
        var (student, _, admin, _) = await CreateActorsAsync();
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var purchase = await CreatePaidPurchaseAsync(student, admin, course.Id);
        var issued = await IssueCodeAsync(admin, purchase.Id);

        Guid codeId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            codeId = await db.ActivationCodes.Where(item => item.PurchaseRequestId == purchase.Id).Select(item => item.Id).SingleAsync();
        }

        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsync($"/api/admin/activation-codes/{codeId}/revoke", null)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await student.PostAsJsonAsync("/api/activation/redeem", new RedeemActivationCodeRequest { Code = issued.ActivationCode })).StatusCode);

        var reissued = await IssueCodeAsync(admin, purchase.Id);
        Assert.NotEqual(issued.ActivationCode, reissued.ActivationCode);
        Assert.Equal(HttpStatusCode.OK, (await student.PostAsJsonAsync("/api/activation/redeem", new RedeemActivationCodeRequest { Code = reissued.ActivationCode })).StatusCode);
    }

    [Fact]
    public async Task Redeem_fails_when_purchase_is_still_awaiting_payment()
    {
        var (student, studentId, admin, _) = await CreateActorsAsync();
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var created = await student.PostAsJsonAsync("/api/purchase-requests", new CreatePurchaseRequestRequest { CourseId = course.Id });
        var request = await created.Content.ReadFromJsonAsync<PurchaseRequestCreatedResponse>(JsonOptions);
        await admin.PostAsJsonAsync($"/api/admin/purchase-requests/{request!.Id}/contacted", new AdminPurchaseNoteRequest());
        await admin.PostAsJsonAsync($"/api/admin/purchase-requests/{request.Id}/awaiting-payment", new AdminAwaitingPaymentRequest { PaymentMethod = "زين كاش" });

        var hasher = new ActivationCodeHasher();
        var generator = new ActivationCodeGenerator();
        var plain = generator.GeneratePlainCode();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.ActivationCodes.Add(new ActivationCode
            {
                Id = Guid.NewGuid(),
                CodeHash = hasher.Hash(plain),
                UserId = studentId,
                CourseId = course.Id,
                PurchaseRequestId = request.Id,
                Status = ActivationCodeStatus.Active,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(30),
                CreatedAt = DateTimeOffset.UtcNow,
                CreatedBy = studentId
            });
            await db.SaveChangesAsync();
        }

        var redeem = await student.PostAsJsonAsync("/api/activation/redeem", new RedeemActivationCodeRequest { Code = plain });
        Assert.Equal(HttpStatusCode.BadRequest, redeem.StatusCode);
        using var verify = _factory.Services.CreateScope();
        var verifyDb = verify.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(0, await verifyDb.CourseEnrollments.CountAsync(item => item.UserId == studentId));
        Assert.Equal(PurchaseRequestStatus.AwaitingPayment, await verifyDb.PurchaseRequests.Where(item => item.Id == request.Id).Select(item => item.Status).SingleAsync());
    }

    [Fact]
    public async Task Concurrent_redeem_creates_one_enrollment_and_one_completion()
    {
        var (student, studentId, admin, _) = await CreateActorsAsync();
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var purchase = await CreatePaidPurchaseAsync(student, admin, course.Id);
        var issued = await IssueCodeAsync(admin, purchase.Id);

        var firstTask = student.PostAsJsonAsync("/api/activation/redeem", new RedeemActivationCodeRequest { Code = issued.ActivationCode });
        var secondTask = student.PostAsJsonAsync("/api/activation/redeem", new RedeemActivationCodeRequest { Code = issued.ActivationCode });
        await Task.WhenAll(firstTask, secondTask);

        var statuses = new[] { (await firstTask).StatusCode, (await secondTask).StatusCode };
        Assert.Contains(HttpStatusCode.OK, statuses);
        Assert.Contains(statuses, status => status is HttpStatusCode.BadRequest or HttpStatusCode.Conflict);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(1, await db.CourseEnrollments.CountAsync(item => item.UserId == studentId && item.CourseId == course.Id));
        Assert.Equal(PurchaseRequestStatus.Completed, await db.PurchaseRequests.Where(item => item.Id == purchase.Id).Select(item => item.Status).SingleAsync());
        Assert.Equal(1, await db.ActivationCodes.CountAsync(item => item.PurchaseRequestId == purchase.Id && item.Status == ActivationCodeStatus.Used));
    }

    [Fact]
    public async Task Issue_direct_and_revoke_require_manage_activations()
    {
        var (student, _, admin, _) = await CreateActorsAsync();
        var (support, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Support);
        var (manager, _) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.ContentManager);
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var purchase = await CreatePaidPurchaseAsync(student, admin, course.Id);

        Assert.Equal(HttpStatusCode.Forbidden, (await student.PostAsync($"/api/admin/purchase-requests/{purchase.Id}/issue-activation-code", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await manager.PostAsync($"/api/admin/purchase-requests/{purchase.Id}/issue-activation-code", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await support.PostAsync($"/api/admin/purchase-requests/{purchase.Id}/issue-activation-code", null)).StatusCode);

        Guid codeId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            codeId = await db.ActivationCodes.Where(item => item.PurchaseRequestId == purchase.Id).Select(item => item.Id).SingleAsync();
        }

        Assert.Equal(HttpStatusCode.Forbidden, (await student.PostAsync($"/api/admin/activation-codes/{codeId}/revoke", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await manager.PostAsync($"/api/admin/activation-codes/{codeId}/revoke", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await support.PostAsync($"/api/admin/activation-codes/{codeId}/revoke", null)).StatusCode);

        var otherCourse = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var directPurchase = await CreatePaidPurchaseAsync(student, admin, otherCourse.Id);
        Assert.Equal(HttpStatusCode.Forbidden, (await student.PostAsync($"/api/admin/purchase-requests/{directPurchase.Id}/direct-activate", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await manager.PostAsync($"/api/admin/purchase-requests/{directPurchase.Id}/direct-activate", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await support.PostAsync($"/api/admin/purchase-requests/{directPurchase.Id}/direct-activate", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsync($"/api/admin/purchase-requests/{purchase.Id}/direct-activate", null)).StatusCode);
    }

    [Fact]
    public async Task Audit_and_timeline_are_written_without_plain_code()
    {
        var (student, _, admin, _) = await CreateActorsAsync();
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var purchase = await CreatePaidPurchaseAsync(student, admin, course.Id);
        var issued = await IssueCodeAsync(admin, purchase.Id);
        await student.PostAsJsonAsync("/api/activation/redeem", new RedeemActivationCodeRequest { Code = issued.ActivationCode });

        var courseB = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var purchaseB = await CreatePaidPurchaseAsync(student, admin, courseB.Id);
        var issuedB = await IssueCodeAsync(admin, purchaseB.Id);

        Guid revokedId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            revokedId = await db.ActivationCodes.Where(item => item.PurchaseRequestId == purchaseB.Id).Select(item => item.Id).SingleAsync();
        }

        await admin.PostAsync($"/api/admin/activation-codes/{revokedId}/revoke", null);
        await admin.PostAsync($"/api/admin/purchase-requests/{purchaseB.Id}/direct-activate", null);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logs = await db.AdminAuditLogs.AsNoTracking().ToListAsync();
            var actions = logs.Select(item => item.Action).ToList();
            Assert.Contains("ActivationCodeIssued", actions);
            Assert.Contains("ActivationCodeRevoked", actions);
            Assert.Contains("CourseActivatedByCode", actions);
            Assert.Contains("CourseDirectlyActivated", actions);

            var metadata = string.Join(" ", logs.Select(item => $"{item.Description} {item.MetadataJson}"));
            Assert.DoesNotContain(issued.ActivationCode, metadata, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(issuedB.ActivationCode, metadata, StringComparison.OrdinalIgnoreCase);

            var eventsA = await db.PurchaseRequestEvents.Where(item => item.PurchaseRequestId == purchase.Id).Select(item => item.ToStatus).ToListAsync();
            Assert.Contains(PurchaseRequestStatus.PaymentReceived, eventsA);
            Assert.Contains(PurchaseRequestStatus.ActivationCodeIssued, eventsA);
            Assert.Contains(PurchaseRequestStatus.Completed, eventsA);

            var eventsB = await db.PurchaseRequestEvents.Where(item => item.PurchaseRequestId == purchaseB.Id).Select(item => item.ToStatus).ToListAsync();
            Assert.Contains(PurchaseRequestStatus.PaymentReceived, eventsB);
            Assert.Contains(PurchaseRequestStatus.Completed, eventsB);
        }
    }

    [Fact]
    public async Task Plain_code_is_not_leaked_from_database_or_list_apis()
    {
        var (student, _, admin, _) = await CreateActorsAsync();
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var purchase = await CreatePaidPurchaseAsync(student, admin, course.Id);
        var issued = await IssueCodeAsync(admin, purchase.Id);

        var list = await admin.GetAsync("/api/admin/activation-codes?page=1&pageSize=50");
        var listBody = await list.Content.ReadAsStringAsync();
        var detail = await admin.GetAsync($"/api/admin/activation-codes/{(await GetCodeIdAsync(purchase.Id))}");
        var detailBody = await detail.Content.ReadAsStringAsync();
        var order = await student.GetAsync($"/api/purchase-requests/{purchase.Id}");
        var orderBody = await order.Content.ReadAsStringAsync();
        await student.PostAsJsonAsync("/api/activation/redeem", new RedeemActivationCodeRequest { Code = issued.ActivationCode });
        var enrollments = await student.GetAsync("/api/enrollments");
        var enrollmentsBody = await enrollments.Content.ReadAsStringAsync();

        Assert.DoesNotContain(issued.ActivationCode, listBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(issued.ActivationCode, detailBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(issued.ActivationCode, orderBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(issued.ActivationCode, enrollmentsBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("plainCode", listBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("activationCode", listBody, StringComparison.OrdinalIgnoreCase);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var stored = await db.ActivationCodes.SingleAsync(item => item.PurchaseRequestId == purchase.Id);
        Assert.NotEqual(issued.ActivationCode, stored.CodeHash);
        Assert.Equal(64, stored.CodeHash.Length);
    }

    [Fact]
    public async Task Issuing_a_second_active_code_conflicts_until_revoked()
    {
        var (student, _, admin, _) = await CreateActorsAsync();
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        var purchase = await CreatePaidPurchaseAsync(student, admin, course.Id);
        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsync($"/api/admin/purchase-requests/{purchase.Id}/issue-activation-code", null)).StatusCode);
        var duplicate = await admin.PostAsync($"/api/admin/purchase-requests/{purchase.Id}/issue-activation-code", null);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Fact]
    public async Task Suspended_enrollment_is_reactivated_on_new_paid_purchase()
    {
        var (student, studentId, admin, _) = await CreateActorsAsync();
        var course = await CourseTestHelpers.SeedPublishedCourseWithLessonsAsync(_factory.Services);
        await CourseTestHelpers.SeedEnrollmentAsync(_factory.Services, studentId, course.Id, EnrollmentStatus.Suspended);
        var purchase = await CreatePaidPurchaseAsync(student, admin, course.Id);
        Assert.Equal(HttpStatusCode.OK, (await admin.PostAsync($"/api/admin/purchase-requests/{purchase.Id}/direct-activate", null)).StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var enrollment = await db.CourseEnrollments.SingleAsync(item => item.UserId == studentId && item.CourseId == course.Id);
        Assert.Equal(EnrollmentStatus.Active, enrollment.Status);
        Assert.Equal(purchase.Id, enrollment.PurchaseRequestId);
    }

    [Fact]
    public async Task Support_satisfies_manage_activations_policy()
    {
        using var scope = _factory.Services.CreateScope();
        var authorization = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Authorization.IAuthorizationService>();
        var user = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
            [new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, RoleNames.Support)],
            authenticationType: "Test"));
        var result = await authorization.AuthorizeAsync(user, resource: null, MohammedRaouf.Application.Authorization.AuthorizationPolicies.ManageActivations);
        Assert.True(result.Succeeded);
    }

    private async Task<(HttpClient Student, Guid StudentId, HttpClient Admin, Guid AdminId)> CreateActorsAsync()
    {
        var (student, studentId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Student);
        var (admin, adminId) = await CourseTestHelpers.LoginAsRoleAsync(_factory, RoleNames.Admin);
        return (student, studentId, admin, adminId);
    }

    private static async Task<PurchaseRequestCreatedResponse> CreatePaidPurchaseAsync(
        HttpClient student,
        HttpClient admin,
        Guid courseId)
    {
        var created = await student.PostAsJsonAsync("/api/purchase-requests", new CreatePurchaseRequestRequest { CourseId = courseId });
        var request = await created.Content.ReadFromJsonAsync<PurchaseRequestCreatedResponse>(JsonOptions);
        Assert.NotNull(request);
        (await admin.PostAsJsonAsync($"/api/admin/purchase-requests/{request.Id}/contacted", new AdminPurchaseNoteRequest { Note = "تم التواصل" })).EnsureSuccessStatusCode();
        (await admin.PostAsJsonAsync($"/api/admin/purchase-requests/{request.Id}/awaiting-payment", new AdminAwaitingPaymentRequest { PaymentMethod = "زين كاش" })).EnsureSuccessStatusCode();
        (await admin.PostAsJsonAsync($"/api/admin/purchase-requests/{request.Id}/confirm-payment", new AdminConfirmPaymentRequest
        {
            PaymentMethod = "زين كاش",
            PaymentReference = "TX-ACT"
        })).EnsureSuccessStatusCode();
        return request;
    }

    private static async Task<IssuedActivationCodeResponse> IssueCodeAsync(HttpClient admin, Guid purchaseId)
    {
        var response = await admin.PostAsync($"/api/admin/purchase-requests/{purchaseId}/issue-activation-code", null);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<IssuedActivationCodeResponse>(JsonOptions);
        Assert.NotNull(body);
        return body;
    }

    private async Task<Guid> GetCodeIdAsync(Guid purchaseId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.ActivationCodes.Where(item => item.PurchaseRequestId == purchaseId).Select(item => item.Id).SingleAsync();
    }
}
