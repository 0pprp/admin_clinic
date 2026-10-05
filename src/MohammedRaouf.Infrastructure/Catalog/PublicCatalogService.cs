using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MohammedRaouf.Application.Catalog;
using MohammedRaouf.Contracts.Public;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.Infrastructure.Catalog;

public sealed class PublicCatalogService(ApplicationDbContext dbContext) : IPublicCatalogService
{
    public static readonly string[] AllowedSettingKeys =
    [
        "BrandName",
        "BrandNameEnglish",
        "PublicPhone",
        "PublicWhatsApp",
        "PublicEmail",
        "SocialLinks",
        "FooterText",
        "ConsultationInfo"
    ];

    public async Task<PublicSiteSettingsResponse> GetSiteSettingsAsync(CancellationToken cancellationToken = default)
    {
        var rows = await dbContext.SiteSettings
            .AsNoTracking()
            .Where(setting => AllowedSettingKeys.Contains(setting.Key))
            .Select(setting => new { setting.Key, setting.ValueJson })
            .ToListAsync(cancellationToken);

        var map = rows.ToDictionary(row => row.Key, row => ReadPublicJson(row.ValueJson), StringComparer.Ordinal);

        return new PublicSiteSettingsResponse
        {
            BrandName = Get(map, "BrandName"),
            BrandNameEnglish = Get(map, "BrandNameEnglish"),
            PublicPhone = Get(map, "PublicPhone"),
            PublicWhatsApp = Get(map, "PublicWhatsApp"),
            PublicEmail = Get(map, "PublicEmail"),
            SocialLinks = Get(map, "SocialLinks"),
            FooterText = Get(map, "FooterText"),
            ConsultationInfo = Get(map, "ConsultationInfo")
        };
    }

