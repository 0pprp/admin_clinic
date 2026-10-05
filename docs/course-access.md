# صلاحية الوصول إلى الدورات

مصدر الحقيقة الوحيد هو `ICourseAccessService`. لا يُكرَّر منطق الوصول في الواجهة أو في الـ Endpoints.

## البنية

```text
Course
  └── CourseSection (SortOrder)
        └── Lesson (SortOrder)
```

الاستعلامات العامة وعرض المنهج يحترمان هذا الترتيب.

## Free Preview

إذا كان الدرس `Published` و`IsFreePreview = true` والدورة الأم `Published`:

- يُسمح بالمعاينة العامة حتى للزائر غير المسجّل.
- Endpoint: `GET /api/public/lessons/{lessonId}/preview`
- لا يُرجع `VideoKey`.

إذا كانت الدورة `Archived` أو `Draft` فلا توجد معاينة عامة، حتى لو الدرس مجانياً.

## Active Enrollment

الوصول إلى الدروس المدفوعة يتطلب `CourseEnrollment` بحيث:

- `UserId` و`CourseId` مطابقان
- `Status == Active`
- `ExpiresAt == null` أو `ExpiresAt > UtcNow`

لا نغيّر `Status` إلى `Expired` عند القراءة. المنطق يعامل السجل المنتهي كغير صالح.

## Lifetime و LimitedDuration

- Lifetime: Enrollment بلا تاريخ انتهاء عادةً.
- LimitedDuration: يُعرض في الواجهة كوصول لمدة X يوماً بعد التفعيل. التحقق الفعلي يتم عبر `ExpiresAt` على الـ Enrollment.

## Suspended / Revoked / Expired

أي حالة غير `Active` لا تمنح وصولاً. كذلك `Active` مع `ExpiresAt` في الماضي.

## Archived Course

الأرشفة تعني:

- اختفاء الدورة من الكتالوج العام
- منع المبيعات الجديدة
- **عدم** سحب وصول الطلاب الذين لديهم Enrollment فعال

الطالب المسجّل يبقى قادراً على المنهج المحمي عبر `GET /api/courses/{courseId}/outline`.

## Protected Lesson

- زائر غير مسجّل يطلب درساً مدفوعاً: `401`
- مستخدم مسجّل بلا Enrollment: `403`
- درس غير موجود أو مسودة/مؤرشف لغير الإدارة: `404`
- الإدارة (`Admin` / `ContentManager`) تُمنح وصولاً عبر الدور وليس عبر Enrollment

الواجهة قد تعرض قفلاً، لكن Backend يمنع `GET /api/lessons/{id}` و`GET /api/lessons/{id}/playback` بدون صلاحية.

التشغيل الفعلي:

```text
User → Authentication → Lesson → ICourseAccessService
  → Active CourseEnrollment → IVideoPlaybackService → short-lived URL
```

المزود الموصى به Bunny Stream. انظر `docs/video.md`.

## Unpublish

`POST /api/admin/courses/{id}/unpublish` يعيد الدورة إلى `Draft` لأن النظام لا يملك حالة Unpublished منفصلة.

## حذف الأقسام

يُسمح بالحذف فقط إذا كانت الدورة `Draft` ولا يوجد `LessonProgress` على دروس القسم. الحذف لا يُستخدم على الدورات المنشورة أو المؤرشفة حتى لا تُفقد بيانات تاريخية.

## VideoKey

`VideoKey` يُعامل كمفتاح كائن للتخزين وليس كرمز سري. يُرجع في Admin Detail فقط إذا لزم للإدارة، ولا يظهر في Public APIs أو Playback. إذا أصبح لاحقاً token سرياً فلا يُخزَّن في جدول الدروس أصلاً.

## المستوى (Level)

المستوى Enum في الـ Domain (`Beginner` / `Intermediate` / `Advanced`). الواجهة العامة تعرضه كنص عربي عبر helper دون Migration إضافية.
