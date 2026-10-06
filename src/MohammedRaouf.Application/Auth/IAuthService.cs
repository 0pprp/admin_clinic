using MohammedRaouf.Contracts.Auth;
using MohammedRaouf.Contracts.Profile;

namespace MohammedRaouf.Application.Auth;

public interface IAuthService
{
    Task<AuthCommandResult<UserSummaryResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthCommandResult<UserSummaryResponse>> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<AuthCommandResult<AuthChallengeResponse>> GoogleLoginAsync(
        GoogleLoginRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<AuthCommandResult<UserSummaryResponse>> RefreshAsync(
        string? refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(string? refreshToken, string? ipAddress, CancellationToken cancellationToken = default);

    Task LogoutAllAsync(Guid userId, string? ipAddress, CancellationToken cancellationToken = default);

    Task<UserSummaryResponse> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default);

    Task<AuthCommandResult> ResetPasswordAsync(
        ResetPasswordRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task ResendVerificationAsync(string email, string? purpose = null, CancellationToken cancellationToken = default);

    Task<AuthCommandResult<UserSummaryResponse>> VerifyEmailAsync(
        VerifyEmailRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<UserSummaryResponse> UpdateProfileAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthCommandResult> ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);
}
