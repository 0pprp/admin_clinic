using System.Security.Claims;
using FluentValidation;
using MohammedRaouf.Api.Security;
using MohammedRaouf.Application.Authorization;
using MohammedRaouf.Application.Cms;
using MohammedRaouf.Contracts.Admin;
using MohammedRaouf.Contracts.Cms;
using MohammedRaouf.Contracts.Public;

namespace MohammedRaouf.Api.Endpoints;

public static class AdminCmsEndpoints
{
    public static IEndpointRouteBuilder MapAdminCmsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var content = endpoints.MapGroup("/api/admin")
            .WithTags("AdminCms")
            .RequireAuthorization(AuthorizationPolicies.ManageContent);

        content.MapGet("/articles", ListArticlesAsync);
        content.MapPost("/articles", CreateArticleAsync);
        content.MapGet("/articles/{id:guid}", GetArticleAsync);
        content.MapPut("/articles/{id:guid}", UpdateArticleAsync);
        content.MapPost("/articles/{id:guid}/publish", PublishArticleAsync);
        content.MapPost("/articles/{id:guid}/archive", ArchiveArticleAsync);

        content.MapGet("/faq", ListFaqAsync);
        content.MapPost("/faq", CreateFaqAsync);
        content.MapPut("/faq/{id:guid}", UpdateFaqAsync);
        content.MapPost("/faq/{id:guid}/activate", ActivateFaqAsync);
        content.MapPost("/faq/{id:guid}/deactivate", DeactivateFaqAsync);
        content.MapPost("/faq/reorder", ReorderFaqAsync);

        content.MapGet("/expertise", ListExpertiseAsync);
        content.MapPost("/expertise", CreateExpertiseAsync);
        content.MapPut("/expertise/{id:guid}", UpdateExpertiseAsync);
        content.MapPost("/expertise/{id:guid}/activate", ActivateExpertiseAsync);
        content.MapPost("/expertise/{id:guid}/deactivate", DeactivateExpertiseAsync);
        content.MapPost("/expertise/reorder", ReorderExpertiseAsync);

        content.MapGet("/testimonials", ListTestimonialsAsync);
        content.MapPost("/testimonials", CreateTestimonialAsync);
        content.MapPut("/testimonials/{id:guid}", UpdateTestimonialAsync);
        content.MapPost("/testimonials/{id:guid}/publish", PublishTestimonialAsync);
        content.MapPost("/testimonials/{id:guid}/unpublish", UnpublishTestimonialAsync);

        content.MapGet("/statistics", ListStatisticsAsync);
        content.MapPut("/statistics/{id:guid}", UpdateStatisticAsync);

        endpoints.MapGet("/api/admin/settings", GetSettingsAsync)
            .WithTags("AdminSettings")
            .RequireAuthorization(AuthorizationPolicies.AccessAdminPanel);

        endpoints.MapPut("/api/admin/settings/content", UpdateContentSettingsAsync)
            .WithTags("AdminSettings")
            .RequireAuthorization(AuthorizationPolicies.ManageContent);

        endpoints.MapPut("/api/admin/settings/payment", UpdatePaymentSettingsAsync)
            .WithTags("AdminSettings")
            .RequireAuthorization(AuthorizationPolicies.ManageBusinessSettings);

