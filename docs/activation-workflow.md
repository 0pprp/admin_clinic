# مسار التفعيل

امتلاك الدورة يتم عبر `CourseEnrollment` فقط، وليس عبر `PurchaseRequest` أو `ActivationCode` أو دور Student أو `ApplicationUser.IsActivated`.

كل Enrollment مستقل لكل زوج `(UserId, CourseId)`. تفعيل الدورة B لا يحذف دورة A ولا يستبدلها ولا يعطّلها.

## العلاقة بين الكيانات

`PurchaseRequest` يثبت الدفع اليدوي. بعد `PaymentReceived` يمكن التفعيل بإحدى طريقتين:

1. إصدار `ActivationCode` ثم استرداده من الطالب.
2. تفعيل مباشر من الإدارة.

النتيجة في الحالتين: صف `CourseEnrollment` بحالة `Active`.

الكود مربوط بـ `UserId` + `CourseId` + `PurchaseRequestId`. لا يُخزَّن النص الأصلي في قاعدة البيانات، بل `CodeHash` فقط.

## إصدار الكود

`POST /api/admin/purchase-requests/{id}/issue-activation-code`

السياسة: `ManageActivations` (`Admin` + `Support`).

يُسمح فقط إذا كانت الحالة `PaymentReceived` (أو `ActivationCodeIssued` بعد انتهاء كود سابق زمنياً)، ولا يوجد كود `Active` غير منتهٍ لنفس الطلب. وجود كود نشط يعيد `409`؛ يجب `Revoke` ثم إصدار جديد.

داخل معاملة:

- قفل صف الطلب.
- توليد كود بالصيغة `MR-XXXX-XXXX` عبر `RandomNumberGenerator` وأبجدية بدون `0 O 1 I L`.
- حساب SHA-256 للنص المعياري.
- إنشاء `ActivationCode`.
- `PaymentReceived → ActivationCodeIssued`.
- `PurchaseRequestEvent` + `AdminAuditLog` بفعل `ActivationCodeIssued` بدون النص الأصلي.

النص الأصلي يظهر مرة واحدة في استجابة الإصدار.

## التوليد والتطبيع والتجزئة

التطبيع قبل البحث: قصّ الفراغات، تحويل لأحرف كبيرة، حذف المسافات والشرطات. المدخلات التالية تمثل نفس الكود:

```text
MR-8K2P-7X4M
MR8K2P7X4M
mr-8k2p-7x4m
```

الصيغة المعيارية بعد التطبيع تُجزَّأ بـ SHA-256 (Hex). القرار: الكود عالي العشوائية وليس كلمة مرور يختارها المستخدم، لذلك لا يُستخدم `PasswordHasher` لأنه يضيف ملحاً ويمنع البحث بالتجزئة.

مدة الصلاحية من الإعداد `ActivationCodes:ExpirationDays` (الافتراضي 30). إذا `ExpiresAt <= UtcNow` يُعامل الكود كمنتهٍ حتى لو بقي `Status = Active` في قاعدة البيانات. لا توجد وظيفة خلفية لتغيير الحالة الآن.

## الإلغاء وإعادة الإصدار

`POST /api/admin/activation-codes/{id}/revoke`

يُسمح إذا `Status == Active` و`UsedAt == null`. بعد الإلغاء:

- `ActivationCodeIssued → PaymentReceived`
- يمكن إصدار كود جديد أو التفعيل المباشر.

## الاسترداد (Redeem)

`POST /api/activation/redeem` يتطلب تسجيل الدخول. الـ Backend يأخذ `UserId` من الجلسة.

داخل معاملة مع `SELECT ... FOR UPDATE`:

1. تطبيع ثم Hash ثم البحث.
2. التحقق من Active / غير منتهٍ / نفس المستخدم / الطلب بحالة `ActivationCodeIssued` ومطابقة الدورة.
3. إنشاء Enrollment أو إعادة تفعيل الصف الحالي بسبب `UNIQUE(UserId, CourseId)`.
4. تعليم الكود `Used`.
5. `ActivationCodeIssued → Completed`.
6. حدث الطلب + تدقيق `CourseActivatedByCode`.

أي فشل: Rollback. الرسالة العامة:

```text
كود التفعيل غير صحيح أو غير متاح للاستخدام.
```

انتهاء الصلاحية يُكشف برسالة أوضح فقط بعد التحقق أن الكود يخص المستخدم الحالي.

Enrollment موجود و`Active` بتاريخ صالح: `409` دون استهلاك الكود.

`ActivatedBy` هو فاعل العملية: الطالب عند الاسترداد، والمشرف عند التفعيل المباشر.

## التفعيل المباشر

`POST /api/admin/purchase-requests/{id}/direct-activate`

فقط من `PaymentReceived`. لا يُنشأ ActivationCode. الانتقال: `PaymentReceived → Completed`. التدقيق: `CourseDirectlyActivated`.

## إعادة التفعيل

إذا وُجد Enrollment بحالة `Suspended` أو `Revoked` أو `Expired` (أو Active منتهٍ زمنياً) وطلب شراء جديد مدفوع: يُحدَّث نفس الصف إلى `Active` مع `StartedAt` و`ExpiresAt` جديدين وربط الطلب/الكود الحالي. لا صف جديد بسبب القيد الفريد.

سجل تاريخ إعادة الشراء المنفصل غير مطلوب في هذه المرحلة.

## تواريخ الوصول

- `Lifetime`: `ExpiresAt = null`
- `LimitedDuration`: `ExpiresAt = UtcNow + AccessDurationDays`

الساعة عبر `TimeProvider`.

## التزامن

قفل صف الكود/الطلب + قيد فريد على `(UserId, CourseId)` + قيد فريد جزئي لكود `Active` لكل طلب. استرداد متزامن لنفس الكود: نجاح واحد وفشل واحد وتسجيل واحد.

## Rate Limiting

`POST /api/activation/redeem`: 5 محاولات / دقيقة لكل مستخدم + IP. بعد التجاوز: `429`. لا يُسجَّل النص الأصلي ولا الـ Hash في السجلات.

## APIs

- `POST /api/admin/purchase-requests/{id}/issue-activation-code`
- `POST /api/admin/purchase-requests/{id}/direct-activate`
- `POST /api/admin/activation-codes/{id}/revoke`
- `GET /api/admin/activation-codes`
- `GET /api/admin/activation-codes/{id}`
- `POST /api/activation/redeem`
- `GET /api/enrollments`
