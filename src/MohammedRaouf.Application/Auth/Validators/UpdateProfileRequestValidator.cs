using FluentValidation;
using MohammedRaouf.Contracts.Profile;

namespace MohammedRaouf.Application.Auth.Validators;

public sealed class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(request => request.FullName)
            .NotEmpty().WithMessage("الاسم الكامل مطلوب.")
            .MaximumLength(200);

        RuleFor(request => request.PhoneNumber)
            .NotEmpty().WithMessage("رقم الهاتف مطلوب.")
            .Matches(@"^(\+964|0)?7\d{9}$").WithMessage("أدخل رقم هاتف عراقي صحيح.");

        RuleFor(request => request.WhatsAppNumber)
            .Matches(@"^(\+964|0)?7\d{9}$")
            .When(request => !string.IsNullOrWhiteSpace(request.WhatsAppNumber))
            .WithMessage("أدخل رقم واتساب عراقي صحيح.");

        RuleFor(request => request.Governorate)
            .NotEmpty().WithMessage("المحافظة مطلوبة.");
    }
}
