# المصادقة والأمان

منصة محمد رؤوف تستخدم Identity مع JWT قصير العمر وRefresh Tokens دوّارة داخل HttpOnly Cookies.

## JWT Signing Key

لا تضع مفتاح الإنتاج داخل Git.

للتطوير المحلي:

```bash
dotnet user-secrets init --project src/MohammedRaouf.Api
dotnet user-secrets set "Jwt:SigningKey" "replace-with-a-long-dev-only-key!!" --project src/MohammedRaouf.Api
```

أو عبر متغير بيئة:

```bash
Jwt__SigningKey=replace-with-a-long-dev-only-key!!
```

## Development Admin Seed

حساب المدير المحلي يُنشأ فقط في بيئة Development عبر:

- `SEED_ADMIN_EMAIL`
- `SEED_ADMIN_PASSWORD`

لا تضع كلمة المرور في المصدر أو `appsettings.json`. استخدم user-secrets أو متغيرات البيئة:

```bash
dotnet user-secrets set "SEED_ADMIN_EMAIL" "dev-admin@localhost.test" --project src/MohammedRaouf.Api
dotnet user-secrets set "SEED_ADMIN_PASSWORD" "use-a-local-only-password" --project src/MohammedRaouf.Api
```

العملية idempotent: إن وُجد البريد مسبقاً لا يُنشأ حساب مكرر. خارج Development تُرفض العملية ما لم يُضبط `SEED_ADMIN_ALLOW_NON_DEVELOPMENT=true` صراحة.

الحد الأدنى لطول المفتاح: 32 بايت UTF-8. في Production يرفض الإقلاع إذا كان المفتاح مفقوداً أو ضعيفاً أو مساوياً لـ placeholder التطوير. المفتاح لا يُطبع في اللوج.

ملف `appsettings.Development.json` يحتوي مفتاح تطوير وهمي فقط لتسهيل التشغيل المحلي والاختبارات. مفتاح الإنتاج يُحقن من البيئة أو من نظام الأسرار.

## Cookies

- `mr_access`: Access Token
- `mr_refresh`: Refresh Token
- HttpOnly = true
- SameSite = Lax
- Secure = true في Production
- Secure = false في Development على HTTP المحلي

## Frontend Proxy

في التطوير يعيد Next.js كتابة `/api/*` إلى `http://127.0.0.1:5000` حتى تبقى الكوكيز طرفاً أول.

لا تخزّن التوكنات في `localStorage` أو `sessionStorage`.
