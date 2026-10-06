using System.Text;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MohammedRaouf.Application.Auth;
using MohammedRaouf.Application.Notifications;
using MohammedRaouf.Application.Security;
using EmailOptions = MohammedRaouf.Application.Security.EmailOptions;
using MohammedRaouf.Contracts.Auth;
using MohammedRaouf.Contracts.Profile;
using MohammedRaouf.Domain.Entities;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Domain.Identity;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.Infrastructure.Auth;

public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    IAccessTokenService accessTokenService,
    IRefreshTokenService refreshTokenService,
    IEmailService emailService,
    IEmailOtpService emailOtpService,
    IOptions<JwtOptions> jwtOptions,
    IOptions<EmailOptions> emailOptions,
    IConfiguration configuration,
    ILogger<AuthService> logger) : IAuthService
{
    private const string GenericLoginError = "البريد الإلكتروني أو كلمة المرور غير صحيحة.";
    private const string GoogleLoginProvider = "Google";

    public async Task<AuthCommandResult<UserSummaryResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return AuthCommandResult<UserSummaryResponse>.Fail(409, "تعارض", "البريد الإلكتروني مستخدم مسبقاً.");
        }

        if (await PhoneExistsAsync(request.PhoneNumber, null, cancellationToken))
        {
            return AuthCommandResult<UserSummaryResponse>.Fail(409, "تعارض", "رقم الهاتف مستخدم مسبقاً.");
        }

        var utcNow = DateTimeOffset.UtcNow;
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            UserName = email,
            Email = email,
            PhoneNumber = request.PhoneNumber.Trim(),
            WhatsAppNumber = string.IsNullOrWhiteSpace(request.WhatsAppNumber) ? request.PhoneNumber.Trim() : request.WhatsAppNumber.Trim(),
            Governorate = request.Governorate.Trim(),
            AccountStatus = AccountStatus.Active,
            EmailConfirmed = false,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var description = createResult.Errors.FirstOrDefault()?.Description ?? "تعذر إنشاء الحساب.";
            return AuthCommandResult<UserSummaryResponse>.Fail(400, "طلب غير صالح", description);
        }

        await userManager.AddToRoleAsync(user, RoleNames.Student);

        if (ShouldAutoConfirmEmail())
        {
            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);
        }
        else
        {
            await IssueAndSendOtpAsync(user, EmailOtpPurposes.EmailVerification, cancellationToken);
        }

        logger.LogInformation("User registered successfully for {UserId}", user.Id);
        return AuthCommandResult<UserSummaryResponse>.Ok(await ToSummaryAsync(user));
    }

    public async Task<AuthCommandResult<UserSummaryResponse>> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            logger.LogInformation("Login failed");
            return AuthCommandResult<UserSummaryResponse>.Fail(401, "غير مصرح", GenericLoginError);
        }

        if (user.AccountStatus != AccountStatus.Active)
        {
            logger.LogInformation("Login denied for inactive account {UserId}", user.Id);
            return AuthCommandResult<UserSummaryResponse>.Fail(403, "ممنوع", "لا يمكن تسجيل الدخول لأن الحساب غير نشط.");
        }

        if (!user.EmailConfirmed)
        {
            await IssueAndSendOtpAsync(user, EmailOtpPurposes.EmailVerification, cancellationToken);
            return AuthCommandResult<UserSummaryResponse>.Fail(
                403,
                "يتطلب التحقق",
                "يجب تأكيد البريد الإلكتروني أولاً. أرسلنا رمز تحقق إلى بريدك.");
        }

        user.LastLoginAt = DateTimeOffset.UtcNow;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await userManager.UpdateAsync(user);

        var cookies = await IssueSessionAsync(user, request.RememberMe, ipAddress, cancellationToken);
        logger.LogInformation("Login succeeded for {UserId}", user.Id);
        return AuthCommandResult<UserSummaryResponse>.Ok(await ToSummaryAsync(user), cookies);
    }

    public async Task<AuthCommandResult<AuthChallengeResponse>> GoogleLoginAsync(
        GoogleLoginRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        _ = ipAddress;
        var clientId = emailOptions.Value.Google.ClientId;
        if (string.IsNullOrWhiteSpace(clientId))
        {
            return AuthCommandResult<AuthChallengeResponse>.Fail(
                503,
                "غير متاح",
                "تسجيل الدخول عبر Google غير مُعد بعد.");
        }

        if (string.IsNullOrWhiteSpace(request.IdToken))
        {
            return AuthCommandResult<AuthChallengeResponse>.Fail(400, "طلب غير صالح", "رمز Google مفقود.");
        }

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(
                request.IdToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [clientId]
                });
        }
        catch (InvalidJwtException)
        {
            return AuthCommandResult<AuthChallengeResponse>.Fail(401, "غير مصرح", "تعذر التحقق من حساب Google.");
        }

        if (string.IsNullOrWhiteSpace(payload.Email) || payload.EmailVerified != true)
        {
            return AuthCommandResult<AuthChallengeResponse>.Fail(
                400,
                "طلب غير صالح",
                "حساب Google لا يوفّر بريداً إلكترونياً مؤكداً.");
        }

        var email = NormalizeEmail(payload.Email);
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            var utcNow = DateTimeOffset.UtcNow;
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                FullName = string.IsNullOrWhiteSpace(payload.Name) ? email.Split('@')[0] : payload.Name.Trim(),
                UserName = email,
                Email = email,
                EmailConfirmed = false,
                PhoneNumber = null,
                AccountStatus = AccountStatus.Active,
                AvatarUrl = payload.Picture,
                CreatedAt = utcNow,
                UpdatedAt = utcNow
            };

            var create = await userManager.CreateAsync(user);
            if (!create.Succeeded)
            {
                var description = create.Errors.FirstOrDefault()?.Description ?? "تعذر إنشاء الحساب عبر Google.";
                return AuthCommandResult<AuthChallengeResponse>.Fail(400, "طلب غير صالح", description);
            }

            await userManager.AddToRoleAsync(user, RoleNames.Student);
        }

        if (user.AccountStatus != AccountStatus.Active)
        {
            return AuthCommandResult<AuthChallengeResponse>.Fail(403, "ممنوع", "لا يمكن تسجيل الدخول لأن الحساب غير نشط.");
        }

        var existingLogin = await userManager.FindByLoginAsync(GoogleLoginProvider, payload.Subject);
        if (existingLogin is null)
        {
            await userManager.AddLoginAsync(
                user,
                new UserLoginInfo(GoogleLoginProvider, payload.Subject, GoogleLoginProvider));
        }

        await IssueAndSendOtpAsync(user, EmailOtpPurposes.GoogleLogin, cancellationToken);
        logger.LogInformation("Google login challenge issued for {UserId}", user.Id);

        return AuthCommandResult<AuthChallengeResponse>.Ok(new AuthChallengeResponse
        {
            RequiresEmailVerification = true,
            Email = email,
            Purpose = EmailOtpPurposes.GoogleLogin,
            Message = "أرسلنا رمز تحقق إلى بريدك على Gmail. أدخله لإكمال تسجيل الدخول."
        });
    }

    public async Task<AuthCommandResult<UserSummaryResponse>> RefreshAsync(
        string? refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return AuthCommandResult<UserSummaryResponse>.Fail(401, "غير مصرح", "جلسة غير صالحة.");
        }

        var stored = await refreshTokenService.FindByRawTokenAsync(refreshToken, cancellationToken);
        if (stored is null)
        {
            return AuthCommandResult<UserSummaryResponse>.Fail(401, "غير مصرح", "جلسة غير صالحة.");
        }

        if (stored.RevokedAt is not null)
        {
            await refreshTokenService.RevokeFamilyAsync(stored.FamilyId, ipAddress, cancellationToken);
            logger.LogWarning("Refresh token reuse detected for family {FamilyId}", stored.FamilyId);
            return AuthCommandResult<UserSummaryResponse>.Fail(401, "غير مصرح", "تم إنهاء الجلسة لأسباب أمنية.");
        }

        if (stored.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return AuthCommandResult<UserSummaryResponse>.Fail(401, "غير مصرح", "انتهت صلاحية الجلسة.");
        }

        var user = stored.User;
        if (user.AccountStatus != AccountStatus.Active)
        {
            await refreshTokenService.RevokeAllForUserAsync(user.Id, ipAddress, cancellationToken);
            return AuthCommandResult<UserSummaryResponse>.Fail(401, "غير مصرح", "الحساب غير نشط.");
        }

        var expiresAt = stored.ExpiresAt;
        var issued = await refreshTokenService.IssueAsync(user, stored.FamilyId, expiresAt, ipAddress, cancellationToken);
        await refreshTokenService.RevokeAsync(stored, ipAddress, issued.Entity.Id, cancellationToken);

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = accessTokenService.CreateAccessToken(user, roles, out _);
        var cookies = new IssuedAuthCookies
        {
            AccessToken = accessToken,
            RefreshToken = issued.RawToken,
            AccessExpiresAt = DateTimeOffset.UtcNow.AddMinutes(jwtOptions.Value.AccessTokenMinutes),
            RefreshExpiresAt = expiresAt
        };

        return AuthCommandResult<UserSummaryResponse>.Ok(await ToSummaryAsync(user), cookies);
    }

    public async Task LogoutAsync(string? refreshToken, string? ipAddress, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var stored = await refreshTokenService.FindByRawTokenAsync(refreshToken, cancellationToken);
        if (stored is null || stored.RevokedAt is not null)
        {
            return;
        }

        await refreshTokenService.RevokeAsync(stored, ipAddress, null, cancellationToken);
        logger.LogInformation("User session logged out for {UserId}", stored.UserId);
    }

    public async Task LogoutAllAsync(Guid userId, string? ipAddress, CancellationToken cancellationToken = default)
    {
        await refreshTokenService.RevokeAllForUserAsync(userId, ipAddress, cancellationToken);
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is not null)
        {
            await userManager.UpdateSecurityStampAsync(user);
        }

        logger.LogInformation("All sessions revoked for {UserId}", userId);
    }

    public async Task<UserSummaryResponse> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("User was not found.");
        return await ToSummaryAsync(user);
    }

    public async Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(NormalizeEmail(email));
        if (user is null)
        {
            logger.LogInformation("Password reset requested for unknown email");
            return;
        }

        await IssueAndSendOtpAsync(user, EmailOtpPurposes.PasswordReset, cancellationToken);

        // Keep legacy link for inbox clients that prefer one-click reset.
        try
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var link = BuildAuthLink("/reset-password", user.Email!, token);
            await emailService.SendPasswordResetAsync(user.Email!, link, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Password reset link email skipped after OTP was issued for {UserId}", user.Id);
        }

        logger.LogInformation("Password reset requested for {UserId}", user.Id);
    }

    public async Task<AuthCommandResult> ResetPasswordAsync(
        ResetPasswordRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(NormalizeEmail(request.Email));
        if (user is null)
        {
            return AuthCommandResult.Fail(400, "طلب غير صالح", "تعذر إعادة تعيين كلمة المرور.");
        }

        var rawToken = (request.Token ?? string.Empty).Trim();
        IdentityResult result;

        if (LooksLikeOtp(rawToken))
        {
            var otpOk = await emailOtpService.VerifyAsync(
                user,
                EmailOtpPurposes.PasswordReset,
                rawToken,
                cancellationToken);
            if (!otpOk)
            {
                return AuthCommandResult.Fail(400, "طلب غير صالح", "رمز الاستعادة غير صالح أو منتهٍ.");
            }

            var identityToken = await userManager.GeneratePasswordResetTokenAsync(user);
            result = await userManager.ResetPasswordAsync(user, identityToken, request.NewPassword);
            if (!result.Succeeded && result.Errors.Any(error => error.Code == "UserMissingPassword"))
            {
                result = await userManager.AddPasswordAsync(user, request.NewPassword);
            }
        }
        else
        {
            result = await userManager.ResetPasswordAsync(user, DecodeToken(rawToken), request.NewPassword);
        }

        if (!result.Succeeded)
        {
            return AuthCommandResult.Fail(400, "طلب غير صالح", "رمز الاستعادة غير صالح أو منتهٍ.");
        }

        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);
        }

        await refreshTokenService.RevokeAllForUserAsync(user.Id, ipAddress, cancellationToken);
        await userManager.UpdateSecurityStampAsync(user);
        logger.LogInformation("Password reset completed for {UserId}", user.Id);
        return AuthCommandResult.Success() with { ClearCookies = true };
    }

    public async Task ResendVerificationAsync(string email, string? purpose = null, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(NormalizeEmail(email));
        if (user is null)
        {
            return;
        }

        var normalizedPurpose = NormalizePurpose(purpose);
        if (normalizedPurpose is EmailOtpPurposes.EmailVerification && user.EmailConfirmed)
        {
            return;
        }

        await IssueAndSendOtpAsync(user, normalizedPurpose, cancellationToken);
    }

    public async Task<AuthCommandResult<UserSummaryResponse>> VerifyEmailAsync(
        VerifyEmailRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(NormalizeEmail(request.Email));
        if (user is null)
        {
            return AuthCommandResult<UserSummaryResponse>.Fail(400, "طلب غير صالح", "تعذر تأكيد البريد الإلكتروني.");
        }

        var purpose = NormalizePurpose(request.Purpose);
        var rawToken = (request.Token ?? string.Empty).Trim();
        var verified = false;

        if (LooksLikeOtp(rawToken))
        {
            verified = await emailOtpService.VerifyAsync(user, purpose, rawToken, cancellationToken);
        }
        else if (purpose is EmailOtpPurposes.EmailVerification)
        {
            var confirm = await userManager.ConfirmEmailAsync(user, DecodeToken(rawToken));
            verified = confirm.Succeeded;
        }

        if (!verified)
        {
            return AuthCommandResult<UserSummaryResponse>.Fail(400, "طلب غير صالح", "رمز التأكيد غير صالح أو منتهٍ.");
        }

        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            user.UpdatedAt = DateTimeOffset.UtcNow;
            await userManager.UpdateAsync(user);
        }

        IssuedAuthCookies? cookies = null;
        if (purpose is EmailOtpPurposes.GoogleLogin or EmailOtpPurposes.EmailVerification)
        {
            user.LastLoginAt = DateTimeOffset.UtcNow;
            user.UpdatedAt = DateTimeOffset.UtcNow;
            await userManager.UpdateAsync(user);
            cookies = await IssueSessionAsync(user, request.RememberMe, ipAddress, cancellationToken);
        }

        logger.LogInformation("Email verified for {UserId} purpose={Purpose}", user.Id, purpose);
        return AuthCommandResult<UserSummaryResponse>.Ok(await ToSummaryAsync(user), cookies);
    }

    public async Task<UserSummaryResponse> UpdateProfileAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("User was not found.");

        if (await PhoneExistsAsync(request.PhoneNumber, userId, cancellationToken))
        {
            throw new InvalidOperationException("PHONE_IN_USE");
        }

        user.FullName = request.FullName.Trim();
        user.PhoneNumber = request.PhoneNumber.Trim();
        user.WhatsAppNumber = string.IsNullOrWhiteSpace(request.WhatsAppNumber) ? null : request.WhatsAppNumber.Trim();
        user.Governorate = request.Governorate.Trim();
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await userManager.UpdateAsync(user);
        return await ToSummaryAsync(user);
    }

    public async Task<AuthCommandResult> ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("User was not found.");

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            return AuthCommandResult.Fail(400, "طلب غير صالح", "كلمة المرور الحالية غير صحيحة.");
        }

        await refreshTokenService.RevokeAllForUserAsync(user.Id, ipAddress, cancellationToken);
        await userManager.UpdateSecurityStampAsync(user);
        logger.LogInformation("Password changed for {UserId}", user.Id);
        return AuthCommandResult.Success() with { ClearCookies = true };
    }

    private async Task IssueAndSendOtpAsync(ApplicationUser user, string purpose, CancellationToken cancellationToken)
    {
        var code = await emailOtpService.IssueAsync(user, purpose, cancellationToken);
        await emailService.SendOtpAsync(user.Email!, code, purpose, cancellationToken);
    }

    private bool ShouldAutoConfirmEmail()
    {
        var provider = emailOptions.Value.Provider;
        return string.Equals(provider, "Logging", StringComparison.OrdinalIgnoreCase)
               || string.Equals(provider, "Disabled", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<IssuedAuthCookies> IssueSessionAsync(
        ApplicationUser user,
        bool rememberMe,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var roles = await userManager.GetRolesAsync(user);
        var accessToken = accessTokenService.CreateAccessToken(user, roles, out _);
        var refreshLifetimeDays = rememberMe
            ? jwtOptions.Value.RefreshTokenDays
            : jwtOptions.Value.SessionRefreshTokenDays;
        var refreshExpiresAt = DateTimeOffset.UtcNow.AddDays(refreshLifetimeDays);
        var issued = await refreshTokenService.IssueAsync(
            user,
            Guid.NewGuid(),
            refreshExpiresAt,
            ipAddress,
            cancellationToken);

        return new IssuedAuthCookies
        {
            AccessToken = accessToken,
            RefreshToken = issued.RawToken,
            AccessExpiresAt = DateTimeOffset.UtcNow.AddMinutes(jwtOptions.Value.AccessTokenMinutes),
            RefreshExpiresAt = refreshExpiresAt
        };
    }

    private async Task<UserSummaryResponse> ToSummaryAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        return new UserSummaryResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            WhatsAppNumber = user.WhatsAppNumber,
            Governorate = user.Governorate,
            Roles = roles.ToArray(),
            AccountStatus = user.AccountStatus.ToString(),
            EmailConfirmed = user.EmailConfirmed
        };
    }

    private async Task<bool> PhoneExistsAsync(string phoneNumber, Guid? excludingUserId, CancellationToken cancellationToken)
    {
        var normalized = phoneNumber.Trim();
        return await dbContext.Users.AnyAsync(
            user => user.PhoneNumber == normalized && (!excludingUserId.HasValue || user.Id != excludingUserId.Value),
            cancellationToken);
    }

    private string BuildAuthLink(string path, string email, string token)
    {
        var origin = (configuration["App:PublicUrl"] ?? configuration["PublicAppUrl"] ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(origin))
        {
            throw new InvalidOperationException("App:PublicUrl is not configured.");
        }

        var encodedToken = EncodeToken(token);
        return $"{origin.TrimEnd('/')}{path}?email={Uri.EscapeDataString(email)}&token={encodedToken}";
    }

    private static bool LooksLikeOtp(string token) =>
        token.Length is >= 4 and <= 8 && token.All(char.IsDigit);

    private static string NormalizePurpose(string? purpose) =>
        purpose?.Trim() switch
        {
            EmailOtpPurposes.PasswordReset => EmailOtpPurposes.PasswordReset,
            EmailOtpPurposes.GoogleLogin => EmailOtpPurposes.GoogleLogin,
            _ => EmailOtpPurposes.EmailVerification
        };

    private static string EncodeToken(string token)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(token))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string DecodeToken(string token)
    {
        try
        {
            var padded = token.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }

            return Encoding.UTF8.GetString(Convert.FromBase64String(padded));
        }
        catch (FormatException)
        {
            return token;
        }
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
