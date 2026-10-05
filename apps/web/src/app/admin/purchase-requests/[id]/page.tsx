"use client";

import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { ConfirmDialog } from "@/components/admin/ConfirmDialog";
import { ForbiddenState } from "@/components/admin/ForbiddenState";
import { ErrorState, Field, LoadingState, PageHeader } from "@/components/admin/PageHeader";
import { StatusBadge } from "@/components/admin/StatusBadge";
import { useToast } from "@/components/admin/ToastProvider";
import { adminJson, errorMessage, isForbidden, isNotFound, isUnauthorized } from "@/lib/admin/http";
import { purchaseLabel, statusTone } from "@/lib/admin/labels";
import { inputClassName, primaryButtonClassName, secondaryButtonClassName } from "@/lib/admin/ui";
import type { AdminPurchaseDetail, DirectActivationResult, IssuedActivationCode } from "@/lib/admin/types";
import { formatBaghdadDateTime, formatIqd } from "@/lib/format";
import { purchaseTimelineLabel } from "@/lib/purchases";
import { safeInternalPath } from "@/lib/safe-path";

type ConfirmKind =
  | "contacted"
  | "awaiting"
  | "confirm-payment"
  | "issue-code"
  | "direct-activate"
  | "reject"
  | "cancel";

export default function PurchaseRequestDetailPage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const toast = useToast();
  const [detail, setDetail] = useState<AdminPurchaseDetail | null | undefined>(undefined);
  const [error, setError] = useState("");
  const [forbidden, setForbidden] = useState(false);
  const [note, setNote] = useState("");
  const [paymentMethod, setPaymentMethod] = useState("");
  const [paymentReference, setPaymentReference] = useState("");
  const [reason, setReason] = useState("");
  const [pending, setPending] = useState(false);
  const [confirm, setConfirm] = useState<ConfirmKind | null>(null);
  const [issued, setIssued] = useState<IssuedActivationCode | null>(null);
  const [activated, setActivated] = useState<DirectActivationResult | null>(null);

  async function load() {
    try {
      const data = await adminJson<AdminPurchaseDetail>(
        `/api/admin/purchase-requests/${params.id}`,
        "تعذر تحميل الطلب."
      );
      setDetail(data);
      setPaymentMethod(data.paymentMethod ?? "");
      setPaymentReference(data.paymentReference ?? "");
    } catch (caught) {
      if (isUnauthorized(caught)) {
        router.replace(`/login?from=${encodeURIComponent(safeInternalPath(`/admin/purchase-requests/${params.id}`))}`);
        return;
      }
      if (isForbidden(caught)) {
        setForbidden(true);
        return;
      }
      if (isNotFound(caught)) {
        setDetail(null);
        return;
      }
      setError(errorMessage(caught, "تعذر تحميل الطلب."));
    }
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [params.id]);

  async function run(kind: ConfirmKind) {
    if (!detail) {
      return;
    }

    setPending(true);
    setError("");
    try {
      if (kind === "contacted") {
        setDetail(
          await adminJson<AdminPurchaseDetail>(
            `/api/admin/purchase-requests/${detail.id}/contacted`,
            "تعذر تحديث الطلب.",
            { method: "POST", body: JSON.stringify({ note: note.trim() || null }) }
          )
        );
        toast.show("تم تسجيل التواصل.");
      } else if (kind === "awaiting") {
        setDetail(
          await adminJson<AdminPurchaseDetail>(
            `/api/admin/purchase-requests/${detail.id}/awaiting-payment`,
            "تعذر تحديث الطلب.",
            {
              method: "POST",
              body: JSON.stringify({
                paymentMethod: paymentMethod.trim() || null,
                paymentReference: paymentReference.trim() || null,
                adminNote: note.trim() || null
              })
            }
          )
        );
        toast.show("تم نقل الطلب إلى انتظار الدفع.");
      } else if (kind === "confirm-payment") {
        setDetail(
          await adminJson<AdminPurchaseDetail>(
            `/api/admin/purchase-requests/${detail.id}/confirm-payment`,
            "تعذر تأكيد الدفع.",
            {
              method: "POST",
              body: JSON.stringify({
                paymentMethod: paymentMethod.trim(),
                paymentReference: paymentReference.trim() || null,
                adminNotes: note.trim() || null
              })
            }
          )
        );
        toast.show("تم تأكيد استلام الدفع.");
      } else if (kind === "issue-code") {
        const result = await adminJson<IssuedActivationCode>(
          `/api/admin/purchase-requests/${detail.id}/issue-activation-code`,
          "تعذر إصدار كود التفعيل.",
          { method: "POST" }
        );
        setIssued(result);
        await load();
        toast.show("تم إصدار كود التفعيل. انسخه الآن لأنه لن يُعرض لاحقاً.");
      } else if (kind === "direct-activate") {
        const result = await adminJson<DirectActivationResult>(
          `/api/admin/purchase-requests/${detail.id}/direct-activate`,
          "تعذر تفعيل الدورة.",
          { method: "POST" }
        );
        setActivated(result);
        await load();
        toast.show("تم تفعيل الدورة مباشرة.");
      } else if (kind === "reject") {
        setDetail(
          await adminJson<AdminPurchaseDetail>(
            `/api/admin/purchase-requests/${detail.id}/reject`,
            "تعذر رفض الطلب.",
            { method: "POST", body: JSON.stringify({ reason: reason.trim() }) }
          )
        );
        toast.show("تم رفض الطلب.");
      } else if (kind === "cancel") {
        setDetail(
          await adminJson<AdminPurchaseDetail>(
            `/api/admin/purchase-requests/${detail.id}/cancel`,
            "تعذر إلغاء الطلب.",
            { method: "POST", body: JSON.stringify({ reason: reason.trim() }) }
          )
        );
        toast.show("تم إلغاء الطلب.");
      }
      setConfirm(null);
    } catch (caught) {
      setError(errorMessage(caught, "تعذر تنفيذ الإجراء."));
    } finally {
      setPending(false);
    }
  }

  if (forbidden) {
    return <ForbiddenState />;
  }

  if (detail === undefined) {
    return (
      <>
        <PageHeader title="تفاصيل الطلب" />
        <LoadingState />
      </>
    );
  }

  if (detail === null) {
    return (
      <>
        <PageHeader title="تفاصيل الطلب" />
        <p className="text-sm text-muted">الطلب غير موجود.</p>
      </>
    );
  }

  const confirmCopy: Record<ConfirmKind, { title: string; description: string; label: string; tone?: "danger" }> = {
    contacted: { title: "تأكيد التواصل؟", description: "سيتم نقل الطلب إلى حالة تم التواصل.", label: "تأكيد" },
    awaiting: { title: "نقل إلى انتظار الدفع؟", description: "سيظهر للطالب أنه بانتظار التحويل.", label: "تأكيد" },
    "confirm-payment": {
      title: "تأكيد استلام الدفع؟",
      description: "بعد التأكيد يمكن إصدار كود تفعيل أو تفعيل الدورة مباشرة.",
      label: "تأكيد استلام الدفع"
    },
    "issue-code": {
      title: "إصدار كود التفعيل؟",
      description: "سيظهر الكود مرة واحدة فقط في هذه الصفحة.",
      label: "إصدار الكود"
    },
    "direct-activate": {
      title: "تفعيل الدورة مباشرة؟",
      description: "سيتم تفعيل الدورة للطالب دون إصدار كود.",
      label: "تفعيل مباشر"
    },
    reject: { title: "رفض الطلب؟", description: "يحتاج هذا الإجراء إلى سبب.", label: "رفض", tone: "danger" },
    cancel: { title: "إلغاء الطلب؟", description: "يحتاج هذا الإجراء إلى سبب.", label: "إلغاء الطلب", tone: "danger" }
  };

  return (
    <>
      <PageHeader
        title={detail.requestNumber}
        description={detail.courseTitle}
        actions={
          <Link href="/admin/purchase-requests" className={secondaryButtonClassName}>
            العودة للقائمة
          </Link>
        }
      />
      {error ? <div className="mb-4"><ErrorState message={error} /></div> : null}
      {issued ? (
        <section className="mb-6 border border-accent bg-surface-warm px-5 py-5">
          <h2 className="text-lg font-semibold">كود التفعيل — اعرضه مرة واحدة</h2>
          <p className="mt-3 break-all font-medium text-lg">{issued.activationCode}</p>
          <p className="mt-2 text-sm text-muted">
            رقم الطلب {issued.requestNumber} · ينتهي {formatBaghdadDateTime(issued.expiresAt)}
          </p>
          <p className="mt-2 text-sm text-muted">لن يظهر هذا الكود مرة أخرى في القوائم. انسخه الآن.</p>
        </section>
      ) : null}
      {activated ? (
        <section className="clinic-card mb-6 px-5 py-5">
          <p className="font-medium">تم تفعيل الدورة: {activated.courseTitle}</p>
          <p className="mt-1 text-sm text-muted">رقم الطلب {activated.requestNumber}</p>
        </section>
      ) : null}
      <section className="clinic-card grid gap-4 px-5 py-6 text-sm sm:grid-cols-2">
        <Info label="الطالب" value={detail.fullName} />
        <Info label="البريد" value={detail.email} />
        <Info label="الهاتف" value={detail.phoneNumber} />
        <Info label="واتساب" value={detail.whatsAppNumber} />
        <Info label="المحافظة" value={detail.governorate} />
        <Info label="الدورة" value={detail.courseTitle} />
        <Info label="السعر" value={formatIqd(detail.amountIQD)} />
        <div>
          <p className="text-muted">الحالة</p>
          <div className="mt-1">
            <StatusBadge label={purchaseLabel(detail.status)} tone={statusTone(detail.status)} />
          </div>
        </div>
        <Info label="طريقة الدفع" value={detail.paymentMethod} />
        <Info label="مرجع الدفع" value={detail.paymentReference} />
        <Info label="تاريخ الطلب" value={formatBaghdadDateTime(detail.createdAt)} />
        <Info label="تاريخ التواصل" value={formatBaghdadDateTime(detail.contactedAt)} />
        <Info label="تاريخ استلام الدفع" value={formatBaghdadDateTime(detail.paymentReceivedAt)} />
        <Info label="أكّده" value={detail.confirmedBy} />
      </section>
      {detail.customerNotes ? (
        <section className="clinic-card mt-6 px-5 py-4 text-sm">
          <p className="text-muted">ملاحظات الطالب</p>
          <p className="mt-2 leading-7 whitespace-pre-wrap">{detail.customerNotes}</p>
        </section>
      ) : null}
      {detail.adminNotes ? (
        <section className="clinic-card mt-6 px-5 py-4 text-sm">
          <p className="text-muted">ملاحظات الإدارة</p>
          <p className="mt-2 leading-7 whitespace-pre-wrap">{detail.adminNotes}</p>
        </section>
      ) : null}
      <section className="mt-8 grid gap-4 sm:grid-cols-2">
        {detail.status === "Pending" || detail.status === "Contacted" || detail.status === "AwaitingPayment" ? (
          <Field label="ملاحظة إدارية">
            <textarea className={inputClassName} rows={3} value={note} onChange={(event) => setNote(event.target.value)} />
          </Field>
        ) : null}
        {detail.status === "Contacted" || detail.status === "AwaitingPayment" ? (
          <>
            <Field label="طريقة الدفع">
              <input className={inputClassName} value={paymentMethod} onChange={(event) => setPaymentMethod(event.target.value)} />
            </Field>
            <Field label="مرجع الدفع">
              <input className={inputClassName} value={paymentReference} onChange={(event) => setPaymentReference(event.target.value)} />
            </Field>
          </>
        ) : null}
        {detail.status === "Pending" || detail.status === "Contacted" || detail.status === "AwaitingPayment" ? (
          <Field label="سبب الرفض أو الإلغاء">
            <textarea className={inputClassName} rows={2} value={reason} onChange={(event) => setReason(event.target.value)} />
          </Field>
        ) : null}
      </section>
      <div className="mt-6 flex flex-wrap gap-2">
        {detail.status === "Pending" ? (
          <button type="button" className={primaryButtonClassName} onClick={() => setConfirm("contacted")}>
            تسجيل التواصل
          </button>
        ) : null}
        {detail.status === "Contacted" ? (
          <button type="button" className={primaryButtonClassName} onClick={() => setConfirm("awaiting")}>
            بانتظار الدفع
          </button>
        ) : null}
        {detail.status === "AwaitingPayment" ? (
          <button type="button" className={primaryButtonClassName} onClick={() => setConfirm("confirm-payment")}>
            تأكيد استلام الدفع
          </button>
        ) : null}
        {detail.status === "PaymentReceived" && detail.canActivate ? (
          <>
            <button type="button" className={primaryButtonClassName} onClick={() => setConfirm("issue-code")}>
              إصدار كود تفعيل
            </button>
            <button type="button" className={secondaryButtonClassName} onClick={() => setConfirm("direct-activate")}>
              تفعيل الدورة مباشرة
            </button>
          </>
        ) : null}
        {detail.status === "ActivationCodeIssued" ? (
          <p className="text-sm text-muted">تم إصدار كود التفعيل. يمكن إلغاء الكود من صفحة أكواد التفعيل إن لزم.</p>
        ) : null}
        {detail.status === "Pending" || detail.status === "Contacted" || detail.status === "AwaitingPayment" ? (
          <>
            <button type="button" className={secondaryButtonClassName} onClick={() => setConfirm("reject")}>
              رفض
            </button>
            <button type="button" className={secondaryButtonClassName} onClick={() => setConfirm("cancel")}>
              إلغاء
            </button>
          </>
        ) : null}
      </div>
      <section className="mt-10">
        <h2 className="text-lg font-semibold">التسلسل الزمني</h2>
        <ol className="mt-4 space-y-4 border-r border-border pr-4">
          {detail.timeline.map((event) => (
            <li key={event.id}>
              <p className="font-medium">{purchaseTimelineLabel(event.toStatus)}</p>
              {event.note ? <p className="text-sm leading-7 text-muted">{event.note}</p> : null}
              <p className="text-xs text-muted">{formatBaghdadDateTime(event.createdAt)}</p>
            </li>
          ))}
        </ol>
      </section>
      <ConfirmDialog
        open={confirm !== null}
        title={confirm ? confirmCopy[confirm].title : ""}
        description={confirm ? confirmCopy[confirm].description : ""}
        confirmLabel={confirm ? confirmCopy[confirm].label : "تأكيد"}
        tone={confirm ? confirmCopy[confirm].tone : "default"}
        pending={pending}
        onClose={() => !pending && setConfirm(null)}
        onConfirm={() => confirm && run(confirm)}
      />
    </>
  );
}

function Info({ label, value }: { label: string; value: string | null | undefined }) {
  return (
    <div>
      <p className="text-muted">{label}</p>
      <p className="mt-1 break-all">{value || "—"}</p>
    </div>
  );
}
