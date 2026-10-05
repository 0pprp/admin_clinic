using MohammedRaouf.Contracts.Public;

namespace MohammedRaouf.Application.Catalog;

public interface IPublicCatalogService
{
    Task<PublicSiteSettingsResponse> GetSiteSettingsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExpertisePublicResponse>> GetExpertiseAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CourseSummaryResponse>> GetFeaturedCoursesAsync(CancellationToken cancellationToken = default);

    Task<PagedResponse<CourseSummaryResponse>> GetCoursesAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<CourseDetailResponse?> GetCourseBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StatisticPublicResponse>> GetStatisticsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TestimonialPublicResponse>> GetTestimonialsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ArticleSummaryResponse>> GetLatestArticlesAsync(
        int take,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<ArticleSummaryResponse>> GetArticlesAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ArticleDetailResponse?> GetArticleBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FaqPublicResponse>> GetFaqAsync(
        bool homeOnly,
        CancellationToken cancellationToken = default);
}
