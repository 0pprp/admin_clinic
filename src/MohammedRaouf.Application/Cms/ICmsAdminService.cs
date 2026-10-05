using MohammedRaouf.Application.Common;
using MohammedRaouf.Contracts.Admin;
using MohammedRaouf.Contracts.Cms;
using MohammedRaouf.Contracts.Public;

namespace MohammedRaouf.Application.Cms;

public interface ICmsAdminService
{
    Task<PagedResponse<AdminArticleSummaryResponse>> ListArticlesAsync(int page, int pageSize, string? search, string? status, CancellationToken cancellationToken = default);
    Task<AdminArticleDetailResponse?> GetArticleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ActionResult<AdminArticleDetailResponse>> CreateArticleAsync(Guid authorId, SaveArticleRequest request, CancellationToken cancellationToken = default);
    Task<ActionResult<AdminArticleDetailResponse>> UpdateArticleAsync(Guid actorUserId, Guid id, SaveArticleRequest request, CancellationToken cancellationToken = default);
    Task<ActionResult<AdminArticleDetailResponse>> PublishArticleAsync(Guid actorUserId, Guid id, string? ip, CancellationToken cancellationToken = default);
    Task<ActionResult<AdminArticleDetailResponse>> ArchiveArticleAsync(Guid actorUserId, Guid id, string? ip, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminFaqResponse>> ListFaqAsync(CancellationToken cancellationToken = default);
    Task<ActionResult<AdminFaqResponse>> CreateFaqAsync(SaveFaqRequest request, CancellationToken cancellationToken = default);
    Task<ActionResult<AdminFaqResponse>> UpdateFaqAsync(Guid id, SaveFaqRequest request, CancellationToken cancellationToken = default);
    Task<ActionResult<AdminFaqResponse>> ActivateFaqAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ActionResult<AdminFaqResponse>> DeactivateFaqAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ActionResult> ReorderFaqAsync(ReorderItemsRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminExpertiseResponse>> ListExpertiseAsync(CancellationToken cancellationToken = default);
    Task<ActionResult<AdminExpertiseResponse>> CreateExpertiseAsync(SaveExpertiseRequest request, CancellationToken cancellationToken = default);
    Task<ActionResult<AdminExpertiseResponse>> UpdateExpertiseAsync(Guid id, SaveExpertiseRequest request, CancellationToken cancellationToken = default);
    Task<ActionResult<AdminExpertiseResponse>> ActivateExpertiseAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ActionResult<AdminExpertiseResponse>> DeactivateExpertiseAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ActionResult> ReorderExpertiseAsync(ReorderItemsRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminTestimonialResponse>> ListTestimonialsAsync(CancellationToken cancellationToken = default);
    Task<ActionResult<AdminTestimonialResponse>> CreateTestimonialAsync(SaveTestimonialRequest request, CancellationToken cancellationToken = default);
    Task<ActionResult<AdminTestimonialResponse>> UpdateTestimonialAsync(Guid id, SaveTestimonialRequest request, CancellationToken cancellationToken = default);
    Task<ActionResult<AdminTestimonialResponse>> PublishTestimonialAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ActionResult<AdminTestimonialResponse>> UnpublishTestimonialAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminStatisticResponse>> ListStatisticsAsync(CancellationToken cancellationToken = default);
    Task<ActionResult<AdminStatisticResponse>> UpdateStatisticAsync(Guid id, SaveStatisticRequest request, CancellationToken cancellationToken = default);
}

public interface ISiteSettingsAdminService
{
    Task<AdminSiteSettingsResponse> GetAsync(bool includePayment, CancellationToken cancellationToken = default);

    Task<ActionResult<AdminSiteSettingsResponse>> UpdateContentAsync(
        Guid actorUserId,
        SaveSiteContentSettingsRequest request,
        string? ip,
        CancellationToken cancellationToken = default);

    Task<ActionResult<AdminSiteSettingsResponse>> UpdatePaymentAsync(
        Guid actorUserId,
        SavePaymentSettingsRequest request,
        string? ip,
        CancellationToken cancellationToken = default);
}
