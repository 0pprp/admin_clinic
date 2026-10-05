namespace MohammedRaouf.Contracts.Cms;

public sealed class SaveArticleRequest
{
    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Excerpt { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? CoverImage { get; set; }
}

public sealed class AdminArticleSummaryResponse
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Slug { get; init; }

    public required string Status { get; init; }

    public DateTimeOffset? PublishedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }
}

public sealed class AdminArticleDetailResponse
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Slug { get; init; }

    public required string Excerpt { get; init; }

    public required string Content { get; init; }

    public string? CoverImage { get; init; }

    public required string Status { get; init; }

    public DateTimeOffset? PublishedAt { get; init; }

    public required Guid AuthorId { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }
}

public sealed class SaveFaqRequest
{
    public string Question { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    public bool ShowOnHome { get; set; }

    public int? SortOrder { get; set; }
}

public sealed class AdminFaqResponse
{
    public required Guid Id { get; init; }

    public required string Question { get; init; }

    public required string Answer { get; init; }

    public required int SortOrder { get; init; }

    public required bool IsActive { get; init; }

    public required bool ShowOnHome { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }
}

public sealed class SaveExpertiseRequest
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? IconKey { get; set; }

    public int? SortOrder { get; set; }
}

public sealed class AdminExpertiseResponse
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public string? IconKey { get; init; }

    public required int SortOrder { get; init; }

    public required bool IsActive { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }
}

public sealed class SaveTestimonialRequest
{
    public string AuthorDisplayName { get; set; } = string.Empty;

    public string? AuthorTitle { get; set; }

    public string Body { get; set; } = string.Empty;

    public int? SortOrder { get; set; }
}

public sealed class AdminTestimonialResponse
{
    public required Guid Id { get; init; }

    public required string AuthorDisplayName { get; init; }

    public string? AuthorTitle { get; init; }

    public required string Body { get; init; }

    public required bool IsPublished { get; init; }

    public required int SortOrder { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }
}

public sealed class SaveStatisticRequest
{
    public string Label { get; set; } = string.Empty;

    public string DisplayValue { get; set; } = string.Empty;

    public bool IsPlaceholder { get; set; } = true;

    public int? SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class AdminStatisticResponse
{
    public required Guid Id { get; init; }

    public required string Key { get; init; }

    public required string Label { get; init; }

    public required string DisplayValue { get; init; }

    public required bool IsPlaceholder { get; init; }

    public required int SortOrder { get; init; }

    public required bool IsActive { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }
}

public sealed class SaveSiteContentSettingsRequest
{
    public string? BrandName { get; set; }

    public string? BrandNameEnglish { get; set; }

    public string? PublicPhone { get; set; }

    public string? PublicWhatsApp { get; set; }

    public string? PublicEmail { get; set; }

    public string? SocialLinks { get; set; }

    public string? FooterText { get; set; }

    public string? ConsultationInfo { get; set; }
}

public sealed class SavePaymentSettingsRequest
{
    public string? PaymentMethods { get; set; }

    public string? TransferInstructions { get; set; }

    public string? SupportPhone { get; set; }

    public string? SupportWhatsApp { get; set; }
}

public sealed class AdminSiteSettingsResponse
{
    public string? BrandName { get; init; }

    public string? BrandNameEnglish { get; init; }

    public string? PublicPhone { get; init; }

    public string? PublicWhatsApp { get; init; }

    public string? PublicEmail { get; init; }

    public string? SocialLinks { get; init; }

    public string? FooterText { get; init; }

    public string? ConsultationInfo { get; init; }

    public string? PaymentMethods { get; init; }

    public string? TransferInstructions { get; init; }

    public string? SupportPhone { get; init; }

    public string? SupportWhatsApp { get; init; }
}
