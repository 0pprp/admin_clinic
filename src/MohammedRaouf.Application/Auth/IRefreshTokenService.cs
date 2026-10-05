using MohammedRaouf.Domain.Entities;
using MohammedRaouf.Domain.Identity;

namespace MohammedRaouf.Application.Auth;

public interface IRefreshTokenService
{
    Task<(string RawToken, RefreshToken Entity)> IssueAsync(
        ApplicationUser user,
        Guid familyId,
        DateTimeOffset expiresAt,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<RefreshToken?> FindActiveByRawTokenAsync(string rawToken, CancellationToken cancellationToken = default);

    Task<RefreshToken?> FindByRawTokenAsync(string rawToken, CancellationToken cancellationToken = default);

    Task RevokeAsync(RefreshToken token, string? ipAddress, Guid? replacedByTokenId, CancellationToken cancellationToken = default);

    Task RevokeFamilyAsync(Guid familyId, string? ipAddress, CancellationToken cancellationToken = default);

    Task RevokeAllForUserAsync(Guid userId, string? ipAddress, CancellationToken cancellationToken = default);
}
