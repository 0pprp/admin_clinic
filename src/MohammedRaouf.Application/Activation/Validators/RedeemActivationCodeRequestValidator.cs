using FluentValidation;
using MohammedRaouf.Contracts.Activation;

namespace MohammedRaouf.Application.Activation.Validators;

public sealed class RedeemActivationCodeRequestValidator : AbstractValidator<RedeemActivationCodeRequest>
{
    public RedeemActivationCodeRequestValidator()
    {
        RuleFor(request => request.Code)
            .NotEmpty().WithMessage("كود التفعيل مطلوب.")
            .MaximumLength(64).WithMessage("كود التفعيل غير صالح.");
    }
}
