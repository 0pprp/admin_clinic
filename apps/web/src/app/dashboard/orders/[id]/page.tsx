"use client";

import Link from "next/link";
import { useParams, useRouter, useSearchParams } from "next/navigation";
import { Suspense, useEffect, useState } from "react";
import { DashboardShell } from "@/components/dashboard/DashboardShell";
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
    <DashboardShell title={created ? "تم استلام طلب الاشتراك بنجاح" : "تفاصيل الطلب"}>
      {created ? (
        <p className="mb-8 max-w-2xl text-sm leading-8 text-muted">
          سيتواصل معك فريقنا لتأكيد تفاصيل الدفع والتفعيل.
        </p>
      ) : null}
      <section className="grid gap-4 border border-border bg-surface px-5 py-6 text-sm sm:grid-cols-2">
        <div>
          <p className="text-muted">رقم الطلب</p>
          <p className="mt-1 break-all font-medium">{detail.requestNumber}</p>
        </div>
        <div>
          <p className="text-muted">الدورة</p>
          <p className="mt-1 font-medium">{detail.courseTitle}</p>
        </div>
        <div>
          <p className="text-muted">السعر</p>
          <p className="mt-1 font-medium">{formatIqd(detail.amountIQD)}</p>
        </div>
        <div>
          <p className="text-muted">الحالة</p>
          <p className="mt-1">{purchaseStatusText(detail.status)}</p>
        </div>
        <div>
          <p className="text-muted">تاريخ الطلب</p>
          <p className="mt-1">{formatBaghdadDateTime(detail.createdAt)}</p>
        </div>
      </section>
      <p className="mt-6 max-w-2xl text-sm leading-8 text-muted">{purchaseStatusGuidance(detail.status)}</p>
      {detail.status === "ActivationCodeIssued" ? (
        <Link href="/dashboard/activate" className="mt-6 inline-flex border border-accent px-4 py-2 text-sm text-accent">
          تفعيل دورة
        </Link>
      ) : null}
      {detail.status === "Completed" && detail.hasActiveEnrollment ? (
        <Link href="/dashboard/courses" className="mt-6 inline-flex border border-accent px-4 py-2 text-sm text-accent">
          الانتقال إلى دوراتي
        </Link>
      ) : null}
      {detail.status === "AwaitingPayment" ? (
        <section className="mt-8 border border-border px-5 py-6 text-sm">
          <h2 className="text-lg font-semibold">تعليمات التحويل</h2>
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
      <section className="mt-10">
        <h2 className="text-lg font-semibold">تتبع الطلب</h2>
        <ol className="mt-5 space-y-4 border-r border-border pr-4">
          {detail.timeline.map((event, index) => (
            <li key={`${event.status}-${index}`}>
              <p className="font-medium">{purchaseTimelineLabel(event.status)}</p>
              <p className="text-xs text-muted">{formatBaghdadDateTime(event.createdAt)}</p>
            </li>
          ))}
        </ol>
      </section>
      <div className="mt-10 flex flex-wrap gap-4 text-sm">
        <Link href="/dashboard/orders" className="border border-border px-4 py-2">
          متابعة حالة الطلب
        </Link>
        <Link href="/courses" className="border border-accent px-4 py-2 text-accent">
          العودة إلى الدورات
        </Link>
      </div>
    </DashboardShell>
  );
}
