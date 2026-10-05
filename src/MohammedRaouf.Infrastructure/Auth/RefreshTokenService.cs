using Microsoft.EntityFrameworkCore;
using MohammedRaouf.Application.Auth;
using MohammedRaouf.Domain.Entities;
using MohammedRaouf.Domain.Identity;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.Infrastructure.Auth;

public sealed class RefreshTokenService(ApplicationDbContext dbContext) : IRefreshTokenService
{
    public async Task<(string RawToken, RefreshToken Entity)> IssueAsync(
        ApplicationUser user,
        Guid familyId,
        DateTimeOffset expiresAt,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var rawToken = TokenHashing.CreateRawToken();
        var entity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = TokenHashing.Hash(rawToken),
            FamilyId = familyId,
            ExpiresAt = expiresAt,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByIp = ipAddress
        };

        dbContext.RefreshTokens.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return (rawToken, entity);
    }

    public async Task<RefreshToken?> FindActiveByRawTokenAsync(string rawToken, CancellationToken cancellationToken = default)
    {
        var token = await FindByRawTokenAsync(rawToken, cancellationToken);
        if (token is null || token.RevokedAt is not null || token.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return null;
        }

        return token;
    }

    public Task<RefreshToken?> FindByRawTokenAsync(string rawToken, CancellationToken cancellationToken = default)
    {
        var hash = TokenHashing.Hash(rawToken);
        return dbContext.RefreshTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(token => token.TokenHash == hash, cancellationToken);
    }

    public async Task RevokeAsync(
        RefreshToken token,
        string? ipAddress,
        Guid? replacedByTokenId,
        CancellationToken cancellationToken = default)
    {
        token.RevokedAt = DateTimeOffset.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReplacedByTokenId = replacedByTokenId;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeFamilyAsync(Guid familyId, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var tokens = await dbContext.RefreshTokens
            .Where(token => token.FamilyId == familyId && token.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTimeOffset.UtcNow;
            token.RevokedByIp = ipAddress;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAllForUserAsync(Guid userId, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var tokens = await dbContext.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTimeOffset.UtcNow;
            token.RevokedByIp = ipAddress;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
