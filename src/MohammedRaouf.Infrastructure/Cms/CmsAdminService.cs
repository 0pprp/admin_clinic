using Microsoft.EntityFrameworkCore;
using MohammedRaouf.Application.Admin;
using MohammedRaouf.Application.Cms;
using MohammedRaouf.Application.Common;
using MohammedRaouf.Application.Courses;
using MohammedRaouf.Contracts.Admin;
using MohammedRaouf.Contracts.Cms;
using MohammedRaouf.Contracts.Public;
using MohammedRaouf.Domain.Entities;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.Infrastructure.Cms;

public sealed class CmsAdminService(
    ApplicationDbContext dbContext,
    IAdminAuditService audit,
    TimeProvider timeProvider) : ICmsAdminService
{
    public async Task<PagedResponse<AdminArticleSummaryResponse>> ListArticlesAsync(
        int page, int pageSize, string? search, string? status, CancellationToken cancellationToken = default)
    {
        var (safePage, safeSize) = Paging.Normalize(page, pageSize);
        var query = dbContext.Articles.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ContentStatus>(status, true, out var parsed))
        {
            query = query.Where(item => item.Status == parsed);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(item => item.Title.ToLower().Contains(term) || item.Slug.ToLower().Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(item => item.UpdatedAt)
            .Skip((safePage - 1) * safeSize)
            .Take(safeSize)
            .Select(item => new AdminArticleSummaryResponse
            {
                Id = item.Id,
                Title = item.Title,
                Slug = item.Slug,
                Status = item.Status.ToString(),
                PublishedAt = item.PublishedAt,
                UpdatedAt = item.UpdatedAt
            })
            .ToListAsync(cancellationToken);
        return new PagedResponse<AdminArticleSummaryResponse> { Items = items, Page = safePage, PageSize = safeSize, TotalCount = total };
    }

    public async Task<AdminArticleDetailResponse?> GetArticleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await dbContext.Articles.AsNoTracking().FirstOrDefaultAsync(row => row.Id == id, cancellationToken);
        return item is null ? null : MapArticle(item);
    }

    public async Task<ActionResult<AdminArticleDetailResponse>> CreateArticleAsync(
        Guid authorId, SaveArticleRequest request, CancellationToken cancellationToken = default)
    {
        var slug = CourseSlug.Normalize(request.Slug);
        if (await dbContext.Articles.AnyAsync(item => item.Slug == slug, cancellationToken))
        {
            return ActionResult<AdminArticleDetailResponse>.Fail(409, "تعارض", "المسار مستخدم مسبقاً.");
        }

        var now = timeProvider.GetUtcNow();
        var entity = new Article
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Slug = slug,
            Excerpt = request.Excerpt.Trim(),
            Content = request.Content.Trim(),
            CoverImage = string.IsNullOrWhiteSpace(request.CoverImage) ? null : request.CoverImage.Trim(),
            Status = ContentStatus.Draft,
            AuthorId = authorId,
            CreatedAt = now,
            UpdatedAt = now
        };
        dbContext.Articles.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<AdminArticleDetailResponse>.Ok(MapArticle(entity), 201);
    }

    public async Task<ActionResult<AdminArticleDetailResponse>> UpdateArticleAsync(
        Guid actorUserId, Guid id, SaveArticleRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Articles.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (entity is null)
        {
            return ActionResult<AdminArticleDetailResponse>.Fail(404, "غير موجود", "المقال غير موجود.");
        }

        var slug = CourseSlug.Normalize(request.Slug);
        if (await dbContext.Articles.AnyAsync(item => item.Slug == slug && item.Id != id, cancellationToken))
        {
            return ActionResult<AdminArticleDetailResponse>.Fail(409, "تعارض", "المسار مستخدم مسبقاً.");
        }

        entity.Title = request.Title.Trim();
        entity.Slug = slug;
        entity.Excerpt = request.Excerpt.Trim();
        entity.Content = request.Content.Trim();
        entity.CoverImage = string.IsNullOrWhiteSpace(request.CoverImage) ? null : request.CoverImage.Trim();
        entity.UpdatedAt = timeProvider.GetUtcNow();
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<AdminArticleDetailResponse>.Ok(MapArticle(entity));
    }

    public async Task<ActionResult<AdminArticleDetailResponse>> PublishArticleAsync(
        Guid actorUserId, Guid id, string? ip, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Articles.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (entity is null)
        {
            return ActionResult<AdminArticleDetailResponse>.Fail(404, "غير موجود", "المقال غير موجود.");
        }

        if (string.IsNullOrWhiteSpace(entity.Title) || string.IsNullOrWhiteSpace(entity.Slug) ||
            string.IsNullOrWhiteSpace(entity.Excerpt) || string.IsNullOrWhiteSpace(entity.Content))
        {
            return ActionResult<AdminArticleDetailResponse>.Fail(400, "تعذر النشر", "أكمل العنوان والمسار والمقتطف والمحتوى قبل النشر.");
        }

        entity.Status = ContentStatus.Published;
        entity.PublishedAt ??= timeProvider.GetUtcNow();
        entity.UpdatedAt = timeProvider.GetUtcNow();
        audit.Add(actorUserId, "ArticlePublished", "Article", entity.Id, $"تم نشر المقال {entity.Title}.", null, ip);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<AdminArticleDetailResponse>.Ok(MapArticle(entity));
    }

    public async Task<ActionResult<AdminArticleDetailResponse>> ArchiveArticleAsync(
        Guid actorUserId, Guid id, string? ip, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Articles.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (entity is null)
        {
            return ActionResult<AdminArticleDetailResponse>.Fail(404, "غير موجود", "المقال غير موجود.");
        }

        entity.Status = ContentStatus.Archived;
        entity.UpdatedAt = timeProvider.GetUtcNow();
        audit.Add(actorUserId, "ArticleArchived", "Article", entity.Id, $"تم أرشفة المقال {entity.Title}.", null, ip);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<AdminArticleDetailResponse>.Ok(MapArticle(entity));
    }

    public async Task<IReadOnlyList<AdminFaqResponse>> ListFaqAsync(CancellationToken cancellationToken = default)
    {
        var rows = await dbContext.FaqItems.AsNoTracking()
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Question)
            .ToListAsync(cancellationToken);
        return rows.Select(MapFaq).ToList();
    }

    public async Task<ActionResult<AdminFaqResponse>> CreateFaqAsync(SaveFaqRequest request, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var max = await dbContext.FaqItems.MaxAsync(item => (int?)item.SortOrder, cancellationToken) ?? 0;
        var entity = new FaqItem
        {
            Id = Guid.NewGuid(),
            Question = request.Question.Trim(),
            Answer = request.Answer.Trim(),
            ShowOnHome = request.ShowOnHome,
            SortOrder = request.SortOrder ?? max + 1,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        dbContext.FaqItems.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<AdminFaqResponse>.Ok(MapFaq(entity), 201);
    }

    public Task<ActionResult<AdminFaqResponse>> UpdateFaqAsync(Guid id, SaveFaqRequest request, CancellationToken cancellationToken = default) =>
        UpdateFaqCoreAsync(id, item =>
        {
            item.Question = request.Question.Trim();
            item.Answer = request.Answer.Trim();
            item.ShowOnHome = request.ShowOnHome;
            if (request.SortOrder is int sort)
            {
                item.SortOrder = sort;
            }
        }, cancellationToken);

    public Task<ActionResult<AdminFaqResponse>> ActivateFaqAsync(Guid id, CancellationToken cancellationToken = default) =>
        UpdateFaqCoreAsync(id, item => item.IsActive = true, cancellationToken);

    public Task<ActionResult<AdminFaqResponse>> DeactivateFaqAsync(Guid id, CancellationToken cancellationToken = default) =>
        UpdateFaqCoreAsync(id, item => item.IsActive = false, cancellationToken);

    public async Task<ActionResult> ReorderFaqAsync(ReorderItemsRequest request, CancellationToken cancellationToken = default)
    {
        var ids = request.Items.Select(item => item.Id).ToList();
        var rows = await dbContext.FaqItems.Where(item => ids.Contains(item.Id)).ToListAsync(cancellationToken);
        if (rows.Count != ids.Count || ids.Distinct().Count() != ids.Count)
        {
            return ActionResult.Fail(400, "غير صالح", "قائمة الترتيب غير مكتملة.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        foreach (var item in request.Items)
        {
            rows.Single(row => row.Id == item.Id).SortOrder = item.SortOrder;
            rows.Single(row => row.Id == item.Id).UpdatedAt = timeProvider.GetUtcNow();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ActionResult.Success();
    }

    public async Task<IReadOnlyList<AdminExpertiseResponse>> ListExpertiseAsync(CancellationToken cancellationToken = default)
    {
        var rows = await dbContext.ExpertiseItems.AsNoTracking()
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Title)
            .ToListAsync(cancellationToken);
        return rows.Select(MapExpertise).ToList();
    }

    public async Task<ActionResult<AdminExpertiseResponse>> CreateExpertiseAsync(SaveExpertiseRequest request, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var max = await dbContext.ExpertiseItems.MaxAsync(item => (int?)item.SortOrder, cancellationToken) ?? 0;
        var entity = new ExpertiseItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            IconKey = string.IsNullOrWhiteSpace(request.IconKey) ? null : request.IconKey.Trim(),
            SortOrder = request.SortOrder ?? max + 1,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        dbContext.ExpertiseItems.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<AdminExpertiseResponse>.Ok(MapExpertise(entity), 201);
    }

    public Task<ActionResult<AdminExpertiseResponse>> UpdateExpertiseAsync(Guid id, SaveExpertiseRequest request, CancellationToken cancellationToken = default) =>
        UpdateExpertiseCoreAsync(id, item =>
        {
            item.Title = request.Title.Trim();
            item.Description = request.Description.Trim();
            item.IconKey = string.IsNullOrWhiteSpace(request.IconKey) ? null : request.IconKey.Trim();
            if (request.SortOrder is int sort)
            {
                item.SortOrder = sort;
            }
        }, cancellationToken);

    public Task<ActionResult<AdminExpertiseResponse>> ActivateExpertiseAsync(Guid id, CancellationToken cancellationToken = default) =>
        UpdateExpertiseCoreAsync(id, item => item.IsActive = true, cancellationToken);

    public Task<ActionResult<AdminExpertiseResponse>> DeactivateExpertiseAsync(Guid id, CancellationToken cancellationToken = default) =>
        UpdateExpertiseCoreAsync(id, item => item.IsActive = false, cancellationToken);

    public async Task<ActionResult> ReorderExpertiseAsync(ReorderItemsRequest request, CancellationToken cancellationToken = default)
    {
        var ids = request.Items.Select(item => item.Id).ToList();
        var rows = await dbContext.ExpertiseItems.Where(item => ids.Contains(item.Id)).ToListAsync(cancellationToken);
        if (rows.Count != ids.Count || ids.Distinct().Count() != ids.Count)
        {
            return ActionResult.Fail(400, "غير صالح", "قائمة الترتيب غير مكتملة.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        foreach (var item in request.Items)
        {
            var row = rows.Single(entry => entry.Id == item.Id);
            row.SortOrder = item.SortOrder;
            row.UpdatedAt = timeProvider.GetUtcNow();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ActionResult.Success();
    }

    public async Task<IReadOnlyList<AdminTestimonialResponse>> ListTestimonialsAsync(CancellationToken cancellationToken = default)
    {
        var rows = await dbContext.Testimonials.AsNoTracking()
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.AuthorDisplayName)
            .ToListAsync(cancellationToken);
        return rows.Select(MapTestimonial).ToList();
    }

    public async Task<ActionResult<AdminTestimonialResponse>> CreateTestimonialAsync(SaveTestimonialRequest request, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        var max = await dbContext.Testimonials.MaxAsync(item => (int?)item.SortOrder, cancellationToken) ?? 0;
        var entity = new Testimonial
        {
            Id = Guid.NewGuid(),
            AuthorDisplayName = request.AuthorDisplayName.Trim(),
            AuthorTitle = string.IsNullOrWhiteSpace(request.AuthorTitle) ? null : request.AuthorTitle.Trim(),
            Body = request.Body.Trim(),
            IsPublished = false,
            SortOrder = request.SortOrder ?? max + 1,
            CreatedAt = now,
            UpdatedAt = now
        };
        dbContext.Testimonials.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<AdminTestimonialResponse>.Ok(MapTestimonial(entity), 201);
    }

    public Task<ActionResult<AdminTestimonialResponse>> UpdateTestimonialAsync(Guid id, SaveTestimonialRequest request, CancellationToken cancellationToken = default) =>
        UpdateTestimonialCoreAsync(id, item =>
        {
            item.AuthorDisplayName = request.AuthorDisplayName.Trim();
            item.AuthorTitle = string.IsNullOrWhiteSpace(request.AuthorTitle) ? null : request.AuthorTitle.Trim();
            item.Body = request.Body.Trim();
            if (request.SortOrder is int sort)
            {
                item.SortOrder = sort;
            }
        }, cancellationToken);

    public Task<ActionResult<AdminTestimonialResponse>> PublishTestimonialAsync(Guid id, CancellationToken cancellationToken = default) =>
        UpdateTestimonialCoreAsync(id, item => item.IsPublished = true, cancellationToken);

    public Task<ActionResult<AdminTestimonialResponse>> UnpublishTestimonialAsync(Guid id, CancellationToken cancellationToken = default) =>
        UpdateTestimonialCoreAsync(id, item => item.IsPublished = false, cancellationToken);

    public async Task<IReadOnlyList<AdminStatisticResponse>> ListStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var rows = await dbContext.SiteStatistics.AsNoTracking()
            .OrderBy(item => item.SortOrder)
            .ToListAsync(cancellationToken);
        return rows.Select(MapStatistic).ToList();
    }

    public async Task<ActionResult<AdminStatisticResponse>> UpdateStatisticAsync(Guid id, SaveStatisticRequest request, CancellationToken cancellationToken = default)
    {
        var item = await dbContext.SiteStatistics.FirstOrDefaultAsync(row => row.Id == id, cancellationToken);
        if (item is null)
        {
            return ActionResult<AdminStatisticResponse>.Fail(404, "غير موجود", "الإحصائية غير موجودة.");
        }

        item.Label = request.Label.Trim();
        item.DisplayValue = request.DisplayValue.Trim();
        item.IsPlaceholder = request.IsPlaceholder;
        item.IsActive = request.IsActive;
        if (request.SortOrder is int sort)
        {
            item.SortOrder = sort;
        }

        item.UpdatedAt = timeProvider.GetUtcNow();
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<AdminStatisticResponse>.Ok(MapStatistic(item));
    }

    private async Task<ActionResult<AdminFaqResponse>> UpdateFaqCoreAsync(Guid id, Action<FaqItem> mutate, CancellationToken cancellationToken)
    {
        var item = await dbContext.FaqItems.FirstOrDefaultAsync(row => row.Id == id, cancellationToken);
        if (item is null)
        {
            return ActionResult<AdminFaqResponse>.Fail(404, "غير موجود", "السؤال غير موجود.");
        }

        mutate(item);
        item.UpdatedAt = timeProvider.GetUtcNow();
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<AdminFaqResponse>.Ok(MapFaq(item));
    }

    private async Task<ActionResult<AdminExpertiseResponse>> UpdateExpertiseCoreAsync(Guid id, Action<ExpertiseItem> mutate, CancellationToken cancellationToken)
    {
        var item = await dbContext.ExpertiseItems.FirstOrDefaultAsync(row => row.Id == id, cancellationToken);
        if (item is null)
        {
            return ActionResult<AdminExpertiseResponse>.Fail(404, "غير موجود", "البند غير موجود.");
        }

        mutate(item);
        item.UpdatedAt = timeProvider.GetUtcNow();
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<AdminExpertiseResponse>.Ok(MapExpertise(item));
    }

    private async Task<ActionResult<AdminTestimonialResponse>> UpdateTestimonialCoreAsync(Guid id, Action<Testimonial> mutate, CancellationToken cancellationToken)
    {
        var item = await dbContext.Testimonials.FirstOrDefaultAsync(row => row.Id == id, cancellationToken);
        if (item is null)
        {
            return ActionResult<AdminTestimonialResponse>.Fail(404, "غير موجود", "الشهادة غير موجودة.");
        }

        mutate(item);
        item.UpdatedAt = timeProvider.GetUtcNow();
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<AdminTestimonialResponse>.Ok(MapTestimonial(item));
    }

    private static AdminArticleDetailResponse MapArticle(Article item) => new()
    {
        Id = item.Id,
        Title = item.Title,
        Slug = item.Slug,
        Excerpt = item.Excerpt,
        Content = item.Content,
        CoverImage = item.CoverImage,
        Status = item.Status.ToString(),
        PublishedAt = item.PublishedAt,
        AuthorId = item.AuthorId,
        CreatedAt = item.CreatedAt,
        UpdatedAt = item.UpdatedAt
    };

    private static AdminFaqResponse MapFaq(FaqItem item) => new()
    {
        Id = item.Id,
        Question = item.Question,
        Answer = item.Answer,
        SortOrder = item.SortOrder,
        IsActive = item.IsActive,
        ShowOnHome = item.ShowOnHome,
        UpdatedAt = item.UpdatedAt
    };

    private static AdminExpertiseResponse MapExpertise(ExpertiseItem item) => new()
    {
        Id = item.Id,
        Title = item.Title,
        Description = item.Description,
        IconKey = item.IconKey,
        SortOrder = item.SortOrder,
        IsActive = item.IsActive,
        UpdatedAt = item.UpdatedAt
    };

    private static AdminTestimonialResponse MapTestimonial(Testimonial item) => new()
    {
        Id = item.Id,
        AuthorDisplayName = item.AuthorDisplayName,
        AuthorTitle = item.AuthorTitle,
        Body = item.Body,
        IsPublished = item.IsPublished,
        SortOrder = item.SortOrder,
        UpdatedAt = item.UpdatedAt
    };

    private static AdminStatisticResponse MapStatistic(SiteStatistic item) => new()
    {
        Id = item.Id,
        Key = item.Key,
        Label = item.Label,
        DisplayValue = item.DisplayValue,
        IsPlaceholder = item.IsPlaceholder,
        SortOrder = item.SortOrder,
        IsActive = item.IsActive,
        UpdatedAt = item.UpdatedAt
    };
}
