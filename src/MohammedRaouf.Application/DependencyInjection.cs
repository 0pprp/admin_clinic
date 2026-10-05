using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MohammedRaouf.Application.Activation;
using MohammedRaouf.Application.Auth.Validators;

namespace MohammedRaouf.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        services.AddSingleton<IActivationCodeGenerator, ActivationCodeGenerator>();
        services.AddSingleton<IActivationCodeHasher, ActivationCodeHasher>();
        return services;
    }
}
