using FluentValidation;
using MohammedRaouf.Contracts.Auth;

namespace MohammedRaouf.Application.Auth.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(request => request.FullName)
            .NotEmpty().WithMessage("الاسم الكامل مطلوب.")
            .MaximumLength(200).WithMessage("الاسم الكامل طويل جداً.");

        RuleFor(request => request.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة.");

        RuleFor(request => request.PhoneNumber)
            .NotEmpty().WithMessage("رقم الهاتف مطلوب.")
            .Matches(@"^(\+964|0)?7\d{9}$").WithMessage("أدخل رقم هاتف عراقي صحيح.");

        RuleFor(request => request.WhatsAppNumber)
            .Matches(@"^(\+964|0)?7\d{9}$")
            .When(request => !string.IsNullOrWhiteSpace(request.WhatsAppNumber))
            .WithMessage("أدخل رقم واتساب عراقي صحيح.");

        RuleFor(request => request.Governorate)
            .NotEmpty().WithMessage("المحافظة مطلوبة.")
            .MaximumLength(100);

        RuleFor(request => request.Password)
            .NotEmpty().WithMessage("كلمة المرور مطلوبة.")
            .MinimumLength(8).WithMessage("كلمة المرور يجب أن تكون 8 أحرف على الأقل.")
            .Matches("[0-9]").WithMessage("كلمة المرور يجب أن تحتوي على رقم واحد على الأقل.")
            .Matches("[a-z]").WithMessage("كلمة المرور يجب أن تحتوي على حرف صغير واحد على الأقل.");

        RuleFor(request => request.PasswordConfirmation)
            .Equal(request => request.Password).WithMessage("تأكيد كلمة المرور غير مطابق.");

        RuleFor(request => request.TermsAccepted)
            .Equal(true).WithMessage("يجب قبول الشروط والمتابعة.");
    }
}
