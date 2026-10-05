import { purchaseStatusText } from "@/lib/purchases";

export function labelOrRaw(map: Record<string, string>, value: string): string {
  return map[value] ?? value;
}

export const accountStatusLabel: Record<string, string> = {
  Active: "نشط",
  Suspended: "موقوف",
  Blocked: "محظور"
};

export const roleLabel: Record<string, string> = {
  Admin: "مدير",
  Support: "دعم",
  ContentManager: "مدير محتوى",
  Student: "طالب"
};

export const courseStatusLabel: Record<string, string> = {
  Draft: "مسودة",
  Published: "منشور",
  Archived: "مؤرشف"
};

export const lessonStatusLabel: Record<string, string> = {
  Draft: "مسودة",
  Published: "منشور",
  Archived: "مؤرشف"
};

export const enrollmentStatusLabel: Record<string, string> = {
  Active: "نشط",
  Suspended: "موقوف",
  Revoked: "ملغى",
  Expired: "منتهٍ"
};

export const activationStatusLabel: Record<string, string> = {
  Active: "نشط",
  Used: "مستخدم",
  Expired: "منتهٍ",
  Revoked: "ملغى"
};

export const consultationStatusLabel: Record<string, string> = {
  New: "جديد",
  Contacted: "تم التواصل",
  Scheduled: "مجدول",
  Completed: "مكتمل",
  Cancelled: "ملغي",
  Rejected: "مرفوض"
};

export const consultationTypeLabel: Record<string, string> = {
  Business: "أعمال",
  Marketing: "تسويق",
  Management: "إدارة",
  Personal: "شخصي",
  Other: "أخرى"
};

export const communicationMethodLabel: Record<string, string> = {
  Phone: "هاتف",
  WhatsApp: "واتساب",
  Email: "بريد"
};

export const contactStatusLabel: Record<string, string> = {
  New: "جديد",
  Read: "مقروء",
  Replied: "تم الرد",
  Archived: "مؤرشف"
};

export const contentStatusLabel: Record<string, string> = {
  Draft: "مسودة",
  Published: "منشور",
  Archived: "مؤرشف"
};

export const videoProviderLabel: Record<string, string> = {
  None: "بدون",
  LocalPlaceholder: "محلي تجريبي",
  CloudflareStream: "Cloudflare Stream",
  CloudflareR2: "Cloudflare R2",
  BunnyStream: "Bunny Stream",
  Vimeo: "Vimeo",
  SelfHostedHls: "رفع على سيرفر العيادة (جودة تلقائية)"
};

export const videoProcessingLabel: Record<string, string> = {
  None: "لا يوجد فيديو",
  Queued: "بانتظار المعالجة",
  Processing: "جاري التحويل",
  Ready: "جاهز",
  Failed: "فشل"
};

export const activationMethodLabel: Record<string, string> = {
  ActivationCode: "كود تفعيل",
  Direct: "تفعيل مباشر",
  Manual: "يدوي"
};

export const featuredLabel = (value: boolean): string => (value ? "مميزة" : "عادية");

export const boolActiveLabel = (value: boolean): string => (value ? "نشط" : "غير نشط");

export function purchaseLabel(status: string): string {
  return purchaseStatusText(status);
}

export function courseLevelLabel(level: string): string {
  switch (level) {
    case "Beginner":
      return "مبتدئ";
    case "Intermediate":
      return "متوسط";
    case "Advanced":
      return "متقدم";
    default:
      return level;
  }
}

export function accessTypeLabel(accessType: string): string {
  return accessType === "LimitedDuration" ? "مدة محدودة" : "مدى الحياة";
}

export type BadgeTone = "neutral" | "warning" | "success" | "danger";

export function statusTone(status: string): BadgeTone {
  switch (status) {
    case "Published":
    case "Active":
    case "Completed":
    case "PaymentReceived":
    case "Used":
    case "Replied":
    case "Scheduled":
      return "success";
    case "Pending":
    case "New":
    case "Draft":
    case "AwaitingPayment":
    case "Contacted":
    case "Read":
    case "Queued":
    case "Processing":
      return "warning";
    case "Ready":
      return "success";
    case "Failed":
      return "danger";
    case "Rejected":
    case "Cancelled":
    case "Revoked":
    case "Suspended":
    case "Blocked":
    case "Expired":
    case "Archived":
      return "danger";
    default:
      return "neutral";
  }
}
