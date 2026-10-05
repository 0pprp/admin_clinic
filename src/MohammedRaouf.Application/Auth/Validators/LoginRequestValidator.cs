using FluentValidation;
using MohammedRaouf.Contracts.Auth;

namespace MohammedRaouf.Application.Auth.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.");

        RuleFor(request => request.Password)
            .NotEmpty().WithMessage("كلمة المرور مطلوبة.");
    }
}
