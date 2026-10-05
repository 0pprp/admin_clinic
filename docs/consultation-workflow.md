# مسار الاستشارات

الحجز عام. لا يُشترط تسجيل الدخول. إن وُجد مستخدم مسجّل يُربط `UserId` اختيارياً.

## النموذج العام

`POST /api/consultations` (Rate limit: 3 / 15 دقيقة / IP)

الحقول: FullName، PhoneNumber، WhatsAppNumber، Email (اختياري وصالح إن وُجد)، ConsultationType، CompanyName، PreferredDate، PreferredTime، Topic، Message، PreferredCommunicationMethod.

Honeypot مخفي `Website`: إن مُلئ يُعاد نجاح شكلي دون حفظ.

رقم الطلب: `CONS-{year}-{n:D6}` عبر عدّاد `ConsultationRequestNumberCounters` (upsert آمن، ليس COUNT+1).

بعد الإرسال تعرض الواجهة: تم الاستلام + رقم الطلب + أن الفريق سيتواصل لتأكيد الموعد. لا يُقال إن الموعد مؤكد.

## آلة الحالات

المسار الطبيعي:

```text
New → Contacted → Scheduled → Completed
```

أيضاً:

```text
New / Contacted / Scheduled → Cancelled
New / Contacted → Rejected
```

أي انتقال غير ذلك: 409.

الجدولة تخزّن `ScheduledAt` كـ UTC مفسَّر من توقيت بغداد (`Asia/Baghdad` / `Arabic Standard Time`). لا يُحشر الموعد داخل `AdminNotes`.

## إدارة

سياسة `ManageConsultations` (Admin + Support):

```text
GET  /api/admin/consultations
GET  /api/admin/consultations/{id}
POST /api/admin/consultations/{id}/contacted
POST /api/admin/consultations/{id}/schedule
POST /api/admin/consultations/{id}/complete
POST /api/admin/consultations/{id}/cancel
POST /api/admin/consultations/{id}/reject
```
