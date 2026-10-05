using Microsoft.EntityFrameworkCore;
using MohammedRaouf.Application.Admin;
using MohammedRaouf.Application.Cms;
using MohammedRaouf.Application.Common;
using MohammedRaouf.Contracts.Cms;
using MohammedRaouf.Domain.Entities;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.Infrastructure.Cms;

public sealed class SiteSettingsAdminService(
    ApplicationDbContext dbContext,
    IAdminAuditService audit,
    TimeProvider timeProvider) : ISiteSettingsAdminService
{
    public static readonly string[] ContentKeys =
    [
        "BrandName", "BrandNameEnglish", "PublicPhone", "PublicWhatsApp", "PublicEmail",
        "SocialLinks", "FooterText", "ConsultationInfo"
    ];

    public static readonly string[] PaymentKeys =
    [
        "PaymentMethods", "TransferInstructions", "SupportPhone", "SupportWhatsApp"
    ];

    public async Task<AdminSiteSettingsResponse> GetAsync(bool includePayment, CancellationToken cancellationToken = default)
    {
        var keys = includePayment ? ContentKeys.Concat(PaymentKeys).ToArray() : ContentKeys;
        var rows = await dbContext.SiteSettings.AsNoTracking()
            .Where(item => keys.Contains(item.Key))
            .ToListAsync(cancellationToken);
        string? Read(string key) => ReadValue(rows.FirstOrDefault(item => item.Key == key)?.ValueJson);
        return new AdminSiteSettingsResponse
        {
            BrandName = Read("BrandName"),
            BrandNameEnglish = Read("BrandNameEnglish"),
            PublicPhone = Read("PublicPhone"),
            PublicWhatsApp = Read("PublicWhatsApp"),
            PublicEmail = Read("PublicEmail"),
            SocialLinks = Read("SocialLinks"),
            FooterText = Read("FooterText"),
            ConsultationInfo = Read("ConsultationInfo"),
            PaymentMethods = includePayment ? Read("PaymentMethods") : null,
            TransferInstructions = includePayment ? Read("TransferInstructions") : null,
            SupportPhone = includePayment ? Read("SupportPhone") : null,
            SupportWhatsApp = includePayment ? Read("SupportWhatsApp") : null
        };
    }

    public async Task<ActionResult<AdminSiteSettingsResponse>> UpdateContentAsync(
        Guid actorUserId,
        SaveSiteContentSettingsRequest request,
        string? ip,
        CancellationToken cancellationToken = default)
    {
        await UpsertAsync("BrandName", request.BrandName, actorUserId, cancellationToken);
        await UpsertAsync("BrandNameEnglish", request.BrandNameEnglish, actorUserId, cancellationToken);
        await UpsertAsync("PublicPhone", request.PublicPhone, actorUserId, cancellationToken);
        await UpsertAsync("PublicWhatsApp", request.PublicWhatsApp, actorUserId, cancellationToken);
        await UpsertAsync("PublicEmail", request.PublicEmail, actorUserId, cancellationToken);
        await UpsertAsync("SocialLinks", request.SocialLinks, actorUserId, cancellationToken);
        await UpsertAsync("FooterText", request.FooterText, actorUserId, cancellationToken);
        await UpsertAsync("ConsultationInfo", request.ConsultationInfo, actorUserId, cancellationToken);
        audit.Add(actorUserId, "SiteSettingsUpdated", "SiteSetting", null, "تم تحديث إعدادات المحتوى العامة.", new { group = "content" }, ip);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<AdminSiteSettingsResponse>.Ok(await GetAsync(true, cancellationToken));
    }

    public async Task<ActionResult<AdminSiteSettingsResponse>> UpdatePaymentAsync(
        Guid actorUserId,
        SavePaymentSettingsRequest request,
        string? ip,
        CancellationToken cancellationToken = default)
    {
        await UpsertAsync("PaymentMethods", request.PaymentMethods, actorUserId, cancellationToken);
        await UpsertAsync("TransferInstructions", request.TransferInstructions, actorUserId, cancellationToken);
        await UpsertAsync("SupportPhone", request.SupportPhone, actorUserId, cancellationToken);
        await UpsertAsync("SupportWhatsApp", request.SupportWhatsApp, actorUserId, cancellationToken);
        audit.Add(actorUserId, "SiteSettingsUpdated", "SiteSetting", null, "تم تحديث تعليمات الدفع.", new { group = "payment" }, ip);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<AdminSiteSettingsResponse>.Ok(await GetAsync(true, cancellationToken));
    }

    private async Task UpsertAsync(string key, string? value, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.SiteSettings.FirstOrDefaultAsync(item => item.Key == key, cancellationToken);
        var stored = System.Text.Json.JsonSerializer.Serialize(value ?? string.Empty);
        if (entity is null)
        {
            dbContext.SiteSettings.Add(new SiteSetting
            {
                Id = Guid.NewGuid(),
                Key = key,
                ValueJson = stored,
                UpdatedAt = timeProvider.GetUtcNow(),
                UpdatedBy = actorUserId
            });
            return;
        }

        entity.ValueJson = stored;
        entity.UpdatedAt = timeProvider.GetUtcNow();
        entity.UpdatedBy = actorUserId;
    }

    private static string? ReadValue(string? json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "{}")
        {
            return null;
        }

        var trimmed = json.Trim();
        if (trimmed.Length >= 2 && trimmed.StartsWith('"') && trimmed.EndsWith('"'))
        {
            return System.Text.Json.JsonSerializer.Deserialize<string>(trimmed);
        }

        return trimmed;
    }
}
