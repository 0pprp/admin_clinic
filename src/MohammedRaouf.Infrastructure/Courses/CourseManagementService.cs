using Microsoft.EntityFrameworkCore;
using MohammedRaouf.Application.Admin;
using MohammedRaouf.Application.Common;
using MohammedRaouf.Application.Courses;
using MohammedRaouf.Contracts.Admin;
using MohammedRaouf.Contracts.Public;
using MohammedRaouf.Domain.Entities;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.Infrastructure.Courses;

public sealed class CourseManagementService(ApplicationDbContext dbContext, IAdminAuditService audit)
    : ICourseManagementService
{
    public async Task<PagedResponse<AdminCourseSummaryResponse>> ListAsync(
        int page,
        int pageSize,
        string? search,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var safePage = page < 1 ? 1 : page;
        var safeSize = pageSize < 1 ? 12 : Math.Min(pageSize, 100);
        var query = dbContext.Courses.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<CourseStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(course => course.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(course =>
                course.Title.ToLower().Contains(term) ||
                course.Slug.ToLower().Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(course => course.UpdatedAt)
            .Skip((safePage - 1) * safeSize)
            .Take(safeSize)
            .Select(course => new AdminCourseSummaryResponse
            {
                Id = course.Id,
                Title = course.Title,
                Slug = course.Slug,
                Status = course.Status.ToString(),
                Level = course.Level.ToString(),
                PriceIQD = course.PriceIQD,
                IsFeatured = course.IsFeatured,
                SectionCount = course.Sections.Count,
                LessonCount = course.Sections.SelectMany(section => section.Lessons).Count(),
                UpdatedAt = course.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<AdminCourseSummaryResponse>
        {
            Items = items,
            Page = safePage,
            PageSize = safeSize,
            TotalCount = total
        };
    }

    public async Task<AdminCourseDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var course = await LoadCourseGraphAsync(id, cancellationToken);
        if (course is null)
        {
            return null;
        }

        var mapped = MapCourse(course);
        return await AttachVideoStatusesAsync(mapped, cancellationToken);
    }

    public async Task<ActionResult<AdminCourseDetailResponse>> CreateAsync(
        SaveCourseRequest request,
        CancellationToken cancellationToken = default)
    {
        var prepared = PrepareCourseFields(request);
        if (!prepared.Succeeded)
        {
            return Fail<AdminCourseDetailResponse>(prepared);
        }

        if (await SlugTakenAsync(prepared.Slug, exceptCourseId: null, cancellationToken))
        {
            return ActionResult<AdminCourseDetailResponse>.Fail(409, "تعارض", "هذا المسار مستخدم لدورة أخرى.");
        }

        var now = DateTimeOffset.UtcNow;
        var course = new Course
        {
            Id = Guid.NewGuid(),
            Title = prepared.CourseTitle,
            Slug = prepared.Slug,
            ShortDescription = prepared.ShortDescription,
            Description = prepared.Description,
            PriceIQD = request.PriceIQD,
            ThumbnailUrl = EmptyToNull(request.ThumbnailUrl),
            TrailerUrl = EmptyToNull(request.TrailerUrl),
            Level = prepared.Level,
            AccessType = prepared.AccessType,
            AccessDurationDays = prepared.AccessDurationDays,
            IsFeatured = request.IsFeatured,
            Status = CourseStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Courses.Add(course);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<AdminCourseDetailResponse>.Ok(MapCourse(course), 201);
    }

    public async Task<ActionResult<AdminCourseDetailResponse>> UpdateAsync(
        Guid id,
        SaveCourseRequest request,
        CancellationToken cancellationToken = default)
    {
        var course = await LoadCourseGraphAsync(id, cancellationToken);
        if (course is null)
        {
            return ActionResult<AdminCourseDetailResponse>.Fail(404, "غير موجود", "الدورة غير موجودة.");
        }

        var prepared = PrepareCourseFields(request);
        if (!prepared.Succeeded)
        {
            return ActionResult<AdminCourseDetailResponse>.Fail(
                prepared.StatusCode,
                prepared.ErrorTitle ?? "خطأ",
                prepared.Detail ?? "تعذر إكمال الطلب.");
        }

        if (await SlugTakenAsync(prepared.Slug, exceptCourseId: id, cancellationToken))
        {
            return ActionResult<AdminCourseDetailResponse>.Fail(409, "تعارض", "هذا المسار مستخدم لدورة أخرى.");
        }

        course.Title = prepared.CourseTitle;
        course.Slug = prepared.Slug;
        course.ShortDescription = prepared.ShortDescription;
        course.Description = prepared.Description;
        course.PriceIQD = request.PriceIQD;
        course.ThumbnailUrl = EmptyToNull(request.ThumbnailUrl);
        course.TrailerUrl = EmptyToNull(request.TrailerUrl);
        course.Level = prepared.Level;
        course.AccessType = prepared.AccessType;
        course.AccessDurationDays = prepared.AccessDurationDays;
        course.IsFeatured = request.IsFeatured;
        course.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<AdminCourseDetailResponse>.Ok(MapCourse(course));
    }

    public async Task<ActionResult<AdminCourseDetailResponse>> PublishAsync(
        Guid id,
        Guid actorUserId,
        string? ip,
        CancellationToken cancellationToken = default)
    {
        var course = await LoadCourseGraphAsync(id, cancellationToken);
        if (course is null)
        {
            return ActionResult<AdminCourseDetailResponse>.Fail(404, "غير موجود", "الدورة غير موجودة.");
        }

        if (string.IsNullOrWhiteSpace(course.Title) ||
            string.IsNullOrWhiteSpace(course.Slug) ||
            string.IsNullOrWhiteSpace(course.Description) ||
            course.PriceIQD < 0)
        {
            return ActionResult<AdminCourseDetailResponse>.Fail(400, "تعذر النشر", "أكمل بيانات الدورة قبل النشر.");
        }

        if (course.Sections.Count == 0)
        {
            return ActionResult<AdminCourseDetailResponse>.Fail(400, "تعذر النشر", "لا يمكن نشر دورة بلا أقسام.");
        }

        var publishedLessons = course.Sections.SelectMany(section => section.Lessons)
            .Count(lesson => lesson.Status == LessonStatus.Published);
        if (publishedLessons == 0)
        {
            return ActionResult<AdminCourseDetailResponse>.Fail(400, "تعذر النشر", "يلزم درس منشور واحد على الأقل قبل نشر الدورة.");
        }

        course.Status = CourseStatus.Published;
        course.UpdatedAt = DateTimeOffset.UtcNow;
        audit.Add(actorUserId, "CoursePublished", "Course", course.Id, $"تم نشر الدورة {course.Title}.", null, ip);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<AdminCourseDetailResponse>.Ok(MapCourse(course));
    }

    public async Task<ActionResult<AdminCourseDetailResponse>> UnpublishAsync(
        Guid id,
        Guid actorUserId,
        string? ip,
        CancellationToken cancellationToken = default)
    {
        var course = await LoadCourseGraphAsync(id, cancellationToken);
        if (course is null)
        {
            return ActionResult<AdminCourseDetailResponse>.Fail(404, "غير موجود", "الدورة غير موجودة.");
        }

        course.Status = CourseStatus.Draft;
        course.UpdatedAt = DateTimeOffset.UtcNow;
        audit.Add(actorUserId, "CourseUnpublished", "Course", course.Id, $"تم إلغاء نشر الدورة {course.Title}.", null, ip);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<AdminCourseDetailResponse>.Ok(MapCourse(course));
    }

    public async Task<ActionResult<AdminCourseDetailResponse>> ArchiveAsync(
        Guid id,
        Guid actorUserId,
        string? ip,
        CancellationToken cancellationToken = default)
    {
        var course = await LoadCourseGraphAsync(id, cancellationToken);
        if (course is null)
        {
            return ActionResult<AdminCourseDetailResponse>.Fail(404, "غير موجود", "الدورة غير موجودة.");
        }

        course.Status = CourseStatus.Archived;
        course.UpdatedAt = DateTimeOffset.UtcNow;
        audit.Add(actorUserId, "CourseArchived", "Course", course.Id, $"تم أرشفة الدورة {course.Title}.", null, ip);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<AdminCourseDetailResponse>.Ok(MapCourse(course));
    }

    public async Task<ActionResult<AdminSectionResponse>> CreateSectionAsync(
        Guid courseId,
        SaveSectionRequest request,
        CancellationToken cancellationToken = default)
    {
        var course = await dbContext.Courses
            .Include(item => item.Sections)
            .FirstOrDefaultAsync(item => item.Id == courseId, cancellationToken);
        if (course is null)
        {
            return ActionResult<AdminSectionResponse>.Fail(404, "غير موجود", "الدورة غير موجودة.");
        }

        var now = DateTimeOffset.UtcNow;
        var section = new CourseSection
        {
            Id = Guid.NewGuid(),
            CourseId = course.Id,
            Title = request.Title.Trim(),
            Description = EmptyToNull(request.Description),
            SortOrder = request.SortOrder ?? NextOrder(course.Sections.Select(item => item.SortOrder)),
            CreatedAt = now,
            UpdatedAt = now
        };
        dbContext.CourseSections.Add(section);
        course.UpdatedAt = now;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<AdminSectionResponse>.Ok(MapSection(section), 201);
    }

    public async Task<ActionResult<AdminSectionResponse>> UpdateSectionAsync(
        Guid sectionId,
        SaveSectionRequest request,
        CancellationToken cancellationToken = default)
    {
        var section = await dbContext.CourseSections
            .Include(item => item.Lessons)
            .Include(item => item.Course)
            .FirstOrDefaultAsync(item => item.Id == sectionId, cancellationToken);
        if (section is null)
        {
            return ActionResult<AdminSectionResponse>.Fail(404, "غير موجود", "القسم غير موجود.");
        }

        section.Title = request.Title.Trim();
        section.Description = EmptyToNull(request.Description);
        if (request.SortOrder.HasValue)
        {
            section.SortOrder = request.SortOrder.Value;
        }

        section.UpdatedAt = DateTimeOffset.UtcNow;
        section.Course.UpdatedAt = section.UpdatedAt;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<AdminSectionResponse>.Ok(MapSection(section));
    }

    public async Task<ActionResult> DeleteSectionAsync(Guid sectionId, CancellationToken cancellationToken = default)
    {
        var section = await dbContext.CourseSections
            .Include(item => item.Lessons)
            .Include(item => item.Course)
            .FirstOrDefaultAsync(item => item.Id == sectionId, cancellationToken);
        if (section is null)
        {
            return ActionResult.Fail(404, "غير موجود", "القسم غير موجود.");
        }

        if (section.Course.Status != CourseStatus.Draft)
        {
            return ActionResult.Fail(409, "تعارض", "لا يمكن حذف قسم إلا إذا كانت الدورة في حالة مسودة.");
        }

        var lessonIds = section.Lessons.Select(lesson => lesson.Id).ToList();
        if (lessonIds.Count > 0 &&
            await dbContext.LessonProgress.AsNoTracking().AnyAsync(progress => lessonIds.Contains(progress.LessonId), cancellationToken))
        {
            return ActionResult.Fail(409, "تعارض", "لا يمكن حذف قسم مرتبط بتقدم طلاب.");
        }

        dbContext.Lessons.RemoveRange(section.Lessons);
        dbContext.CourseSections.Remove(section);
        section.Course.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult.Success();
    }

    public async Task<ActionResult> ReorderSectionsAsync(
        Guid courseId,
        ReorderItemsRequest request,
        CancellationToken cancellationToken = default)
    {
        var courseExists = await dbContext.Courses.AnyAsync(course => course.Id == courseId, cancellationToken);
        if (!courseExists)
        {
            return ActionResult.Fail(404, "غير موجود", "الدورة غير موجودة.");
        }

        var ids = request.Items.Select(item => item.Id).ToList();
        var sections = await dbContext.CourseSections
            .Where(section => ids.Contains(section.Id))
            .ToListAsync(cancellationToken);

        if (sections.Count != ids.Count || sections.Any(section => section.CourseId != courseId))
        {
            return ActionResult.Fail(400, "طلب غير صالح", "كل الأقسام يجب أن تنتمي لنفس الدورة.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var orderMap = request.Items.ToDictionary(item => item.Id, item => item.SortOrder);
        foreach (var section in sections)
        {
            section.SortOrder = orderMap[section.Id];
            section.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ActionResult.Success();
    }

    public async Task<ActionResult<AdminLessonResponse>> CreateLessonAsync(
        Guid sectionId,
        SaveLessonRequest request,
        CancellationToken cancellationToken = default)
    {
        var section = await dbContext.CourseSections
            .Include(item => item.Lessons)
            .Include(item => item.Course)
            .FirstOrDefaultAsync(item => item.Id == sectionId, cancellationToken);
        if (section is null)
        {
            return ActionResult<AdminLessonResponse>.Fail(404, "غير موجود", "القسم غير موجود.");
        }

        var now = DateTimeOffset.UtcNow;
        var lesson = new Lesson
        {
            Id = Guid.NewGuid(),
            CourseSectionId = section.Id,
            Title = request.Title.Trim(),
            Description = EmptyToNull(request.Description),
            DurationSeconds = request.DurationSeconds,
            IsFreePreview = request.IsFreePreview,
            VideoProvider = ParseProvider(request.VideoProvider),
            VideoKey = EmptyToNull(request.VideoKey),
            SortOrder = request.SortOrder ?? NextOrder(section.Lessons.Select(item => item.SortOrder)),
            Status = LessonStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now
        };
        dbContext.Lessons.Add(lesson);
        section.Course.UpdatedAt = now;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<AdminLessonResponse>.Ok(MapLesson(lesson), 201);
    }

    public async Task<ActionResult<AdminLessonResponse>> UpdateLessonAsync(
        Guid lessonId,
        SaveLessonRequest request,
        CancellationToken cancellationToken = default)
    {
        var lesson = await dbContext.Lessons
            .Include(item => item.CourseSection)
            .ThenInclude(section => section.Course)
            .FirstOrDefaultAsync(item => item.Id == lessonId, cancellationToken);
        if (lesson is null)
        {
            return ActionResult<AdminLessonResponse>.Fail(404, "غير موجود", "الدرس غير موجود.");
        }

        lesson.Title = request.Title.Trim();
        lesson.Description = EmptyToNull(request.Description);
        lesson.DurationSeconds = request.DurationSeconds;
        lesson.IsFreePreview = request.IsFreePreview;
        lesson.VideoProvider = ParseProvider(request.VideoProvider);
        lesson.VideoKey = EmptyToNull(request.VideoKey);
        if (request.SortOrder.HasValue)
        {
            lesson.SortOrder = request.SortOrder.Value;
        }

        lesson.UpdatedAt = DateTimeOffset.UtcNow;
        lesson.CourseSection.Course.UpdatedAt = lesson.UpdatedAt;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<AdminLessonResponse>.Ok(MapLesson(lesson));
    }

    public Task<ActionResult<AdminLessonResponse>> PublishLessonAsync(Guid lessonId, CancellationToken cancellationToken = default) =>
        SetLessonStatusAsync(lessonId, LessonStatus.Published, cancellationToken);

    public Task<ActionResult<AdminLessonResponse>> ArchiveLessonAsync(Guid lessonId, CancellationToken cancellationToken = default) =>
        SetLessonStatusAsync(lessonId, LessonStatus.Archived, cancellationToken);

    public async Task<ActionResult> ReorderLessonsAsync(
        Guid sectionId,
        ReorderItemsRequest request,
        CancellationToken cancellationToken = default)
    {
        var sectionExists = await dbContext.CourseSections.AnyAsync(section => section.Id == sectionId, cancellationToken);
        if (!sectionExists)
        {
            return ActionResult.Fail(404, "غير موجود", "القسم غير موجود.");
        }

        var ids = request.Items.Select(item => item.Id).ToList();
        var lessons = await dbContext.Lessons
            .Where(lesson => ids.Contains(lesson.Id))
            .ToListAsync(cancellationToken);

        if (lessons.Count != ids.Count || lessons.Any(lesson => lesson.CourseSectionId != sectionId))
        {
            return ActionResult.Fail(400, "طلب غير صالح", "كل الدروس يجب أن تنتمي لنفس القسم.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var orderMap = request.Items.ToDictionary(item => item.Id, item => item.SortOrder);
        foreach (var lesson in lessons)
        {
            lesson.SortOrder = orderMap[lesson.Id];
            lesson.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ActionResult.Success();
    }

    private async Task<ActionResult<AdminLessonResponse>> SetLessonStatusAsync(
        Guid lessonId,
        LessonStatus status,
        CancellationToken cancellationToken)
    {
        var lesson = await dbContext.Lessons
            .Include(item => item.CourseSection)
            .ThenInclude(section => section.Course)
            .FirstOrDefaultAsync(item => item.Id == lessonId, cancellationToken);
        if (lesson is null)
        {
            return ActionResult<AdminLessonResponse>.Fail(404, "غير موجود", "الدرس غير موجود.");
        }

        lesson.Status = status;
        lesson.UpdatedAt = DateTimeOffset.UtcNow;
        lesson.CourseSection.Course.UpdatedAt = lesson.UpdatedAt;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ActionResult<AdminLessonResponse>.Ok(MapLesson(lesson));
    }

    private Task<Course?> LoadCourseGraphAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Courses
            .Include(course => course.Sections)
            .ThenInclude(section => section.Lessons)
            .FirstOrDefaultAsync(course => course.Id == id, cancellationToken);

    private Task<bool> SlugTakenAsync(string slug, Guid? exceptCourseId, CancellationToken cancellationToken) =>
        dbContext.Courses.AnyAsync(
            course => course.Slug == slug && (!exceptCourseId.HasValue || course.Id != exceptCourseId.Value),
            cancellationToken);

    private static PreparedCourse PrepareCourseFields(SaveCourseRequest request)
    {
        if (!Enum.TryParse<CourseLevel>(request.Level, true, out var level))
        {
            return PreparedCourse.Fail(400, "طلب غير صالح", "مستوى الدورة غير صالح.");
        }

        if (!Enum.TryParse<CourseAccessType>(request.AccessType, true, out var accessType))
        {
            return PreparedCourse.Fail(400, "طلب غير صالح", "نوع الوصول غير صالح.");
        }

        var slug = CourseSlug.Normalize(request.Slug);
        if (!CourseSlug.IsValid(slug))
        {
            return PreparedCourse.Fail(400, "طلب غير صالح", "مسار الدورة غير صالح.");
        }

        var duration = accessType == CourseAccessType.Lifetime ? null : request.AccessDurationDays;
        if (accessType == CourseAccessType.LimitedDuration && (duration is null or <= 0))
        {
            return PreparedCourse.Fail(400, "طلب غير صالح", "مدة الوصول مطلوبة للدورات محدودة المدة.");
        }

        return new PreparedCourse
        {
            Succeeded = true,
            CourseTitle = request.Title.Trim(),
            Slug = slug,
            ShortDescription = request.ShortDescription.Trim(),
            Description = request.Description.Trim(),
            Level = level,
            AccessType = accessType,
            AccessDurationDays = duration
        };
    }

    private static ActionResult<T> Fail<T>(PreparedCourse prepared) =>
        ActionResult<T>.Fail(prepared.StatusCode, prepared.ErrorTitle ?? "خطأ", prepared.Detail ?? "تعذر إكمال الطلب.");

    private static AdminCourseDetailResponse MapCourse(Course course) =>
        new()
        {
            Id = course.Id,
            Title = course.Title,
            Slug = course.Slug,
            ShortDescription = course.ShortDescription,
            Description = course.Description,
            PriceIQD = course.PriceIQD,
            ThumbnailUrl = course.ThumbnailUrl,
            TrailerUrl = course.TrailerUrl,
            Level = course.Level.ToString(),
            Status = course.Status.ToString(),
            IsFeatured = course.IsFeatured,
            AccessType = course.AccessType.ToString(),
            AccessDurationDays = course.AccessDurationDays,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt,
            Sections = course.Sections
                .OrderBy(section => section.SortOrder)
                .Select(MapSection)
                .ToList()
        };

    private static AdminSectionResponse MapSection(CourseSection section) =>
        new()
        {
            Id = section.Id,
            CourseId = section.CourseId,
            Title = section.Title,
            Description = section.Description,
            SortOrder = section.SortOrder,
            Lessons = section.Lessons
                .OrderBy(lesson => lesson.SortOrder)
                .Select(MapLesson)
                .ToList()
        };

    private static AdminLessonResponse MapLesson(Lesson lesson) =>
        new()
        {
            Id = lesson.Id,
            SectionId = lesson.CourseSectionId,
            Title = lesson.Title,
            Description = lesson.Description,
            DurationSeconds = lesson.DurationSeconds,
            SortOrder = lesson.SortOrder,
            IsFreePreview = lesson.IsFreePreview,
            Status = lesson.Status.ToString(),
            VideoProvider = lesson.VideoProvider.ToString(),
            VideoKey = lesson.VideoKey,
            VideoProcessingStatus = string.IsNullOrWhiteSpace(lesson.VideoKey) ? null : "Ready",
            VideoProcessingMessage = string.IsNullOrWhiteSpace(lesson.VideoKey) ? null : "الفيديو جاهز."
        };

    private async Task<AdminCourseDetailResponse> AttachVideoStatusesAsync(
        AdminCourseDetailResponse course,
        CancellationToken cancellationToken)
    {
        var lessonIds = course.Sections.SelectMany(section => section.Lessons).Select(lesson => lesson.Id).ToList();
        if (lessonIds.Count == 0)
        {
            return course;
        }

        var jobs = await dbContext.VideoTranscodeJobs.AsNoTracking()
            .Where(item => lessonIds.Contains(item.LessonId))
            .ToListAsync(cancellationToken);
        var byLesson = jobs
            .GroupBy(item => item.LessonId)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(item => item.CreatedAt).First());

        var sections = course.Sections.Select(section => new AdminSectionResponse
        {
            Id = section.Id,
            CourseId = section.CourseId,
            Title = section.Title,
            Description = section.Description,
            SortOrder = section.SortOrder,
            Lessons = section.Lessons.Select(lesson =>
            {
                if (!byLesson.TryGetValue(lesson.Id, out var job))
                {
                    return lesson;
                }

                var status = job.Status switch
                {
                    VideoTranscodeStatus.Queued => "Queued",
                    VideoTranscodeStatus.Processing => "Processing",
                    VideoTranscodeStatus.Ready => "Ready",
                    VideoTranscodeStatus.Failed => "Failed",
                    _ => job.Status.ToString()
                };
                var message = job.Status switch
                {
                    VideoTranscodeStatus.Queued => "بانتظار المعالجة...",
                    VideoTranscodeStatus.Processing => "جاري تحويل الفيديو لجودة متعددة...",
                    VideoTranscodeStatus.Ready => "الفيديو جاهز للتشغيل بجودة تلقائية.",
                    VideoTranscodeStatus.Failed => job.ErrorMessage ?? "فشلت المعالجة.",
                    _ => null
                };

                return new AdminLessonResponse
                {
                    Id = lesson.Id,
                    SectionId = lesson.SectionId,
                    Title = lesson.Title,
                    Description = lesson.Description,
                    DurationSeconds = lesson.DurationSeconds,
                    SortOrder = lesson.SortOrder,
                    IsFreePreview = lesson.IsFreePreview,
                    Status = lesson.Status,
                    VideoProvider = lesson.VideoProvider,
                    VideoKey = lesson.VideoKey,
                    VideoProcessingStatus = status,
                    VideoProcessingMessage = message
                };
            }).ToList()
        }).ToList();

        return new AdminCourseDetailResponse
        {
            Id = course.Id,
            Title = course.Title,
            Slug = course.Slug,
            ShortDescription = course.ShortDescription,
            Description = course.Description,
            PriceIQD = course.PriceIQD,
            ThumbnailUrl = course.ThumbnailUrl,
            TrailerUrl = course.TrailerUrl,
            Level = course.Level,
            Status = course.Status,
            IsFeatured = course.IsFeatured,
            AccessType = course.AccessType,
            AccessDurationDays = course.AccessDurationDays,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt,
            Sections = sections
        };
    }

    private static VideoProvider ParseProvider(string? value) =>
        Enum.TryParse<VideoProvider>(value, true, out var provider) ? provider : VideoProvider.None;

    private static int NextOrder(IEnumerable<int> existing) =>
        existing.DefaultIfEmpty(0).Max() + 1;

    private static string? EmptyToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed class PreparedCourse
    {
        public bool Succeeded { get; init; }

        public int StatusCode { get; init; } = 400;

        public string? ErrorTitle { get; init; }

        public string? Detail { get; init; }

        public string CourseTitle { get; init; } = string.Empty;

        public string Slug { get; init; } = string.Empty;

        public string ShortDescription { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public CourseLevel Level { get; init; }

        public CourseAccessType AccessType { get; init; }

        public int? AccessDurationDays { get; init; }

        public static PreparedCourse Fail(int statusCode, string title, string detail) =>
            new() { Succeeded = false, StatusCode = statusCode, ErrorTitle = title, Detail = detail };
    }
}
