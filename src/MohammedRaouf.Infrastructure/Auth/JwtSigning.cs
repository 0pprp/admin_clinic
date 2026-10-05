using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MohammedRaouf.Infrastructure.Auth;

public static class JwtSigning
{
    public const string KeyId = "mr-hs256";

    public static SymmetricSecurityKey CreateKey(string signingKey) =>
        new(Encoding.UTF8.GetBytes(signingKey)) { KeyId = KeyId };
}
