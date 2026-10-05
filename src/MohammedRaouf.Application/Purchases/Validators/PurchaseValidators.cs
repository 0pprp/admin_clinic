using FluentValidation;
using MohammedRaouf.Contracts.Purchases;

namespace MohammedRaouf.Application.Purchases.Validators;

public sealed class CreatePurchaseRequestRequestValidator : AbstractValidator<CreatePurchaseRequestRequest>
{
    public CreatePurchaseRequestRequestValidator()
    {
        RuleFor(request => request.CourseId)
            .NotEmpty().WithMessage("الدورة مطلوبة.");

        RuleFor(request => request.CustomerNotes)
            .MaximumLength(2000)
            .When(request => request.CustomerNotes is not null);
    }
}

public sealed class AdminPurchaseNoteRequestValidator : AbstractValidator<AdminPurchaseNoteRequest>
{
    public AdminPurchaseNoteRequestValidator()
    {
        RuleFor(request => request.Note)
            .MaximumLength(2000)
            .When(request => request.Note is not null);
    }
}

public sealed class AdminAwaitingPaymentRequestValidator : AbstractValidator<AdminAwaitingPaymentRequest>
{
    public AdminAwaitingPaymentRequestValidator()
    {
        RuleFor(request => request.PaymentMethod).MaximumLength(64);
        RuleFor(request => request.PaymentReference).MaximumLength(128);
        RuleFor(request => request.AdminNote).MaximumLength(2000);
    }
}

public sealed class AdminConfirmPaymentRequestValidator : AbstractValidator<AdminConfirmPaymentRequest>
{
    public AdminConfirmPaymentRequestValidator()
    {
        RuleFor(request => request.PaymentMethod)
            .NotEmpty().WithMessage("طريقة الدفع مطلوبة.")
            .MaximumLength(64);

        RuleFor(request => request.PaymentReference).MaximumLength(128);
        RuleFor(request => request.AdminNotes).MaximumLength(2000);
    }
}

public sealed class AdminPurchaseReasonRequestValidator : AbstractValidator<AdminPurchaseReasonRequest>
{
    public AdminPurchaseReasonRequestValidator()
    {
        RuleFor(request => request.Reason)
            .NotEmpty().WithMessage("سبب الإجراء مطلوب.")
            .MaximumLength(2000);
    }
}
