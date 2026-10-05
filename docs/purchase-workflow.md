# مسار طلب الاشتراك والدفع اليدوي

`PurchaseRequest` طلب بيع ومتابعة تحويل. **لا يمنح وصولاً إلى الدورة**.

`CourseEnrollment` هو ما يمنح الوصول، ويُنشأ لاحقاً في مرحلة التفعيل فقط.

## Price Snapshot

عند إنشاء الطلب يُنسخ `Course.PriceIQD` إلى `PurchaseRequest.AmountIQD` مرة واحدة. تغيير سعر الدورة لاحقاً لا يغيّر الطلب التاريخي.

## State Machine

المصدر الوحيد للانتقالات: `IPurchaseRequestStateMachine`.

المسموح في هذه المرحلة:

```text
Pending → Contacted → AwaitingPayment → PaymentReceived
Pending/Contacted/AwaitingPayment → Rejected
Pending/Contacted/AwaitingPayment → Cancelled
```

`PaymentReceived` لا يُرفض ولا يُلغى في v1 (المال مؤكَّد). التفعيل من هذه الحالة:

```text
PaymentReceived → ActivationCodeIssued → Completed
PaymentReceived → Completed
ActivationCodeIssued → PaymentReceived   (إلغاء كود غير مستخدم)
```

التفاصيل في `docs/activation-workflow.md`.

`Rejected` و`Cancelled` حالتان نهائيتان. إعادة الفتح لاحقاً تكون Workflow مستقل.

إذا وُجد طلب `Completed` لنفس المستخدم والدورة، لا يُنشأ طلب جديد تلقائياً حتى يتوفر منطق تجديد لاحق.

## Manual Payment

لا توجد بوابة دفع. الإدارة تتواصل، ترسل تعليمات التحويل من `SiteSettings` المسموح بها فقط (`PaymentMethods`, `TransferInstructions`, `SupportPhone`, `SupportWhatsApp`)، ثم تؤكد الاستلام يدوياً.

`PaymentMethod` على الطلب = قناة هذه الطلبية (مثل زين كاش).
`PaymentReference` = مرجع عملية العميل عند التأكيد، وليس رقم حساب الشركة.

عند `ConfirmPayment`:

- الحالة `PaymentReceived`
- `PaymentReceivedAt` و`ConfirmedBy`
- `PurchaseRequestEvent`
- `AdminAuditLog` بفعل `PurchasePaymentConfirmed`

ولا يُنشأ `CourseEnrollment` ولا `ActivationCode`.

## Timeline

كل تغيير حالة يضيف `PurchaseRequestEvent`. الإنشاء يسجّل `FromStatus = null` → `Pending`.

`AdminNotes` ملخص داخلي أحدث. التاريخ الكامل في الأحداث.

## Concurrency

- رقم الطلب: `MR-{YEAR}-{6}` عبر جدول `PurchaseRequestNumberCounters` مع `INSERT ... ON CONFLICT DO UPDATE RETURNING` داخل نفس معاملة الإنشاء.
- منع الطلب المفتوح المكرر: فحص تطبيقي + **Partial Unique Index** على `(UserId, CourseId)` للحالات المفتوحة.
- سباق طلبين متزامنين: أحدهما `201` والآخر `409`.

## Student vs Admin

الطالب ينشئ ويقرأ طلباته فقط. طلب مستخدم آخر يُرجع `404` حتى لا يُكشف وجوده.

إدارة الدفع: سياسة `ManagePayments` (`Admin` + `Support`). `ContentManager` لا يؤكد الدفع.