        return endpoints;
    }

    private static Task<PagedResponse<AdminArticleSummaryResponse>> ListArticlesAsync(
        ICmsAdminService cms,
        int page = 1,
        int pageSize = 12,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default) =>
        cms.ListArticlesAsync(page, pageSize, search, status, cancellationToken);

    private static async Task<IResult> GetArticleAsync(Guid id, ICmsAdminService cms, CancellationToken cancellationToken)
    {
        var item = await cms.GetArticleAsync(id, cancellationToken);
        return item is null ? Results.NotFound() : Results.Ok(item);
    }

    private static async Task<IResult> CreateArticleAsync(
        SaveArticleRequest request,
        ClaimsPrincipal user,
        IValidator<SaveArticleRequest> validator,
        ICmsAdminService cms,
        CancellationToken cancellationToken)
    {
        var invalid = await EndpointHttp.ValidateAsync(validator, request);
        if (invalid is not null)
        {
            return invalid;
        }

        var userId = user.GetUserId();
        return userId is null
            ? Results.Unauthorized()
            : EndpointHttp.ToHttp(await cms.CreateArticleAsync(userId.Value, request, cancellationToken));
    }

    private static async Task<IResult> UpdateArticleAsync(
        Guid id,
        SaveArticleRequest request,
        ClaimsPrincipal user,
        IValidator<SaveArticleRequest> validator,
        ICmsAdminService cms,
        CancellationToken cancellationToken)
    {
        var invalid = await EndpointHttp.ValidateAsync(validator, request);
        if (invalid is not null)
        {
            return invalid;
        }

        var userId = user.GetUserId();
        return userId is null
            ? Results.Unauthorized()
            : EndpointHttp.ToHttp(await cms.UpdateArticleAsync(userId.Value, id, request, cancellationToken));
    }

    private static async Task<IResult> PublishArticleAsync(
        Guid id,
        ClaimsPrincipal user,
        HttpContext httpContext,
        ICmsAdminService cms,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        return userId is null
            ? Results.Unauthorized()
            : EndpointHttp.ToHttp(await cms.PublishArticleAsync(
                userId.Value, id, httpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken));
    }

    private static async Task<IResult> ArchiveArticleAsync(
        Guid id,
        ClaimsPrincipal user,
        HttpContext httpContext,
        ICmsAdminService cms,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        return userId is null
            ? Results.Unauthorized()
            : EndpointHttp.ToHttp(await cms.ArchiveArticleAsync(
                userId.Value, id, httpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken));
    }

    private static Task<IReadOnlyList<AdminFaqResponse>> ListFaqAsync(ICmsAdminService cms, CancellationToken cancellationToken) =>
        cms.ListFaqAsync(cancellationToken);

    private static async Task<IResult> CreateFaqAsync(
        SaveFaqRequest request, IValidator<SaveFaqRequest> validator, ICmsAdminService cms, CancellationToken cancellationToken)
    {
        var invalid = await EndpointHttp.ValidateAsync(validator, request);
        return invalid ?? EndpointHttp.ToHttp(await cms.CreateFaqAsync(request, cancellationToken));
    }

    private static async Task<IResult> UpdateFaqAsync(
        Guid id, SaveFaqRequest request, IValidator<SaveFaqRequest> validator, ICmsAdminService cms, CancellationToken cancellationToken)
    {
        var invalid = await EndpointHttp.ValidateAsync(validator, request);
        return invalid ?? EndpointHttp.ToHttp(await cms.UpdateFaqAsync(id, request, cancellationToken));
    }

    private static async Task<IResult> ActivateFaqAsync(Guid id, ICmsAdminService cms, CancellationToken cancellationToken) =>
        EndpointHttp.ToHttp(await cms.ActivateFaqAsync(id, cancellationToken));

    private static async Task<IResult> DeactivateFaqAsync(Guid id, ICmsAdminService cms, CancellationToken cancellationToken) =>
        EndpointHttp.ToHttp(await cms.DeactivateFaqAsync(id, cancellationToken));

    private static async Task<IResult> ReorderFaqAsync(
        ReorderItemsRequest request, IValidator<ReorderItemsRequest> validator, ICmsAdminService cms, CancellationToken cancellationToken)
    {
        var invalid = await EndpointHttp.ValidateAsync(validator, request);
        return invalid ?? EndpointHttp.ToHttp(await cms.ReorderFaqAsync(request, cancellationToken));
    }

    private static Task<IReadOnlyList<AdminExpertiseResponse>> ListExpertiseAsync(ICmsAdminService cms, CancellationToken cancellationToken) =>
        cms.ListExpertiseAsync(cancellationToken);

    private static async Task<IResult> CreateExpertiseAsync(
        SaveExpertiseRequest request, IValidator<SaveExpertiseRequest> validator, ICmsAdminService cms, CancellationToken cancellationToken)
    {
        var invalid = await EndpointHttp.ValidateAsync(validator, request);
        return invalid ?? EndpointHttp.ToHttp(await cms.CreateExpertiseAsync(request, cancellationToken));
    }

    private static async Task<IResult> UpdateExpertiseAsync(
        Guid id, SaveExpertiseRequest request, IValidator<SaveExpertiseRequest> validator, ICmsAdminService cms, CancellationToken cancellationToken)
    {
        var invalid = await EndpointHttp.ValidateAsync(validator, request);
        return invalid ?? EndpointHttp.ToHttp(await cms.UpdateExpertiseAsync(id, request, cancellationToken));
    }

    private static async Task<IResult> ActivateExpertiseAsync(Guid id, ICmsAdminService cms, CancellationToken cancellationToken) =>
        EndpointHttp.ToHttp(await cms.ActivateExpertiseAsync(id, cancellationToken));

    private static async Task<IResult> DeactivateExpertiseAsync(Guid id, ICmsAdminService cms, CancellationToken cancellationToken) =>
        EndpointHttp.ToHttp(await cms.DeactivateExpertiseAsync(id, cancellationToken));

    private static async Task<IResult> ReorderExpertiseAsync(
        ReorderItemsRequest request, IValidator<ReorderItemsRequest> validator, ICmsAdminService cms, CancellationToken cancellationToken)
    {
        var invalid = await EndpointHttp.ValidateAsync(validator, request);
        return invalid ?? EndpointHttp.ToHttp(await cms.ReorderExpertiseAsync(request, cancellationToken));
    }

    private static Task<IReadOnlyList<AdminTestimonialResponse>> ListTestimonialsAsync(ICmsAdminService cms, CancellationToken cancellationToken) =>
        cms.ListTestimonialsAsync(cancellationToken);

    private static async Task<IResult> CreateTestimonialAsync(
        SaveTestimonialRequest request, IValidator<SaveTestimonialRequest> validator, ICmsAdminService cms, CancellationToken cancellationToken)
    {
        var invalid = await EndpointHttp.ValidateAsync(validator, request);
        return invalid ?? EndpointHttp.ToHttp(await cms.CreateTestimonialAsync(request, cancellationToken));
    }

    private static async Task<IResult> UpdateTestimonialAsync(
        Guid id, SaveTestimonialRequest request, IValidator<SaveTestimonialRequest> validator, ICmsAdminService cms, CancellationToken cancellationToken)
    {
        var invalid = await EndpointHttp.ValidateAsync(validator, request);
        return invalid ?? EndpointHttp.ToHttp(await cms.UpdateTestimonialAsync(id, request, cancellationToken));
    }

    private static async Task<IResult> PublishTestimonialAsync(Guid id, ICmsAdminService cms, CancellationToken cancellationToken) =>
        EndpointHttp.ToHttp(await cms.PublishTestimonialAsync(id, cancellationToken));

    private static async Task<IResult> UnpublishTestimonialAsync(Guid id, ICmsAdminService cms, CancellationToken cancellationToken) =>
        EndpointHttp.ToHttp(await cms.UnpublishTestimonialAsync(id, cancellationToken));

    private static Task<IReadOnlyList<AdminStatisticResponse>> ListStatisticsAsync(ICmsAdminService cms, CancellationToken cancellationToken) =>
        cms.ListStatisticsAsync(cancellationToken);

    private static async Task<IResult> UpdateStatisticAsync(
        Guid id, SaveStatisticRequest request, IValidator<SaveStatisticRequest> validator, ICmsAdminService cms, CancellationToken cancellationToken)
    {
        var invalid = await EndpointHttp.ValidateAsync(validator, request);
        return invalid ?? EndpointHttp.ToHttp(await cms.UpdateStatisticAsync(id, request, cancellationToken));
    }

    private static Task<AdminSiteSettingsResponse> GetSettingsAsync(
        ClaimsPrincipal user,
        ISiteSettingsAdminService settings,
        CancellationToken cancellationToken)
    {
        var includePayment = user.IsInRole(MohammedRaouf.Domain.Identity.RoleNames.Admin);
        return settings.GetAsync(includePayment, cancellationToken);
    }

    private static async Task<IResult> UpdateContentSettingsAsync(
        SaveSiteContentSettingsRequest request,
        ClaimsPrincipal user,
        HttpContext httpContext,
        IValidator<SaveSiteContentSettingsRequest> validator,
        ISiteSettingsAdminService settings,
        CancellationToken cancellationToken)
    {
        var invalid = await EndpointHttp.ValidateAsync(validator, request);
        if (invalid is not null)
        {
            return invalid;
        }

        var userId = user.GetUserId();
        return userId is null
            ? Results.Unauthorized()
            : EndpointHttp.ToHttp(await settings.UpdateContentAsync(
                userId.Value, request, httpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken));
    }

    private static async Task<IResult> UpdatePaymentSettingsAsync(
        SavePaymentSettingsRequest request,
        ClaimsPrincipal user,
        HttpContext httpContext,
        IValidator<SavePaymentSettingsRequest> validator,
        ISiteSettingsAdminService settings,
        CancellationToken cancellationToken)
    {
        var invalid = await EndpointHttp.ValidateAsync(validator, request);
        if (invalid is not null)
        {
            return invalid;
        }

        var userId = user.GetUserId();
        return userId is null
            ? Results.Unauthorized()
            : EndpointHttp.ToHttp(await settings.UpdatePaymentAsync(
                userId.Value, request, httpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken));
    }
}
