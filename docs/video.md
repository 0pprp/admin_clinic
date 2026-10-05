# Video Provider

تشغيل الدروس المحمية يبقى خلف:

```text
User → Authentication → Lesson → ICourseAccessService
  → Active CourseEnrollment → IVideoPlaybackService
  → Short-lived Playback Token/URL
```

معرفة `LessonId` وحدها لا تكفي.

## Decision

التوصية للإصدار الأول: **Bunny Stream**.

| | السعر | سهولة التنفيذ | العراق / المنطقة | الحماية | Streaming | Signed playback |
| --- | --- | --- | --- | --- | --- | --- |
| Cloudflare Stream | أعلى من Bunny عادةً للحجم المتوسط | جيد، مرتبط بحساب Cloudflare | CDN عالمي قوي | جيد | أصل للبث | نعم |
| **Bunny Stream** | منخفض وواضح حسب الدقائق/التخزين | Embed + token بسيط | نقاط CDN في الشرق الأوسط مناسبة | جيد مع توكن قصير | أصل للبث | نعم (token + expiry) |
| Vimeo | مرتفع للخطط التي تسمح بإخفاء الصفحة العامة | سهل للعرض، أصعب للتحكم البرمجي | جيد عالمياً | يعتمد على الخطة | أصل للبث | محدود/أغلى |
| Cloudflare R2 | تخزين رخيص | ليس مشغّل فيديو. تحتاج ترميز + مشغل + توقيع | تخزين فقط | يجب بناؤه | لا | عليك تنفيذه |

R2 مناسب لأرشفة الملفات وليس بديلاً عن منصة بث في الإصدار الأول. Vimeo مكلف للتحكم المغلق. Cloudflare Stream ممتاز لكن أعقد/أغلى للبداية. Bunny Stream يعطي embed موقَّع، سعراً أقل، وتنفيذاً أقصر مع `IVideoPlaybackService`.

## Implementation

- `BunnyStreamPlaybackService` يقرأ `Video:Bunny:*` من البيئة
- إن كان المزود `Unavailable` أو الإعداد ناقصاً في Development → `UnavailableVideoPlaybackService`
- في Production إذا `Video:Provider=BunnyStream` بدون `LibraryId`/`TokenKey` يفشل Startup
- `Video:TokenLifetimeMinutes` افتراضي 10
- لا تخزين لـ Signed URL في قاعدة البيانات
- اللوج يسجّل `LessonId` ووقت الانتهاء فقط، وليس الرابط

متغيرات البيئة:

```text
Video__Provider=BunnyStream
Video__TokenLifetimeMinutes=10
Video__Bunny__LibraryId=
Video__Bunny__CdnHostname=iframe.mediadelivery.net
Video__Bunny__TokenKey=
```
