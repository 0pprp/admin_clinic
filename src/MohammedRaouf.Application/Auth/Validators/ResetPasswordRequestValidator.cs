using FluentValidation;
using MohammedRaouf.Contracts.Auth;

namespace MohammedRaouf.Application.Auth.Validators;

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة.");

        RuleFor(request => request.Token)
            .NotEmpty().WithMessage("رمز الاستعادة مطلوب.");

        RuleFor(request => request.NewPassword)
            .NotEmpty().WithMessage("كلمة المرور الجديدة مطلوبة.")
            .MinimumLength(8).WithMessage("كلمة المرور يجب أن تكون 8 أحرف على الأقل.")
            .Matches("[0-9]").WithMessage("كلمة المرور يجب أن تحتوي على رقم واحد على الأقل.")
            .Matches("[a-z]").WithMessage("كلمة المرور يجب أن تحتوي على حرف صغير واحد على الأقل.");

        RuleFor(request => request.ConfirmPassword)
            .Equal(request => request.NewPassword).WithMessage("تأكيد كلمة المرور غير مطابق.");
    }
}
