using FluentValidation;
using MohammedRaouf.Contracts.Admin;
using MohammedRaouf.Domain.Enums;

namespace MohammedRaouf.Application.Courses.Validators;

public sealed class SaveCourseRequestValidator : AbstractValidator<SaveCourseRequest>
{
    public SaveCourseRequestValidator()
    {
        RuleFor(request => request.Title)
            .NotEmpty().WithMessage("عنوان الدورة مطلوب.")
            .MaximumLength(200);

        RuleFor(request => request.Slug)
            .NotEmpty().WithMessage("المسار مطلوب.")
            .Must(slug => CourseSlug.IsValid(CourseSlug.Normalize(slug)))
            .WithMessage("المسار يجب أن يكون صالحاً للروابط. يُفضَّل الإنجليزي الصغير، ويُسمح بالحروف العربية والأرقام والشرطة.");

        RuleFor(request => request.ShortDescription)
            .NotEmpty().WithMessage("الوصف المختصر مطلوب.")
            .MaximumLength(500);

        RuleFor(request => request.Description)
            .NotEmpty().WithMessage("وصف الدورة مطلوب.");

        RuleFor(request => request.PriceIQD)
            .GreaterThanOrEqualTo(0).WithMessage("السعر غير صالح.");

        RuleFor(request => request.ThumbnailUrl)
            .MaximumLength(2048)
            .When(request => !string.IsNullOrWhiteSpace(request.ThumbnailUrl));

        RuleFor(request => request.TrailerUrl)
            .MaximumLength(2048)
            .When(request => !string.IsNullOrWhiteSpace(request.TrailerUrl));

        RuleFor(request => request.Level)
            .Must(value => Enum.TryParse<CourseLevel>(value, true, out _))
            .WithMessage("مستوى الدورة غير صالح.");

        RuleFor(request => request.AccessType)
            .Must(value => Enum.TryParse<CourseAccessType>(value, true, out _))
            .WithMessage("نوع الوصول غير صالح.");

        RuleFor(request => request.AccessDurationDays)
            .NotNull().WithMessage("مدة الوصول مطلوبة للدورات محدودة المدة.")
            .GreaterThan(0).WithMessage("مدة الوصول يجب أن تكون أكبر من صفر.")
            .When(request => Enum.TryParse<CourseAccessType>(request.AccessType, true, out var type) &&
                             type == CourseAccessType.LimitedDuration);

        RuleFor(request => request.AccessDurationDays)
            .Must(days => days is null)
            .WithMessage("مدة الوصول تُستخدم فقط مع الوصول المحدود.")
            .When(request => Enum.TryParse<CourseAccessType>(request.AccessType, true, out var type) &&
                             type == CourseAccessType.Lifetime);
    }
}

public sealed class SaveSectionRequestValidator : AbstractValidator<SaveSectionRequest>
{
    public SaveSectionRequestValidator()
    {
        RuleFor(request => request.Title)
            .NotEmpty().WithMessage("عنوان القسم مطلوب.")
            .MaximumLength(200);

        RuleFor(request => request.Description)
            .MaximumLength(2000)
            .When(request => request.Description is not null);

        RuleFor(request => request.SortOrder)
            .GreaterThan(0)
            .When(request => request.SortOrder.HasValue);
    }
}

public sealed class SaveLessonRequestValidator : AbstractValidator<SaveLessonRequest>
{
    public SaveLessonRequestValidator()
    {
        RuleFor(request => request.Title)
            .NotEmpty().WithMessage("عنوان الدرس مطلوب.")
            .MaximumLength(200);

        RuleFor(request => request.Description)
            .MaximumLength(4000)
            .When(request => request.Description is not null);

        RuleFor(request => request.DurationSeconds)
            .GreaterThanOrEqualTo(0).WithMessage("مدة الدرس غير صالحة.");

        RuleFor(request => request.VideoKey)
            .MaximumLength(500)
            .When(request => request.VideoKey is not null);

        RuleFor(request => request.VideoProvider)
            .Must(value => value is null || Enum.TryParse<VideoProvider>(value, true, out _))
            .WithMessage("مزود الفيديو غير صالح.");

        RuleFor(request => request.SortOrder)
            .GreaterThan(0)
            .When(request => request.SortOrder.HasValue);
    }
}

public sealed class ReorderItemsRequestValidator : AbstractValidator<ReorderItemsRequest>
{
    public ReorderItemsRequestValidator()
    {
        RuleFor(request => request.Items)
            .NotEmpty().WithMessage("قائمة الترتيب مطلوبة.");

        RuleFor(request => request.Items)
            .Must(items => items.Select(item => item.Id).Distinct().Count() == items.Count)
            .WithMessage("لا يجوز تكرار العناصر في إعادة الترتيب.");

        RuleForEach(request => request.Items)
            .ChildRules(item =>
            {
                item.RuleFor(value => value.Id).NotEmpty();
                item.RuleFor(value => value.SortOrder).GreaterThan(0);
            });
    }
}
