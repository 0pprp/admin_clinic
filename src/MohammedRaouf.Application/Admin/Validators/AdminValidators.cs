using FluentValidation;
using MohammedRaouf.Application.Courses;
using MohammedRaouf.Contracts.Consultations;
using MohammedRaouf.Contracts.Contact;
using MohammedRaouf.Contracts.Cms;
using MohammedRaouf.Contracts.Admin;

namespace MohammedRaouf.Application.Admin.Validators;

public sealed class CreateConsultationRequestValidator : AbstractValidator<CreateConsultationRequest>
{
    public static readonly string[] Types = ["Business", "Marketing", "Management", "Personal", "Other"];
    public static readonly string[] CommunicationMethods = ["Phone", "WhatsApp", "Email"];

    public CreateConsultationRequestValidator()
    {
        RuleFor(request => request.FullName).NotEmpty().MaximumLength(200).WithMessage("الاسم مطلوب.");
        RuleFor(request => request.PhoneNumber).NotEmpty().MaximumLength(32).WithMessage("رقم الهاتف مطلوب.");
        RuleFor(request => request.Email)
            .EmailAddress()
            .When(request => !string.IsNullOrWhiteSpace(request.Email))
            .WithMessage("البريد الإلكتروني غير صالح.");
        RuleFor(request => request.ConsultationType)
            .Must(value => Types.Contains(value))
            .WithMessage("نوع الاستشارة غير صالح.");
        RuleFor(request => request.Topic).NotEmpty().MaximumLength(200).WithMessage("موضوع الاستشارة مطلوب.");
        RuleFor(request => request.Message).NotEmpty().MaximumLength(4000).WithMessage("الرسالة مطلوبة.");
        RuleFor(request => request.PreferredCommunicationMethod)
            .Must(value => CommunicationMethods.Contains(value))
            .WithMessage("طريقة التواصل غير صالحة.");
        RuleFor(request => request.PreferredDate)
            .Must(date => date is null || date >= DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1)))
            .WithMessage("تاريخ الموعد المفضل غير صالح.");
        RuleFor(request => request.CompanyName).MaximumLength(200);
        RuleFor(request => request.WhatsAppNumber).MaximumLength(32);
    }
}

public sealed class ScheduleConsultationRequestValidator : AbstractValidator<ScheduleConsultationRequest>
{
    public ScheduleConsultationRequestValidator()
    {
        RuleFor(request => request.ScheduledDate).NotEmpty().WithMessage("تاريخ الموعد مطلوب.");
        RuleFor(request => request.ScheduledTime).NotEmpty().WithMessage("وقت الموعد مطلوب.");
        RuleFor(request => request.AdminNotes).MaximumLength(4000);
    }
}

public sealed class ConsultationNoteRequestValidator : AbstractValidator<ConsultationNoteRequest>
{
    public ConsultationNoteRequestValidator()
    {
        RuleFor(request => request.AdminNotes).MaximumLength(4000);
    }
}

public sealed class CreateContactMessageRequestValidator : AbstractValidator<CreateContactMessageRequest>
{
    public CreateContactMessageRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(200).WithMessage("الاسم مطلوب.");
        RuleFor(request => request.Email).NotEmpty().EmailAddress().WithMessage("البريد الإلكتروني غير صالح.");
        RuleFor(request => request.Subject).NotEmpty().MaximumLength(200).WithMessage("الموضوع مطلوب.");
        RuleFor(request => request.Message).NotEmpty().MaximumLength(4000).WithMessage("الرسالة مطلوبة.");
        RuleFor(request => request.Phone).MaximumLength(32);
    }
}

public sealed class SaveArticleRequestValidator : AbstractValidator<SaveArticleRequest>
{
    public SaveArticleRequestValidator()
    {
        RuleFor(request => request.Title).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Slug).NotEmpty().MaximumLength(120)
            .Must(CourseSlug.IsValid)
            .WithMessage("المسار غير صالح.");
        RuleFor(request => request.Excerpt).NotEmpty().MaximumLength(500);
        RuleFor(request => request.Content).NotEmpty().MaximumLength(20000);
        RuleFor(request => request.CoverImage).MaximumLength(500);
    }
}

public sealed class SaveFaqRequestValidator : AbstractValidator<SaveFaqRequest>
{
    public SaveFaqRequestValidator()
    {
        RuleFor(request => request.Question).NotEmpty().MaximumLength(300);
        RuleFor(request => request.Answer).NotEmpty().MaximumLength(4000);
    }
}

public sealed class SaveExpertiseRequestValidator : AbstractValidator<SaveExpertiseRequest>
{
    public SaveExpertiseRequestValidator()
    {
        RuleFor(request => request.Title).NotEmpty().MaximumLength(120);
        RuleFor(request => request.Description).NotEmpty().MaximumLength(1000);
        RuleFor(request => request.IconKey).MaximumLength(64);
    }
}

public sealed class SaveTestimonialRequestValidator : AbstractValidator<SaveTestimonialRequest>
{
    public SaveTestimonialRequestValidator()
    {
        RuleFor(request => request.AuthorDisplayName).NotEmpty().MaximumLength(120);
        RuleFor(request => request.Body).NotEmpty().MaximumLength(2000);
        RuleFor(request => request.AuthorTitle).MaximumLength(160);
    }
}

public sealed class SaveStatisticRequestValidator : AbstractValidator<SaveStatisticRequest>
{
    public SaveStatisticRequestValidator()
    {
        RuleFor(request => request.Label).NotEmpty().MaximumLength(120);
        RuleFor(request => request.DisplayValue).NotEmpty().MaximumLength(64);
    }
}

public sealed class SaveSiteContentSettingsRequestValidator : AbstractValidator<SaveSiteContentSettingsRequest>
{
    public SaveSiteContentSettingsRequestValidator()
    {
        RuleFor(request => request.BrandName).MaximumLength(120);
        RuleFor(request => request.BrandNameEnglish).MaximumLength(120);
        RuleFor(request => request.PublicPhone).MaximumLength(32);
        RuleFor(request => request.PublicWhatsApp).MaximumLength(32);
        RuleFor(request => request.PublicEmail).EmailAddress().When(request => !string.IsNullOrWhiteSpace(request.PublicEmail));
        RuleFor(request => request.SocialLinks).MaximumLength(2000);
        RuleFor(request => request.FooterText).MaximumLength(500);
        RuleFor(request => request.ConsultationInfo).MaximumLength(2000);
    }
}

public sealed class SavePaymentSettingsRequestValidator : AbstractValidator<SavePaymentSettingsRequest>
{
    public SavePaymentSettingsRequestValidator()
    {
        RuleFor(request => request.PaymentMethods).MaximumLength(2000);
        RuleFor(request => request.TransferInstructions).MaximumLength(4000);
        RuleFor(request => request.SupportPhone).MaximumLength(32);
        RuleFor(request => request.SupportWhatsApp).MaximumLength(32);
    }
}

public sealed class UpdateRolesRequestValidator : AbstractValidator<UpdateRolesRequest>
{
    public UpdateRolesRequestValidator()
    {
        RuleFor(request => request.Roles).NotEmpty().WithMessage("يجب اختيار دور واحد على الأقل.");
    }
}
