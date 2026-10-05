using MohammedRaouf.Application.Catalog;
using MohammedRaouf.Application.Courses;
using MohammedRaouf.Contracts.Public;
using Microsoft.Net.Http.Headers;

namespace MohammedRaouf.Api.Endpoints;

public static class PublicEndpoints
{
    public static IEndpointRouteBuilder MapPublicEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/public")
            .WithTags("Public")
            .AllowAnonymous()
            .AddEndpointFilter(SetPublicCacheHeaders);

        group.MapGet("/site-settings", GetSiteSettingsAsync);
        group.MapGet("/expertise", GetExpertiseAsync);
        group.MapGet("/courses/featured", GetFeaturedCoursesAsync);
        group.MapGet("/courses", GetCoursesAsync);
        group.MapGet("/courses/{slug}", GetCourseBySlugAsync);
        group.MapGet("/statistics", GetStatisticsAsync);
        group.MapGet("/testimonials", GetTestimonialsAsync);
        group.MapGet("/articles/latest", GetLatestArticlesAsync);
        group.MapGet("/articles", GetArticlesAsync);
        group.MapGet("/articles/{slug}", GetArticleBySlugAsync);
        group.MapGet("/faq", GetFaqAsync);
        group.MapGet("/lessons/{lessonId:guid}/preview", GetLessonPreviewAsync);

        return endpoints;
    }

    private static async ValueTask<object?> SetPublicCacheHeaders(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        context.HttpContext.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
        {
            Public = true,
            MaxAge = TimeSpan.FromMinutes(1)
        };

        return await next(context);
    }

    private static Task<PublicSiteSettingsResponse> GetSiteSettingsAsync(
        IPublicCatalogService catalog,
        CancellationToken cancellationToken) =>
        catalog.GetSiteSettingsAsync(cancellationToken);

    private static Task<IReadOnlyList<ExpertisePublicResponse>> GetExpertiseAsync(
        IPublicCatalogService catalog,
        CancellationToken cancellationToken) =>
        catalog.GetExpertiseAsync(cancellationToken);

    private static Task<IReadOnlyList<CourseSummaryResponse>> GetFeaturedCoursesAsync(
        IPublicCatalogService catalog,
        CancellationToken cancellationToken) =>
        catalog.GetFeaturedCoursesAsync(cancellationToken);

    private static Task<PagedResponse<CourseSummaryResponse>> GetCoursesAsync(
        IPublicCatalogService catalog,
        int page = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default) =>
        catalog.GetCoursesAsync(page, pageSize, cancellationToken);

    private static async Task<IResult> GetCourseBySlugAsync(
        string slug,
        IPublicCatalogService catalog,
        CancellationToken cancellationToken)
    {
        var course = await catalog.GetCourseBySlugAsync(slug, cancellationToken);
        return course is null ? Results.NotFound() : Results.Ok(course);
    }

    private static Task<IReadOnlyList<StatisticPublicResponse>> GetStatisticsAsync(
        IPublicCatalogService catalog,
        CancellationToken cancellationToken) =>
        catalog.GetStatisticsAsync(cancellationToken);

    private static Task<IReadOnlyList<TestimonialPublicResponse>> GetTestimonialsAsync(
        IPublicCatalogService catalog,
        CancellationToken cancellationToken) =>
        catalog.GetTestimonialsAsync(cancellationToken);

    private static Task<IReadOnlyList<ArticleSummaryResponse>> GetLatestArticlesAsync(
        IPublicCatalogService catalog,
        CancellationToken cancellationToken) =>
        catalog.GetLatestArticlesAsync(3, cancellationToken);

    private static Task<PagedResponse<ArticleSummaryResponse>> GetArticlesAsync(
        IPublicCatalogService catalog,
        int page = 1,
        int pageSize = 12,
        CancellationToken cancellationToken = default) =>
        catalog.GetArticlesAsync(page, pageSize, cancellationToken);

    private static async Task<IResult> GetArticleBySlugAsync(
        string slug,
        IPublicCatalogService catalog,
        CancellationToken cancellationToken)
    {
        var article = await catalog.GetArticleBySlugAsync(slug, cancellationToken);
        return article is null ? Results.NotFound() : Results.Ok(article);
    }

    private static Task<IReadOnlyList<FaqPublicResponse>> GetFaqAsync(
        IPublicCatalogService catalog,
        bool home = false,
        CancellationToken cancellationToken = default) =>
        catalog.GetFaqAsync(home, cancellationToken);

    private static async Task<IResult> GetLessonPreviewAsync(
        Guid lessonId,
        ICourseQueryService queries,
        CancellationToken cancellationToken)
    {
        var preview = await queries.GetPublicPreviewAsync(lessonId, cancellationToken);
        return preview is null ? Results.NotFound() : Results.Ok(preview);
    }
}
