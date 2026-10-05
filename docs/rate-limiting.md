# Rate limiting

السياسات الحالية لم تُغيَّر إلا حيث لزم التوثيق. القيم من `RateLimiting` في `appsettings.json` (Development يخفّف حد التسجيل فقط).

| Policy | Limit | Window | Endpoints |
| --- | --- | --- | --- |
| login | 5 | 60s | `POST /api/auth/login` |
| register | 3 | 600s | `POST /api/auth/register` |
| forgot-password | 3 | 900s | `POST /api/auth/forgot-password` |
| reset-password | 5 | 600s | `POST /api/auth/reset-password` |
| refresh | 30 | 60s | `POST /api/auth/refresh` |
| activation-redeem | 5 | 60s | تفعيل الرمز (مع المستخدم) |
| lesson-progress | 120 | 60s | تحديث تقدم الدرس (مع المستخدم) |
| consultation | 3 | 900s | طلب استشارة |
| contact | 5 | 600s | نموذج التواصل |

المفتاح: عنوان IP، أو `userId:IP` عندما يكون المستخدم مصادقاً (activation/progress).

استجابة التجاوز: `429` مع ProblemDetails عربي وبدون أسرار.
