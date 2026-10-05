using FluentValidation;
using MohammedRaouf.Contracts.Profile;

namespace MohammedRaouf.Application.Auth.Validators;

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(request => request.CurrentPassword)
            .NotEmpty().WithMessage("كلمة المرور الحالية مطلوبة.");

        RuleFor(request => request.NewPassword)
            .NotEmpty().WithMessage("كلمة المرور الجديدة مطلوبة.")
            .MinimumLength(8).WithMessage("كلمة المرور يجب أن تكون 8 أحرف على الأقل.")
            .Matches("[0-9]").WithMessage("كلمة المرور يجب أن تحتوي على رقم واحد على الأقل.")
            .Matches("[a-z]").WithMessage("كلمة المرور يجب أن تحتوي على حرف صغير واحد على الأقل.")
            .NotEqual(request => request.CurrentPassword).WithMessage("كلمة المرور الجديدة يجب أن تختلف عن الحالية.");

        RuleFor(request => request.ConfirmPassword)
            .Equal(request => request.NewPassword).WithMessage("تأكيد كلمة المرور غير مطابق.");
    }
}
