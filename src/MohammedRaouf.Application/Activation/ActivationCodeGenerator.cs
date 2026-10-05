using System.Security.Cryptography;

namespace MohammedRaouf.Application.Activation;

public sealed class ActivationCodeGenerator : IActivationCodeGenerator
{
    public string GeneratePlainCode()
    {
        Span<char> random = stackalloc char[ActivationCodeFormat.RandomLength];
        var alphabet = ActivationCodeFormat.Alphabet;
        for (var i = 0; i < random.Length; i++)
        {
            random[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
        }

        return ActivationCodeFormat.Display(random);
    }
}
