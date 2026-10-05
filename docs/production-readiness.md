# Production Readiness

هذا الدليل لإطلاق الإصدار الأول. الدفع الإلكتروني ليس متطلباً؛ المنصة تعتمد **Manual Payment**. الشهادات التعليمية ميزة مستقبلية وليست blocker.

لا يُنفَّذ من هذا الملف: `git push`، تحديث قاعدة إنتاج، تغيير DNS، أو نشر حقيقي.

## Production blockers البرمجية التي عولجت في الكود

- JWT production secret (فشل الإقلاع إن نُقص أو ضعف)
- Production database connection (من البيئة فقط، بدون localhost)
- `App:PublicUrl` بنطاق `https://`
- CORS من الإعدادات مع Credentials
- HTTPS + HSTS بعد Forwarded Headers
- Reverse proxy / `X-Forwarded-Proto`
- Video provider (Bunny Stream) كتجريد جاهز
- إعداد بيئة الإنتاج
- إجراء Migrations بدون `Database.Migrate()` عند الإقلاع
- قائمة Browser QA اليدوية

## Required environment variables

لا تضع أسراراً حقيقية في Git. انظر `.env.production.example`.

| Variable | Required | Notes |
| --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | نعم | `Production` |
| `ConnectionStrings__DefaultConnection` | نعم | PostgreSQL مُدار + `Ssl Mode=Require` |
| `Jwt__Issuer` | نعم | |
| `Jwt__Audience` | نعم | |
| `Jwt__SigningKey` | نعم | ≥ 32 بايت UTF-8، ليس مفتاح التطوير |
| `Jwt__AccessTokenMinutes` | لا | الافتراضي 15 |
| `Jwt__RefreshTokenDays` | لا | الافتراضي 14 |
| `App__PublicUrl` | نعم | `https://example.com` (بدون localhost) |
| `Cors__AllowedOrigins__0` | نعم | أصل HTTPS مطابق للنطاق العام |
| `Email__Provider` | نعم | `Disabled` حتى يتوفر SMTP. `Logging` مرفوض |
| `Video__Provider` | نعم | `Unavailable` أو `BunnyStream` |
| `Video__TokenLifetimeMinutes` | لا | الافتراضي 10 |
| `Video__Bunny__LibraryId` | إذا BunnyStream | |
| `Video__Bunny__CdnHostname` | لا | `iframe.mediadelivery.net` |
| `Video__Bunny__TokenKey` | إذا BunnyStream | لا يُسجَّل في اللوج |
| `ForwardedHeaders__AllowAllProxies` | خلف Caddy | `true` فقط إذا Kestrel غير منشور للعامة |
| `NEXT_PUBLIC_SITE_URL` | للواجهة | قيمة عامة للـ SEO فقط |

لا تضع أسرار Backend في أي متغير يبدأ بـ `NEXT_PUBLIC_`.

المتصفح يستخدم `https://example.com/api/*` عبر Caddy. لا يحتاج منفذ Kestrel.

## Build commands

من جذر المستودع:

```powershell
dotnet publish src/MohammedRaouf.Api/MohammedRaouf.Api.csproj -c Release
cd apps/web
npm ci
npm run build
```

Docker:

```powershell
docker build -f Dockerfile.api -t mohammed-raouf-api .
docker build -f apps/web/Dockerfile -t mohammed-raouf-web .
```

`docker-compose.production.example.yml` مثال فقط.

## Migration commands

لا يُستدعى `Database.Migrate()` عند إقلاع التطبيق.

Windows PowerShell، من جذر المشروع، على قاعدة الإنتاج بعد ضبط `ConnectionStrings__DefaultConnection`:

```powershell
dotnet ef database update `
  --project src/MohammedRaouf.Infrastructure `
  --startup-project src/MohammedRaouf.Api
```

التحقق:

```powershell
dotnet ef migrations list `
  --project src/MohammedRaouf.Infrastructure `
  --startup-project src/MohammedRaouf.Api
```

آخر migration المتوقعة: `AddAdminAndConsultationWorkflowSupport`.

## Backup

قبل كل migration:

```powershell
$env:PGPASSWORD = "CHANGE_ME"
pg_dump -h CHANGE_ME -p 5432 -U CHANGE_ME -d mohammed_raouf -Fc -f backup.dump
Remove-Item Env:PGPASSWORD
```

الاستعادة:

```powershell
$env:PGPASSWORD = "CHANGE_ME"
pg_restore -h CHANGE_ME -p 5432 -U CHANGE_ME -d mohammed_raouf --clean --if-exists backup.dump
Remove-Item Env:PGPASSWORD
```

