# Browser Production QA

Cursor browser غير مستخدم هنا. نفّذ هذه القائمة يدوياً في Chromium على `https://example.com` (أو النطاق الحقيقي بعد ربطه) قبل الإطلاق.

استخدم نافذة خاصة. سجّل أي خطأ ثم أوقفه قبل الإعلان عن الإطلاق.

## قبل البدء

- [ ] النطاق يفتح عبر HTTPS بدون تحذير شهادة
- [ ] `https://example.com/api/public/courses` يعمل (نفس النطاق)
- [ ] `https://example.com/health/live` و`/health/ready` يعيدان نجاحاً

## Console / Network

- [ ] لا أخطاء Console حمراء في الصفحة الرئيسية
- [ ] لا Hydration warnings
- [ ] لا استجابات Network 500 على المسارات العامة
- [ ] لا طلبات إلى `localhost` أو `127.0.0.1` أو منفذ Kestrel

## CORS / Cookies / Redirects

- [ ] تسجيل الدخول من نفس النطاق ينجح
- [ ] Cookie `mr_access` و`mr_refresh`: HttpOnly، Secure، SameSite=Lax
- [ ] لا حلقة Redirect بين HTTP وHTTPS
- [ ] Origin غير مسموح لا يحصل على `Access-Control-Allow-Origin`

## Admin

- [ ] دخول Admin
- [ ] لوحة التحكم
- [ ] قائمة طلبات الشراء
- [ ] الطلاب والتسجيلات
- [ ] إدارة المحتوى والإعدادات

## Student

- [ ] التسجيل/الدخول
- [ ] إنشاء طلب شراء يدوي
- [ ] بعد التفعيل: دوراتي والمنهج
- [ ] درس محمي بدون تسجيل يظهر رفضاً وليس الفيديو
- [ ] تشغيل الدرس عبر iframe Bunny بعد وجود Enrollment فعّال
- [ ] الاستشارات ونموذج التواصل

## Course playback

- [ ] لا يظهر `videoKey` في استجابة JSON العامة (Network)
- [ ] رابط التشغيل قصير العمر ويعمل داخل الصفحة
- [ ] طالب بلا enrollment لا يشغّل الدرس المدفوع

## Mobile

- [ ] عرض هاتف (~390px): قائمة، تسجيل دخول، درس، لوحة تحكم
- [ ] لا قصّ للنصوص العربية RTL

## بعد الجولة

- [ ] لا أسرار في Network headers أو HTML
- [ ] Correlation ID يظهر في استجابة خطأ إن تم اختبار 401/403
