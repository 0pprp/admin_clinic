namespace MohammedRaouf.Application.Activation;

public interface IActivationCodeHasher
{
    string Hash(string canonicalCode);
}
