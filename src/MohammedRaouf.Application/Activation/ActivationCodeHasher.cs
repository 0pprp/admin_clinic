using System.Security.Cryptography;
using System.Text;

namespace MohammedRaouf.Application.Activation;

public sealed class ActivationCodeHasher : IActivationCodeHasher
{
    public string Hash(string canonicalCode)
    {
        var bytes = Encoding.UTF8.GetBytes(canonicalCode);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
