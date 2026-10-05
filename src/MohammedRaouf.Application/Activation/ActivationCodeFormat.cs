using System.Text;

namespace MohammedRaouf.Application.Activation;

public static class ActivationCodeFormat
{
    public const string Prefix = "MR";
    public const string Alphabet = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";
    public const int RandomLength = 8;

    public static string Display(ReadOnlySpan<char> randomChars)
    {
        if (randomChars.Length != RandomLength)
        {
            throw new ArgumentException("Activation codes require 8 random characters.", nameof(randomChars));
        }

        return $"{Prefix}-{randomChars[..4].ToString()}-{randomChars[4..].ToString()}";
    }

    public static bool TryNormalize(string? input, out string canonical)
    {
        canonical = string.Empty;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var buffer = new StringBuilder(Prefix.Length + RandomLength);
        foreach (var ch in input.AsSpan())
        {
            if (char.IsWhiteSpace(ch) || ch is '-' or '_')
            {
                continue;
            }

            var upper = char.ToUpperInvariant(ch);
            buffer.Append(upper);
        }

        if (buffer.Length != Prefix.Length + RandomLength)
        {
            return false;
        }

        var compact = buffer.ToString();
        if (!compact.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return false;
        }

        var random = compact.AsSpan(Prefix.Length);
        foreach (var ch in random)
        {
            if (Alphabet.IndexOf(ch) < 0)
            {
                return false;
            }
        }

        canonical = Display(random);
        return true;
    }
}