    public async Task<IReadOnlyList<ExpertisePublicResponse>> GetExpertiseAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.ExpertiseItems
            .AsNoTracking()
            .Where(item => item.IsActive)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Title)
            .Select(item => new ExpertisePublicResponse
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                IconKey = item.IconKey,
                SortOrder = item.SortOrder
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CourseSummaryResponse>> GetFeaturedCoursesAsync(CancellationToken cancellationToken = default)
    {
        return await PublishedCourses()
            .Where(course => course.IsFeatured)
            .OrderByDescending(course => course.UpdatedAt)
            .Select(course => new CourseSummaryResponse
            {
                Id = course.Id,
                Title = course.Title,
                Slug = course.Slug,
                ShortDescription = course.ShortDescription,
                PriceIQD = course.PriceIQD,
                ThumbnailUrl = course.ThumbnailUrl,
                Level = course.Level.ToString()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResponse<CourseSummaryResponse>> GetCoursesAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var (safePage, safeSize) = NormalizePage(page, pageSize);
        var query = PublishedCourses();
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(course => course.UpdatedAt)
            .Skip((safePage - 1) * safeSize)
            .Take(safeSize)
            .Select(course => new CourseSummaryResponse
            {
                Id = course.Id,
                Title = course.Title,
                Slug = course.Slug,
                ShortDescription = course.ShortDescription,
                PriceIQD = course.PriceIQD,
                ThumbnailUrl = course.ThumbnailUrl,
                Level = course.Level.ToString()
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<CourseSummaryResponse>
        {
            Items = items,
            Page = safePage,
            PageSize = safeSize,
            TotalCount = total
        };
    }

    public async Task<CourseDetailResponse?> GetCourseBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await PublishedCourses()
            .Where(item => item.Slug == slug)
            .Select(item => new CourseDetailResponse
            {
                Id = item.Id,
                Title = item.Title,
                Slug = item.Slug,
                ShortDescription = item.ShortDescription,
                Description = item.Description,
                PriceIQD = item.PriceIQD,
                ThumbnailUrl = item.ThumbnailUrl,
                TrailerUrl = item.TrailerUrl,
                Level = item.Level.ToString(),
                AccessType = item.AccessType.ToString(),
                AccessDurationDays = item.AccessDurationDays,
                SectionCount = item.Sections.Count(section =>
                    section.Lessons.Any(lesson => lesson.Status == LessonStatus.Published)),
                LessonCount = item.Sections.SelectMany(section => section.Lessons)
                    .Count(lesson => lesson.Status == LessonStatus.Published),
                TotalDurationSeconds = item.Sections.SelectMany(section => section.Lessons)
                    .Where(lesson => lesson.Status == LessonStatus.Published)
                    .Sum(lesson => lesson.DurationSeconds),
                Sections = item.Sections
                    .Where(section => section.Lessons.Any(lesson => lesson.Status == LessonStatus.Published))
                    .OrderBy(section => section.SortOrder)
                    .ThenBy(section => section.Title)
                    .Select(section => new CourseSectionPublicResponse
                    {
                        Id = section.Id,
                        Title = section.Title,
                        Description = section.Description,
                        SortOrder = section.SortOrder,
                        LessonCount = section.Lessons.Count(lesson => lesson.Status == LessonStatus.Published),
                        TotalDurationSeconds = section.Lessons
                            .Where(lesson => lesson.Status == LessonStatus.Published)
                            .Sum(lesson => lesson.DurationSeconds),
                        Lessons = section.Lessons
                            .Where(lesson => lesson.Status == LessonStatus.Published)
                            .OrderBy(lesson => lesson.SortOrder)
                            .ThenBy(lesson => lesson.Title)
                            .Select(lesson => new LessonSummaryPublicResponse
                            {
                                Id = lesson.Id,
                                Title = lesson.Title,
                                DurationSeconds = lesson.DurationSeconds,
                                SortOrder = lesson.SortOrder,
                                IsFreePreview = lesson.IsFreePreview,
                                Status = lesson.Status.ToString()
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StatisticPublicResponse>> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.SiteStatistics
            .AsNoTracking()
            .Where(item => item.IsActive && !item.IsPlaceholder)
            .OrderBy(item => item.SortOrder)
            .Select(item => new StatisticPublicResponse
            {
                Id = item.Id,
                Key = item.Key,
                Label = item.Label,
                DisplayValue = item.DisplayValue,
                SortOrder = item.SortOrder
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TestimonialPublicResponse>> GetTestimonialsAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Testimonials
            .AsNoTracking()
            .Where(item => item.IsPublished)
            .OrderBy(item => item.SortOrder)
            .Select(item => new TestimonialPublicResponse
            {
                Id = item.Id,
                AuthorDisplayName = item.AuthorDisplayName,
                AuthorTitle = item.AuthorTitle,
                Body = item.Body,
                SortOrder = item.SortOrder
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ArticleSummaryResponse>> GetLatestArticlesAsync(
        int take,
        CancellationToken cancellationToken = default)
    {
        var limit = take <= 0 ? 3 : Math.Min(take, 12);
        return await PublishedArticles()
            .OrderByDescending(article => article.PublishedAt)
            .Take(limit)
            .Select(article => new ArticleSummaryResponse
            {
                Id = article.Id,
                Title = article.Title,
                Slug = article.Slug,
                Excerpt = article.Excerpt,
                CoverImage = article.CoverImage,
                PublishedAt = article.PublishedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResponse<ArticleSummaryResponse>> GetArticlesAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var (safePage, safeSize) = NormalizePage(page, pageSize);
        var query = PublishedArticles();
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(article => article.PublishedAt)
            .Skip((safePage - 1) * safeSize)
            .Take(safeSize)
            .Select(article => new ArticleSummaryResponse
            {
                Id = article.Id,
                Title = article.Title,
                Slug = article.Slug,
                Excerpt = article.Excerpt,
                CoverImage = article.CoverImage,
                PublishedAt = article.PublishedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<ArticleSummaryResponse>
        {
            Items = items,
            Page = safePage,
            PageSize = safeSize,
            TotalCount = total
        };
    }

    public Task<ArticleDetailResponse?> GetArticleBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return PublishedArticles()
            .Where(article => article.Slug == slug)
            .Select(article => new ArticleDetailResponse
            {
                Id = article.Id,
                Title = article.Title,
                Slug = article.Slug,
                Excerpt = article.Excerpt,
                Content = article.Content,
                CoverImage = article.CoverImage,
                PublishedAt = article.PublishedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FaqPublicResponse>> GetFaqAsync(
        bool homeOnly,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.FaqItems.AsNoTracking().Where(item => item.IsActive);
        if (homeOnly)
        {
            query = query.Where(item => item.ShowOnHome);
        }

        return await query
            .OrderBy(item => item.SortOrder)
            .Select(item => new FaqPublicResponse
            {
                Id = item.Id,
                Question = item.Question,
                Answer = item.Answer,
                SortOrder = item.SortOrder
            })
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Domain.Entities.Course> PublishedCourses() =>
        dbContext.Courses.AsNoTracking().Where(course => course.Status == CourseStatus.Published);

    private IQueryable<Domain.Entities.Article> PublishedArticles()
    {
        var now = DateTimeOffset.UtcNow;
        return dbContext.Articles.AsNoTracking()
            .Where(article =>
                article.Status == ContentStatus.Published &&
                article.PublishedAt != null &&
                article.PublishedAt <= now);
    }

    private static (int Page, int PageSize) NormalizePage(int page, int pageSize)
    {
        var safePage = page < 1 ? 1 : page;
        var safeSize = pageSize < 1 ? 12 : Math.Min(pageSize, 24);
        return (safePage, safeSize);
    }

    private static string? Get(IReadOnlyDictionary<string, string?> map, string key) =>
        map.TryGetValue(key, out var value) ? value : null;

    private static string? ReadPublicJson(string valueJson)
    {
        try
        {
            using var document = JsonDocument.Parse(valueJson);
            return document.RootElement.ValueKind switch
            {
                JsonValueKind.String => document.RootElement.GetString(),
                JsonValueKind.Object or JsonValueKind.Array => document.RootElement.GetRawText(),
                JsonValueKind.Null => null,
                _ => document.RootElement.ToString()
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
