using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MohammedRaouf.Contracts.Public;
using MohammedRaouf.Domain.Entities;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Domain.Identity;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.IntegrationTests;

[Collection("Postgres")]
public class PublicCatalogTests : IClassFixture<AuthApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly AuthApiFactory _factory;

    public PublicCatalogTests(AuthApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Public_courses_include_only_published_items()
    {
        var published = await SeedCourseAsync(CourseStatus.Published, featured: false);
        var draft = await SeedCourseAsync(CourseStatus.Draft, featured: true);
        var archived = await SeedCourseAsync(CourseStatus.Archived, featured: true);

        var client = _factory.CreateAuthClient();
        var response = await client.GetFromJsonAsync<PagedResponse<CourseSummaryResponse>>("/api/public/courses?page=1&pageSize=24", JsonOptions);

        Assert.NotNull(response);
        Assert.Contains(response.Items, item => item.Slug == published.Slug);
        Assert.DoesNotContain(response.Items, item => item.Slug == draft.Slug);
        Assert.DoesNotContain(response.Items, item => item.Slug == archived.Slug);
    }

    [Fact]
    public async Task Featured_courses_require_published_and_featured()
    {
        var featured = await SeedCourseAsync(CourseStatus.Published, featured: true);
        var published = await SeedCourseAsync(CourseStatus.Published, featured: false);
        var draftFeatured = await SeedCourseAsync(CourseStatus.Draft, featured: true);

        var client = _factory.CreateAuthClient();
        var items = await client.GetFromJsonAsync<List<CourseSummaryResponse>>("/api/public/courses/featured", JsonOptions);

        Assert.NotNull(items);
        Assert.Contains(items, item => item.Slug == featured.Slug);
        Assert.DoesNotContain(items, item => item.Slug == published.Slug);
        Assert.DoesNotContain(items, item => item.Slug == draftFeatured.Slug);
    }

    [Fact]
    public async Task Public_articles_include_only_currently_published_items()
    {
        var author = await SeedUserAsync();
        var live = await SeedArticleAsync(author.Id, ContentStatus.Published, DateTimeOffset.UtcNow.AddMinutes(-5));
        var draft = await SeedArticleAsync(author.Id, ContentStatus.Draft, DateTimeOffset.UtcNow.AddMinutes(-5));
        var future = await SeedArticleAsync(author.Id, ContentStatus.Published, DateTimeOffset.UtcNow.AddDays(2));

        var client = _factory.CreateAuthClient();
        var page = await client.GetFromJsonAsync<PagedResponse<ArticleSummaryResponse>>("/api/public/articles", JsonOptions);
        var latest = await client.GetFromJsonAsync<List<ArticleSummaryResponse>>("/api/public/articles/latest", JsonOptions);

        Assert.NotNull(page);
        Assert.Contains(page.Items, item => item.Slug == live.Slug);
        Assert.DoesNotContain(page.Items, item => item.Slug == draft.Slug);
        Assert.DoesNotContain(page.Items, item => item.Slug == future.Slug);
        Assert.NotNull(latest);
        Assert.Contains(latest, item => item.Slug == live.Slug);
        Assert.DoesNotContain(latest, item => item.Slug == future.Slug);
    }

    [Fact]
    public async Task Only_active_expertise_is_public()
    {
        var active = await SeedExpertiseAsync(isActive: true);
        var inactive = await SeedExpertiseAsync(isActive: false);

        var client = _factory.CreateAuthClient();
        var items = await client.GetFromJsonAsync<List<ExpertisePublicResponse>>("/api/public/expertise", JsonOptions);

        Assert.NotNull(items);
        Assert.Contains(items, item => item.Id == active.Id);
        Assert.DoesNotContain(items, item => item.Id == inactive.Id);
    }

    [Fact]
    public async Task Only_published_testimonials_are_public()
    {
        var published = await SeedTestimonialAsync(isPublished: true);
        var hidden = await SeedTestimonialAsync(isPublished: false);

        var client = _factory.CreateAuthClient();
        var items = await client.GetFromJsonAsync<List<TestimonialPublicResponse>>("/api/public/testimonials", JsonOptions);

        Assert.NotNull(items);
        Assert.Contains(items, item => item.Id == published.Id);
        Assert.DoesNotContain(items, item => item.Id == hidden.Id);
    }

    [Fact]
    public async Task Placeholder_statistics_are_not_exposed_publicly()
    {
        var real = await SeedStatisticAsync(isPlaceholder: false, isActive: true);
        var placeholder = await SeedStatisticAsync(isPlaceholder: true, isActive: true);

        var client = _factory.CreateAuthClient();
        var items = await client.GetFromJsonAsync<List<StatisticPublicResponse>>("/api/public/statistics", JsonOptions);

        Assert.NotNull(items);
        Assert.Contains(items, item => item.Id == real.Id);
        Assert.DoesNotContain(items, item => item.Id == placeholder.Id);
        Assert.DoesNotContain(items, item => item.DisplayValue == placeholder.DisplayValue);
    }

    [Fact]
    public async Task Faq_home_endpoint_respects_ShowOnHome()
    {
        var home = await SeedFaqAsync(showOnHome: true);
        var inner = await SeedFaqAsync(showOnHome: false);

        var client = _factory.CreateAuthClient();
        var homeItems = await client.GetFromJsonAsync<List<FaqPublicResponse>>("/api/public/faq?home=true", JsonOptions);
        var allItems = await client.GetFromJsonAsync<List<FaqPublicResponse>>("/api/public/faq", JsonOptions);

        Assert.NotNull(homeItems);
        Assert.Contains(homeItems, item => item.Id == home.Id);
        Assert.DoesNotContain(homeItems, item => item.Id == inner.Id);
        Assert.NotNull(allItems);
        Assert.Contains(allItems, item => item.Id == home.Id);
        Assert.Contains(allItems, item => item.Id == inner.Id);

        var inactive = await SeedFaqAsync(showOnHome: true, isActive: false);
        var afterInactive = await client.GetFromJsonAsync<List<FaqPublicResponse>>("/api/public/faq", JsonOptions);
        Assert.NotNull(afterInactive);
        Assert.DoesNotContain(afterInactive, item => item.Id == inactive.Id);
    }

    [Fact]
    public async Task Site_settings_expose_only_allowlisted_keys()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emailSetting = await db.SiteSettings.FirstOrDefaultAsync(setting => setting.Key == "PublicEmail");
            if (emailSetting is null)
            {
                db.SiteSettings.Add(new SiteSetting
                {
                    Id = Guid.NewGuid(),
                    Key = "PublicEmail",
                    ValueJson = "\"hello@example.test\"",
                    UpdatedAt = DateTimeOffset.UtcNow
                });
            }
            else
            {
                emailSetting.ValueJson = "\"hello@example.test\"";
                emailSetting.UpdatedAt = DateTimeOffset.UtcNow;
            }

            db.SiteSettings.Add(new SiteSetting
            {
                Id = Guid.NewGuid(),
                Key = $"PaymentSecret-{suffix}",
                ValueJson = "\"sk_live_should_not_leak\"",
                UpdatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateAuthClient();
        var response = await client.GetAsync("/api/public/site-settings");
        var body = await response.Content.ReadAsStringAsync();
        var settings = JsonSerializer.Deserialize<PublicSiteSettingsResponse>(body, JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("hello@example.test", settings?.PublicEmail);
        Assert.DoesNotContain("sk_live_should_not_leak", body, StringComparison.Ordinal);
        Assert.DoesNotContain("PaymentSecret", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Public_course_payload_does_not_leak_internal_fields()
    {
        var course = await SeedCourseAsync(CourseStatus.Published, featured: true, withSecretLesson: true);
        var client = _factory.CreateAuthClient();

        var featured = await client.GetAsync("/api/public/courses/featured");
        var list = await client.GetAsync("/api/public/courses");
        var detail = await client.GetAsync($"/api/public/courses/{course.Slug}");
        var featuredBody = await featured.Content.ReadAsStringAsync();
        var listBody = await list.Content.ReadAsStringAsync();
        var detailBody = await detail.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, featured.StatusCode);
        Assert.Equal(HttpStatusCode.OK, detail.StatusCode);
        foreach (var body in new[] { featuredBody, listBody, detailBody })
        {
            Assert.DoesNotContain("videoKey", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("codeHash", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("passwordHash", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("refreshToken", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("adminNotes", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("paymentReference", body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("secret-video-key", body, StringComparison.OrdinalIgnoreCase);
        }
    }

    private async Task<Course> SeedCourseAsync(CourseStatus status, bool featured, bool withSecretLesson = false)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var now = DateTimeOffset.UtcNow;
        var course = new Course
        {
            Id = Guid.NewGuid(),
            Title = "دورة عامة",
            Slug = $"course-{Guid.NewGuid():N}",
            ShortDescription = "وصف مختصر",
            Description = "وصف تفصيلي",
            PriceIQD = 250000,
            Level = CourseLevel.Beginner,
            Status = status,
            IsFeatured = featured,
            AccessType = CourseAccessType.Lifetime,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Courses.Add(course);

        if (withSecretLesson)
        {
            var section = new CourseSection
            {
                Id = Guid.NewGuid(),
                CourseId = course.Id,
                Title = "قسم سري",
                SortOrder = 1,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.CourseSections.Add(section);
            db.Lessons.Add(new Lesson
            {
                Id = Guid.NewGuid(),
                CourseSectionId = section.Id,
                Title = "درس",
                VideoKey = "secret-video-key",
                DurationSeconds = 60,
                SortOrder = 1,
                Status = LessonStatus.Published,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await db.SaveChangesAsync();
        return course;
    }

    private async Task<ApplicationUser> SeedUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var id = Guid.NewGuid();
        var email = $"pub-{id:N}@test.local";
        var user = new ApplicationUser
        {
            Id = id,
            FullName = "كاتب اختبار",
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            SecurityStamp = Guid.NewGuid().ToString(),
            AccountStatus = AccountStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    private async Task<Article> SeedArticleAsync(Guid authorId, ContentStatus status, DateTimeOffset publishedAt)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var article = new Article
        {
            Id = Guid.NewGuid(),
            Title = "مقال",
            Slug = $"article-{Guid.NewGuid():N}",
            Excerpt = "مقتطف",
            Content = "المحتوى الكامل",
            Status = status,
            PublishedAt = publishedAt,
            AuthorId = authorId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.Articles.Add(article);
        await db.SaveChangesAsync();
        return article;
    }

    private async Task<ExpertiseItem> SeedExpertiseAsync(bool isActive)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var item = new ExpertiseItem
        {
            Id = Guid.NewGuid(),
            Title = isActive ? "مجال ظاهر" : "مجال مخفي",
            Description = "وصف",
            SortOrder = 1,
            IsActive = isActive,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.ExpertiseItems.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    private async Task<Testimonial> SeedTestimonialAsync(bool isPublished)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var item = new Testimonial
        {
            Id = Guid.NewGuid(),
            AuthorDisplayName = "مشترك",
            Body = "شهادة",
            IsPublished = isPublished,
            SortOrder = 1,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.Testimonials.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    private async Task<SiteStatistic> SeedStatisticAsync(bool isPlaceholder, bool isActive)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var item = new SiteStatistic
        {
            Id = Guid.NewGuid(),
            Key = $"stat-{Guid.NewGuid():N}",
            Label = "مؤشر",
            DisplayValue = isPlaceholder ? "+10,000 طالب" : "12",
            IsPlaceholder = isPlaceholder,
            IsActive = isActive,
            SortOrder = 1,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.SiteStatistics.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    private async Task<FaqItem> SeedFaqAsync(bool showOnHome, bool isActive = true)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var item = new FaqItem
        {
            Id = Guid.NewGuid(),
            Question = showOnHome ? "سؤال رئيسي" : "سؤال داخلي",
            Answer = "إجابة",
            SortOrder = 1,
            IsActive = isActive,
            ShowOnHome = showOnHome,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.FaqItems.Add(item);
        await db.SaveChangesAsync();
        return item;
    }
}