لا تلصق كلمة المرور داخل الأمر حتى لا تبقى في تاريخ PowerShell.

سياسة مقترحة (ليست خدمة منفَّذة): نسخ يومي، احتفاظ 7–30 يوماً، نسخة قبل كل migration، اختبار استعادة شهري.

SSL لـ PostgreSQL المُدار: `Ssl Mode=Require;Trust Server Certificate=false`. لا تعطّل التحقق من شهادة الخادم في الإنتاج.

## Start order

1. Backup database
2. Stop/prepare deployment
3. Apply migrations
4. Verify migrations list
5. Start API ثم Next.js ثم Caddy (أو عبر compose)
6. Health check: `/health/live` ثم `/health/ready`

## Health checks

| Path | Meaning |
| --- | --- |
| `/health` | Live JSON للتوافق الحالي |
| `/health/live` | العملية تعمل. لا يعتمد على الفيديو |
| `/health/ready` | العملية + PostgreSQL |

لا تُعرض أسرار قاعدة البيانات في الاستجابة. تعطل Bunny Stream المؤقت لا يجعل التطبيق Unlive.

## Rollback

1. أوقف النسخة الجديدة
2. أعد تشغيل الحاويات/العملية السابقة المعروفة
3. إن كانت migration غير متوافقة للخلف، استعد من `pg_restore` للنسخة المأخوذة قبل التحديث
4. تحقق `/health/ready`

لا يوجد rollback تلقائي للـ schema.

## Admin bootstrap

1. أنشئ كلمة مرور قوية مؤقتة خارج Git
2. عيّن `SEED_ADMIN_EMAIL` و`SEED_ADMIN_PASSWORD` و`SEED_ADMIN_ALLOW_NON_DEVELOPMENT=true`
3. شغّل الـ API مرة واحدة
4. سجّل الدخول وتحقق من دور Admin
5. احذف متغيرات الـ seed من البيئة
6. أعد التشغيل إن لزم

بدون `SEED_ADMIN_ALLOW_NON_DEVELOPMENT=true` يرفض الإنتاج الإقلاع إذا بقيت `SEED_ADMIN_PASSWORD`. لا تترك كلمة مرور المدير في env بشكل دائم.

## Video configuration

المزود الموصى به: **Bunny Stream**. التجريد يبقى `IVideoPlaybackService`.

- Development بدون إعدادات Bunny → `UnavailableVideoPlaybackService`
- Production مع `Video:Provider=BunnyStream` بدون LibraryId/TokenKey → فشل الإقلاع
- التوكن قصير العمر (`Video:TokenLifetimeMinutes`، افتراضي 10)
- لا يُخزَّن Signed URL في قاعدة البيانات ولا يُسجَّل في اللوج
- التشغيل يتطلب مصادقة + `ICourseAccessService` + Enrollment فعّال

التفاصيل: [docs/video.md](video.md)

## Security notes

- JWT: مفتاح الإنتاج ≥ 32 بايت وليس placeholder التطوير. لا يُطبع المفتاح.
- Cookies: HttpOnly + Secure + SameSite=Lax في Production. Development على HTTP: Secure=false
- CORS: أصول HTTPS من الإعدادات + AllowCredentials. ممنوع AllowAnyOrigin مع Credentials
- HTTPS redirection وHSTS بعد Forwarded Headers حتى لا يحدث redirect loop خلف Caddy
- الـ proxy يجب أن يمرر `X-Forwarded-For` و`X-Forwarded-Proto`
- CSP محسوبة للـ Next.js وBunny embed. الـ API يستخدم CSP ضيقة (`default-src 'none'`)
- ProblemDetails: title/status/detail آمن + correlationId. بدون StackTrace أو SQL أو أسرار
- Serilog لا يسجّل كلمات المرور، JWT، RefreshToken، أكواد التفعيل، توكن إعادة التعيين، أو Signed URL
- Rate limits الحالية موثّقة في [docs/rate-limiting.md](rate-limiting.md)

## Reverse proxy

التوصية للإصدار الأول: **Caddy** بسبب HTTPS التلقائي وReverse Proxy البسيط. انظر `Caddyfile.example`.

Kestrel يستمع HTTP داخلياً على `:8080` ويجب ألا يُنشر للعامة.

## Browser QA

نفّذ [docs/browser-production-qa.md](browser-production-qa.md) يدوياً في Chromium قبل الإطلاق.

Smoke (بدون أسرار في الملف):

```powershell
.\scripts\smoke-test.ps1 -ApiBaseUrl http://localhost:5000 -WebBaseUrl http://localhost:3000
```
