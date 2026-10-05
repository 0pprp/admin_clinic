using FluentValidation;
using MohammedRaouf.Contracts.Learning;

namespace MohammedRaouf.Application.Learning.Validators;

public sealed class UpdateLessonProgressRequestValidator : AbstractValidator<UpdateLessonProgressRequest>
{
    public UpdateLessonProgressRequestValidator()
    {
        RuleFor(request => request.WatchedSeconds)
            .GreaterThanOrEqualTo(0)
            .WithMessage("وقت المشاهدة غير صالح.");
    }
}
