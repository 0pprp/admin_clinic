using Microsoft.EntityFrameworkCore;
using MohammedRaouf.Domain.Entities;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Domain.Identity;
using Npgsql;

namespace MohammedRaouf.IntegrationTests;

[Collection("Postgres")]
public class DatabaseConstraintTests
{
    private readonly PostgresFixture _fixture;

    public DatabaseConstraintTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Enrollment_cannot_be_duplicated_for_the_same_user_and_course()
    {
        await using var dbContext = _fixture.CreateContext();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var user = CreateUser();
        var course = CreateLifetimeCourse();
        dbContext.Users.Add(user);
        dbContext.Courses.Add(course);
        await dbContext.SaveChangesAsync();

        dbContext.CourseEnrollments.Add(CreateEnrollment(user.Id, course.Id));
        await dbContext.SaveChangesAsync();

        dbContext.CourseEnrollments.Add(CreateEnrollment(user.Id, course.Id));

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
        Assert.IsType<PostgresException>(exception.InnerException);
    }

    [Fact]
    public async Task Lesson_progress_cannot_be_duplicated_for_the_same_user_and_lesson()
    {
        await using var dbContext = _fixture.CreateContext();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var user = CreateUser();
        var course = CreateLifetimeCourse();
        var section = new CourseSection
        {
            Id = Guid.NewGuid(),
            CourseId = course.Id,
            Title = "القسم الأول",
            SortOrder = 1,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var lesson = new Lesson
        {
            Id = Guid.NewGuid(),
            CourseSectionId = section.Id,
            Title = "الدرس الأول",
            DurationSeconds = 60,
            SortOrder = 1,
            Status = LessonStatus.Published,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Users.Add(user);
        dbContext.Courses.Add(course);
        dbContext.CourseSections.Add(section);
        dbContext.Lessons.Add(lesson);
        await dbContext.SaveChangesAsync();

        dbContext.LessonProgress.Add(CreateProgress(user.Id, lesson.Id));
        await dbContext.SaveChangesAsync();

        dbContext.LessonProgress.Add(CreateProgress(user.Id, lesson.Id));

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
        Assert.IsType<PostgresException>(exception.InnerException);
    }

    [Fact]
    public async Task Course_slug_must_be_unique()
    {
        await using var dbContext = _fixture.CreateContext();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var slug = $"course-{Guid.NewGuid():N}";
        dbContext.Courses.Add(CreateLifetimeCourse(slug));
        await dbContext.SaveChangesAsync();

        dbContext.Courses.Add(CreateLifetimeCourse(slug));

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
        Assert.IsType<PostgresException>(exception.InnerException);
    }

    [Fact]
    public async Task Activation_code_hash_must_be_unique()
    {
        await using var dbContext = _fixture.CreateContext();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var user = CreateUser();
        var creator = CreateUser();
        var course = CreateLifetimeCourse();
        var purchaseRequest = CreatePurchaseRequest(user.Id, course.Id);
        dbContext.Users.AddRange(user, creator);
        dbContext.Courses.Add(course);
        dbContext.PurchaseRequests.Add(purchaseRequest);
        await dbContext.SaveChangesAsync();

        var hash = Convert.ToHexString(Guid.NewGuid().ToByteArray());
        dbContext.ActivationCodes.Add(CreateActivationCode(user.Id, course.Id, purchaseRequest.Id, creator.Id, hash));
        await dbContext.SaveChangesAsync();

        dbContext.ActivationCodes.Add(CreateActivationCode(user.Id, course.Id, purchaseRequest.Id, creator.Id, hash));

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
        Assert.IsType<PostgresException>(exception.InnerException);
    }

    [Fact]
    public async Task Purchase_request_number_must_be_unique()
    {
        await using var dbContext = _fixture.CreateContext();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var user = CreateUser();
        var course = CreateLifetimeCourse();
        var requestNumber = $"MR-2026-{Random.Shared.Next(100000, 999999)}";
        dbContext.Users.Add(user);
        dbContext.Courses.Add(course);
        dbContext.PurchaseRequests.Add(CreatePurchaseRequest(user.Id, course.Id, requestNumber));
        await dbContext.SaveChangesAsync();

        dbContext.PurchaseRequests.Add(CreatePurchaseRequest(user.Id, course.Id, requestNumber));

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
        Assert.IsType<PostgresException>(exception.InnerException);
    }

    [Fact]
    public async Task Lifetime_course_allows_null_access_duration()
    {
        await using var dbContext = _fixture.CreateContext();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var course = CreateLifetimeCourse();
        dbContext.Courses.Add(course);

        await dbContext.SaveChangesAsync();

        Assert.Null(course.AccessDurationDays);
        Assert.Equal(CourseAccessType.Lifetime, course.AccessType);
    }

    [Fact]
    public async Task Limited_duration_course_rejects_non_positive_access_duration()
    {
        await using var dbContext = _fixture.CreateContext();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        dbContext.Courses.Add(new Course
        {
            Id = Guid.NewGuid(),
            Title = "دورة محدودة",
            Slug = $"limited-{Guid.NewGuid():N}",
            ShortDescription = "وصف مختصر",
            Description = "وصف",
            PriceIQD = 100000,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Draft,
            AccessType = CourseAccessType.LimitedDuration,
            AccessDurationDays = 0,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
        Assert.IsType<PostgresException>(exception.InnerException);
    }

    private static ApplicationUser CreateUser()
    {
        var id = Guid.NewGuid();
        var email = $"user-{id:N}@test.local";
        var now = DateTimeOffset.UtcNow;

        return new ApplicationUser
        {
            Id = id,
            FullName = "مستخدم اختبار",
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            AccountStatus = AccountStatus.Active,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static Course CreateLifetimeCourse(string? slug = null)
    {
        var now = DateTimeOffset.UtcNow;
        return new Course
        {
            Id = Guid.NewGuid(),
            Title = "دورة اختبار",
            Slug = slug ?? $"course-{Guid.NewGuid():N}",
            ShortDescription = "وصف مختصر",
            Description = "وصف",
            PriceIQD = 250000,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Draft,
            AccessType = CourseAccessType.Lifetime,
            AccessDurationDays = null,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static PurchaseRequest CreatePurchaseRequest(Guid userId, Guid courseId, string? requestNumber = null)
    {
        var now = DateTimeOffset.UtcNow;
        return new PurchaseRequest
        {
            Id = Guid.NewGuid(),
            RequestNumber = requestNumber ?? $"MR-2026-{Guid.NewGuid():N}"[..20],
            UserId = userId,
            CourseId = courseId,
            FullName = "مستخدم اختبار",
            PhoneNumber = "07700000000",
            Email = "buyer@test.local",
            AmountIQD = 250000,
            Status = PurchaseRequestStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static ActivationCode CreateActivationCode(
        Guid userId,
        Guid courseId,
        Guid purchaseRequestId,
        Guid createdBy,
        string codeHash)
    {
        return new ActivationCode
        {
            Id = Guid.NewGuid(),
            CodeHash = codeHash,
            UserId = userId,
            CourseId = courseId,
            PurchaseRequestId = purchaseRequestId,
            Status = ActivationCodeStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };
    }

    private static CourseEnrollment CreateEnrollment(Guid userId, Guid courseId)
    {
        var now = DateTimeOffset.UtcNow;
        return new CourseEnrollment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CourseId = courseId,
            Status = EnrollmentStatus.Active,
            StartedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static LessonProgress CreateProgress(Guid userId, Guid lessonId)
    {
        var now = DateTimeOffset.UtcNow;
        return new LessonProgress
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LessonId = lessonId,
            WatchedSeconds = 10,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
