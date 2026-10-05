export const purchaseStatusLabel: Record<string, string> = {
  Pending: "جديد",
  Contacted: "تم التواصل",
  AwaitingPayment: "بانتظار الدفع",
  PaymentReceived: "تم استلام الدفع",
  ActivationCodeIssued: "تم إصدار كود التفعيل",
  Completed: "مكتمل",
  Rejected: "مرفوض",
  Cancelled: "ملغي"
};

export function purchaseStatusText(status: string): string {
  return purchaseStatusLabel[status] ?? status;
}

export function purchaseTimelineLabel(status: string): string {
  switch (status) {
    case "Pending":
      return "تم إنشاء الطلب";
    case "Contacted":
      return "تم التواصل";
    case "AwaitingPayment":
      return "بانتظار التحويل";
    case "PaymentReceived":
      return "تم تأكيد الدفع";
    case "ActivationCodeIssued":
      return "تم إصدار كود التفعيل";
    case "Completed":
      return "اكتمل الطلب";
    case "Rejected":
      return "رُفض الطلب";
    case "Cancelled":
      return "أُلغي الطلب";
    default:
      return purchaseStatusText(status);
  }
}

export function purchaseStatusGuidance(status: string): string {
  switch (status) {
    case "Pending":
      return "تم استلام طلبك، وسيتواصل معك الفريق قريباً.";
    case "Contacted":
      return "تم التواصل معك، وسيتم تزويدك بتفاصيل الدفع.";
    case "AwaitingPayment":
      return "بانتظار التحويل حسب تعليمات الدفع المعروضة أدناه.";
    case "PaymentReceived":
      return "تم تأكيد الدفع، وسيتم تزويدك بكود التفعيل أو تفعيل الدورة مباشرة.";
    case "ActivationCodeIssued":
      return "تم إصدار كود التفعيل. أدخل الكود في صفحة تفعيل دورة.";
    case "Completed":
      return "تم تفعيل الدورة بنجاح.";
    case "Rejected":
      return "تم رفض هذا الطلب. يمكنك التواصل مع الفريق إن احتجت توضيحاً.";
    case "Cancelled":
      return "أُلغي هذا الطلب.";
    default:
      return "يمكنك متابعة حالة الطلب من هذه الصفحة.";
  }
}

export type StudentPurchaseSummary = {
  id: string;
  requestNumber: string;
  courseId: string;
  courseTitle: string;
  courseSlug: string;
  amountIQD: number;
  status: string;
  createdAt: string;
};

export type StudentPurchaseEvent = {
  status: string;
  createdAt: string;
};

export type StudentPurchaseDetail = StudentPurchaseSummary & {
  contactedAt: string | null;
  paymentReceivedAt: string | null;
  customerNotes: string | null;
  hasActiveEnrollment: boolean;
  timeline: StudentPurchaseEvent[];
};

export type PurchaseCreated = {
  id: string;
  requestNumber: string;
  courseId: string;
  courseTitle: string;
  amountIQD: number;
  status: string;
  createdAt: string;
};

export type PaymentInstructions = {
  paymentMethods: string | null;
  transferInstructions: string | null;
  supportPhone: string | null;
  supportWhatsApp: string | null;
};
