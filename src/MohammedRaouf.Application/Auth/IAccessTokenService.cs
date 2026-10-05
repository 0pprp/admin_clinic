using MohammedRaouf.Domain.Identity;

namespace MohammedRaouf.Application.Auth;

public interface IAccessTokenService
{
    string CreateAccessToken(ApplicationUser user, IEnumerable<string> roles, out string jwtId);
}
