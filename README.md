# منصة محمد رؤوف

منصة البراند الشخصي والدورات والاستشارات لمحمد رؤوف.

السوق الأساسي: العراق  
اللغة: العربية (RTL)  
العملة: الدينار العراقي (IQD)

هذا المستودع في مرحلة الأساس مع طبقة المصادقة جاهزة، ومسار الإنتاج موثّق بدون نشر فعلي.

## التقنيات

### Backend

- ASP.NET Core Web API (NET 10)
- C#
- Serilog
- OpenAPI + Swagger UI

### Frontend

- Next.js
- TypeScript
- App Router
- Tailwind CSS

### Database

- PostgreSQL 16 عبر Docker
- منفذ المضيف: `5433` (لتجنب تعارض PostgreSQL المحلي على 5432)
- قاعدة التطوير: `mohammed_raouf_dev`
- المستخدم المحلي: `mr_dev`

## المتطلبات

تحقق أولاً من الأدوات:

```bash
dotnet --version
node --version
npm --version
git --version
docker --version
docker compose version
```

## إعداد ملف البيئة

انسخ المثال ثم عدّل كلمة مرور التطوير محلياً:

```bash
cp .env.example .env
```

على Windows PowerShell:

```powershell
Copy-Item .env.example .env
```

لا تضع كلمة مرور إنتاج داخل Git. ملف `.env` موجود في `.gitignore`.

## تشغيل PostgreSQL

من جذر المشروع:

```bash
docker compose up -d
docker compose ps
```

انتظر حتى تصبح الحالة `healthy`.

اختبار الاتصال من داخل الحاوية:

```bash
docker compose exec postgres psql -U mr_dev -d mohammed_raouf_dev
```

ثم:

```sql
SELECT current_database();
SELECT version();
```

للخروج: `\q`

## قاعدة البيانات و Migrations

من جذر المشروع:

```bash
dotnet ef database update --project src/MohammedRaouf.Infrastructure --startup-project src/MohammedRaouf.Api
```

سلسلة الاتصال المحلية تستخدم المنفذ `5433`.

## تشغيل Backend

من جذر المشروع:

```bash
dotnet run --project src/MohammedRaouf.Api
```

- الصحة: [http://localhost:5000/health](http://localhost:5000/health)
- OpenAPI: [http://localhost:5000/openapi/v1.json](http://localhost:5000/openapi/v1.json)
- Swagger UI في التطوير: [http://localhost:5000/swagger](http://localhost:5000/swagger)

إعداد مفتاح JWT موضح في [docs/authentication.md](docs/authentication.md).

Frontend في التطوير يوجّه `/api/*` إلى Backend حتى تبقى الكوكيز على نفس الموقع.

صفحات المصادقة:

- [http://localhost:3000/login](http://localhost:3000/login)
- [http://localhost:3000/register](http://localhost:3000/register)
- [http://localhost:3000/forgot-password](http://localhost:3000/forgot-password)
- [http://localhost:3000/reset-password](http://localhost:3000/reset-password)
- [http://localhost:3000/dashboard](http://localhost:3000/dashboard) (محمية)

## تشغيل Frontend

```bash
cd apps/web
npm install
npm run dev
```

ثم افتح العنوان الذي يظهره Next.js، عادة [http://localhost:3000](http://localhost:3000).

## تشغيل الاختبارات

من جذر المشروع:

```bash
dotnet test
```

للواجهة:

```bash
cd apps/web
npm run lint
npm run build
```

## أوامر Docker المهمة

```bash
docker compose up -d
docker compose ps
docker compose logs postgres
docker compose down
```

تحذير: الأمر التالي يحذف بيانات قاعدة التطوير لأنها تزيل الـ volume:

```bash
docker compose down -v
```

استخدمه فقط إذا كنت تريد إعادة قاعدة التطوير من الصفر.

## Production

الدفع في الإصدار الأول يدوي (Manual Payment). الشهادات ليست جزءاً من الإطلاق.

لا تضع أسراراً في Git. انسخ `.env.production.example` إلى مخزن أسرار، ثم راجع [docs/production-readiness.md](docs/production-readiness.md).

الخلاصة التشغيلية:

```text
Internet → Caddy (HTTPS)
  /        → Next.js
  /api/*   → ASP.NET Core
```

المتصفح يرى `https://example.com` و`https://example.com/api` حتى تبقى الكوكيز على نفس الموقع.

أوامر مفيدة (بدون نشر حقيقي من هنا):

```powershell
dotnet publish src/MohammedRaouf.Api/MohammedRaouf.Api.csproj -c Release

dotnet ef database update `
  --project src/MohammedRaouf.Infrastructure `
  --startup-project src/MohammedRaouf.Api

dotnet ef migrations list `
  --project src/MohammedRaouf.Infrastructure `
  --startup-project src/MohammedRaouf.Api

.\scripts\smoke-test.ps1
```

آخر migration المتوقعة: `AddAdminAndConsultationWorkflowSupport`.

الفيديو الموصى به: Bunny Stream. التفاصيل في [docs/video.md](docs/video.md). قائمة فحص المتصفح: [docs/browser-production-qa.md](docs/browser-production-qa.md).

## هيكل المجلدات

```text
src/           مشاريع ASP.NET Core
tests/         اختبارات الوحدة والتكامل
apps/web       واجهة Next.js
docs/          توثيق المعمارية
```
