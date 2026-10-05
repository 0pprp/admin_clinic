# لوحة الإدارة

لوحة يومية لفريق محمد رؤوف. الحماية الحقيقية في سياسات الـBackend؛ الواجهة تخفي الروابط للتجربة فقط.

## الأدوار

| الدور | الاستخدام |
| --- | --- |
| Admin | كل سياسات الإدارة بما فيها المستخدمون وسجل العمليات وإعدادات الدفع |
| ContentManager | الدورات والمحتوى العام (مقالات، FAQ، خبرات، شهادات، إحصائيات، إعدادات المحتوى) |
| Support | الطلبات، التفعيل، الطلاب، الاستشارات، رسائل التواصل |
| Student | لا يصل لأي مورد `/api/admin/*` (403) |

## السياسات

| السياسة | الأدوار |
| --- | --- |
| AccessAdminPanel | Admin, Support, ContentManager |
| ManageCourses | Admin, ContentManager |
| ManageContent | Admin, ContentManager |
| ManageStudents | Admin, Support |
| ManagePayments | Admin, Support |
| ManageActivations | Admin, Support |
| ManageConsultations | Admin, Support |
| ManageUsers | Admin |
| ViewAuditLogs | Admin |
| ManageBusinessSettings | Admin فقط (تعليمات الدفع والتحويل) |

Frontend يحسب القائمة من الأدوار. Backend يبقى مصدر الصلاحية.

## المسارات

```text
/admin
/admin/purchase-requests
/admin/purchase-requests/[id]
/admin/students
/admin/students/[id]
/admin/courses
/admin/courses/[id]
/admin/enrollments
/admin/activation-codes
/admin/consultations
/admin/consultations/[id]
/admin/articles
/admin/articles/[id]
/admin/faq
/admin/contact-messages
/admin/contact-messages/[id]
/admin/users
/admin/audit-logs
/admin/settings
```

كلها Authenticated + Authorized + `noindex` + RTL.

طالب بلا أي سياسة إدارية يُحوَّل في الواجهة إلى `/dashboard`، وأي طلب API يبقى 403.

## إدارة الطلاب

- تعليق دورة: `CourseEnrollment.Status = Suspended` للدورة المحددة فقط.
- استعادة دورة: من Suspended إلى Active. إذا انتهى `ExpiresAt` تُرفض الاستعادة بـ 409 ويُطلب شراء جديد. لا يُمدَّد التاريخ تلقائياً حتى لا يُمنح وصول بلا تجديد.
- سحب دورة: `Status = Revoked`. لا تُحذف LessonProgress أو طلبات الشراء أو أكواد التفعيل.
- تعليق الحساب: `AccountStatus = Suspended` + إلغاء كل Refresh Tokens + تحديث SecurityStamp. الاشتراكات لا تُغيَّر.
- استعادة الحساب: `AccountStatus = Active` دون إنشاء جلسة تلقائياً.

## حماية آخر مدير

ممنوع:

- تعليق الحساب الذاتي.
- تعليق آخر Admin نشط.
- إزالة دور Admin عن آخر Admin نشط.

إذا وُجد مدير ثانٍ نشط، يمكن تعديل الأدوار أو تعليق غير الأخير حسب السياسة.

## إدارة المحتوى

- المقالات نص عادي / textarea. لا HTML غير مطهّر ولا `dangerouslySetInnerHTML` لمحتوى المقال.
- FAQ / Expertise / Testimonials / الإحصائيات عبر `/api/admin/*` وسياسة ManageContent.
- الإعدادات العامة (العلامة، الهواتف العامة، التذييل، ConsultationInfo): ManageContent.
- تعليمات الدفع: ManageBusinessSettings (Admin). لا تُخزَّن أسرار أو مفاتيح خاصة في `SiteSetting`.
- القائمة العامة للمفاتيح مسموحة فقط (allowlist). لا يقبل Client مفتاحاً اعتباطياً.
- الكاش العام لإعدادات الموقع يبقى حتى 60 ثانية؛ هذا مقبول في v1.

## سجل العمليات

يُسجَّل على الأقل عند:

- تأكيد الدفع
- إصدار / إلغاء كود التفعيل والتفعيل المباشر
- تعليق / استعادة / سحب دورة
- تعليق / استعادة حساب
- تغيير الأدوار
- نشر / أرشفة دورة أو مقال
- تحديث إعدادات الموقع
- جدولة / إكمال الاستشارة

لا تُسجَّل أسرار (كلمات مرور، توكنات، أكواد التفعيل النصية). واجهة التفاصيل تحجب Metadata إذا تسربت كلمات حساسة بالخطأ.
