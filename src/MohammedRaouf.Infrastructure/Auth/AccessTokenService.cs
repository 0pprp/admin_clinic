using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MohammedRaouf.Application.Auth;
using MohammedRaouf.Application.Security;
using MohammedRaouf.Domain.Identity;

namespace MohammedRaouf.Infrastructure.Auth;

public sealed class AccessTokenService(IOptions<JwtOptions> jwtOptions) : IAccessTokenService
{
    public string CreateAccessToken(ApplicationUser user, IEnumerable<string> roles, out string jwtId)
    {
        var options = jwtOptions.Value;
        jwtId = Guid.NewGuid().ToString("N");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, jwtId),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("sid", user.SecurityStamp ?? string.Empty)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = JwtSigning.CreateKey(options.SigningKey);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(options.AccessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
