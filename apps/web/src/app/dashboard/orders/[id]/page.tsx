"use client";

import { useParams, useRouter, useSearchParams } from "next/navigation";
import { Suspense, useEffect, useState } from "react";
import { DashboardShell } from "@/components/dashboard/DashboardShell";
import { Badge, ButtonLink } from "@/components/ui/clinic";
import { apiFetch, parseJson } from "@/lib/api/client";
import { getCurrentUser } from "@/lib/auth/session";
import { formatBaghdadDateTime, formatIqd } from "@/lib/format";
import {
  purchaseStatusGuidance,
  purchaseStatusText,
  purchaseTimelineLabel,
  type PaymentInstructions,
  type StudentPurchaseDetail
} from "@/lib/purchases";

function purchaseTone(status: string): "orange" | "mint" | "sand" | "rose" | "navy" | "sky" {
  switch (status) {
    case "Completed":
      return "mint";
    case "ActivationCodeIssued":
      return "orange";
    case "AwaitingPayment":
    case "PaymentReceived":
      return "sky";
    case "Rejected":
    case "Cancelled":
      return "rose";
    case "Contacted":
      return "sand";
    default:
      return "navy";
  }
}

/** S06 · تفاصيل الطلب */
export default function DashboardOrderDetailPage() {
  return (
    <Suspense
      fallback={
        <DashboardShell title="تفاصيل الطلب">
          <p className="text-sm text-muted">جاري التحميل...</p>
        </DashboardShell>
      }
    >
      <OrderDetailContent />
    </Suspense>
  );
}

function OrderDetailContent() {
  const router = useRouter();
  const params = useParams<{ id: string }>();
  const searchParams = useSearchParams();
  const created = searchParams.get("created") === "1";
  const [detail, setDetail] = useState<StudentPurchaseDetail | null | undefined>(undefined);
  const [instructions, setInstructions] = useState<PaymentInstructions | null>(null);

  useEffect(() => {
    getCurrentUser().then(async (session) => {
      if (!session) {
        router.replace(`/login?from=/dashboard/orders/${params.id}`);
        return;
      }

      const response = await apiFetch(`/api/purchase-requests/${params.id}`);
      if (response.status === 404) {
        setDetail(null);
        return;
      }

      const body = await parseJson<StudentPurchaseDetail>(response, "تعذر تحميل الطلب.");
      setDetail(body);
      if (body.status === "AwaitingPayment") {
        const payment = await apiFetch("/api/purchase-requests/payment-instructions");
        if (payment.ok) {
          setInstructions(await payment.json());
        }
      }
    });
  }, [params.id, router]);

  if (detail === undefined) {
    return (
      <DashboardShell title="تفاصيل الطلب">
        <p className="text-sm text-muted">جاري التحميل...</p>
      </DashboardShell>
    );
  }

  if (detail === null) {
    return (
      <DashboardShell title="تفاصيل الطلب">
        <p className="text-sm text-muted">الطلب غير موجود.</p>
      </DashboardShell>
    );
  }

  const hasInstructions =
    instructions &&
    (instructions.paymentMethods || instructions.transferInstructions || instructions.supportPhone || instructions.supportWhatsApp);

  return (
    <DashboardShell
      title={created ? "تم استلام طلب الاشتراك بنجاح" : "تفاصيل الطلب"}
      description={created ? "سيتواصل معك فريقنا لتأكيد تفاصيل الدفع والتفعيل." : undefined}
    >
      <section className="clinic-card grid gap-5 px-5 py-6 text-sm sm:grid-cols-2 sm:px-6">
        <div>
          <p className="text-xs font-bold text-accent">رقم الطلب</p>
          <p className="mt-1 break-all font-semibold">{detail.requestNumber}</p>
        </div>
        <div>
          <p className="text-xs font-bold text-accent">الدورة</p>
          <p className="mt-1 font-semibold">{detail.courseTitle}</p>
        </div>
        <div>
          <p className="text-xs font-bold text-accent">السعر</p>
          <p className="mt-1 font-semibold">{formatIqd(detail.amountIQD)}</p>
        </div>
        <div>
          <p className="text-xs font-bold text-accent">الحالة</p>
          <div className="mt-1">
            <Badge tone={purchaseTone(detail.status)}>{purchaseStatusText(detail.status)}</Badge>
          </div>
        </div>
        <div className="sm:col-span-2">
          <p className="text-xs font-bold text-accent">تاريخ الطلب</p>
          <p className="mt-1">{formatBaghdadDateTime(detail.createdAt)}</p>
        </div>
      </section>

      <p className="mt-6 max-w-2xl text-sm leading-8 text-muted">{purchaseStatusGuidance(detail.status)}</p>

      {detail.status === "ActivationCodeIssued" ? (
        <ButtonLink href="/dashboard/activate" variant="accent" size="md" className="mt-6">
          تفعيل دورة
        </ButtonLink>
      ) : null}
      {detail.status === "Completed" && detail.hasActiveEnrollment ? (
        <ButtonLink href="/dashboard/courses" variant="accent" size="md" className="mt-6">
          الانتقال إلى كورساتي
        </ButtonLink>
      ) : null}

      {detail.status === "AwaitingPayment" ? (
        <section className="clinic-card mt-8 px-5 py-6 text-sm sm:px-6">
          <h2 className="text-lg font-extrabold tracking-tight">تعليمات التحويل</h2>
          {hasInstructions ? (
            <div className="mt-4 space-y-3 leading-8">
              {instructions?.paymentMethods ? <p>{instructions.paymentMethods}</p> : null}
              {instructions?.transferInstructions ? <p>{instructions.transferInstructions}</p> : null}
              {instructions?.supportPhone ? <p>الهاتف: {instructions.supportPhone}</p> : null}
              {instructions?.supportWhatsApp ? <p>واتساب: {instructions.supportWhatsApp}</p> : null}
            </div>
          ) : (
            <p className="mt-4 leading-8 text-muted">سيتواصل معك فريقنا لتزويدك بتفاصيل التحويل.</p>
          )}
        </section>
      ) : null}

      <section className="clinic-card mt-8 px-5 py-6 sm:px-6">
        <h2 className="text-lg font-extrabold tracking-tight">تتبع الطلب</h2>
        <ol className="mt-5 space-y-4 border-r-2 border-accent/30 pr-4">
          {detail.timeline.map((event, index) => (
            <li key={`${event.status}-${index}`} className="relative">
              <span className="absolute -right-[1.35rem] top-1.5 h-2.5 w-2.5 rounded-full bg-accent" aria-hidden="true" />
              <p className="font-semibold">{purchaseTimelineLabel(event.status)}</p>
              <p className="text-xs text-muted">{formatBaghdadDateTime(event.createdAt)}</p>
            </li>
          ))}
        </ol>
      </section>

      <div className="mt-10 flex flex-wrap gap-3">
        <ButtonLink href="/dashboard/orders" variant="soft" size="md">
          متابعة حالة الطلب
        </ButtonLink>
        <ButtonLink href="/courses" variant="outline" size="md">
          العودة إلى الدورات
        </ButtonLink>
      </div>
    </DashboardShell>
  );
}
