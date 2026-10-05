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
import {
  communicationMethodLabel,
  consultationStatusLabel,
  consultationTypeLabel,
  labelOrRaw,
  statusTone
} from "@/lib/admin/labels";
import { inputClassName, primaryButtonClassName, secondaryButtonClassName } from "@/lib/admin/ui";
import type { AdminConsultationDetail } from "@/lib/admin/types";
import { formatBaghdadDateTime, formatDate } from "@/lib/format";
import { safeInternalPath } from "@/lib/safe-path";

type ActionKind = "contacted" | "schedule" | "complete" | "cancel" | "reject";

export default function ConsultationDetailPage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const toast = useToast();
  const [detail, setDetail] = useState<AdminConsultationDetail | null | undefined>(undefined);
  const [error, setError] = useState("");
  const [forbidden, setForbidden] = useState(false);
  const [pending, setPending] = useState(false);
  const [notes, setNotes] = useState("");
  const [scheduledDate, setScheduledDate] = useState("");
  const [scheduledTime, setScheduledTime] = useState("");
  const [confirm, setConfirm] = useState<ActionKind | null>(null);

  async function load() {
    try {
      const data = await adminJson<AdminConsultationDetail>(
        `/api/admin/consultations/${params.id}`,
        "تعذر تحميل الاستشارة."
      );
      setDetail(data);
      setNotes(data.adminNotes ?? "");
    } catch (caught) {
      if (isUnauthorized(caught)) {
        router.replace(`/login?from=${encodeURIComponent(safeInternalPath(`/admin/consultations/${params.id}`))}`);
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
      setError(errorMessage(caught, "تعذر تحميل الاستشارة."));
    }
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [params.id]);

  async function run() {
    if (!confirm || !detail) {
      return;
    }

    setPending(true);
    setError("");
    try {
      if (confirm === "schedule") {
        if (!scheduledDate || !scheduledTime) {
          setError("تاريخ الموعد ووقته مطلوبان.");
          setPending(false);
          return;
        }

        setDetail(
          await adminJson<AdminConsultationDetail>(
            `/api/admin/consultations/${detail.id}/schedule`,
            "تعذر جدولة الاستشارة.",
            {
              method: "POST",
              body: JSON.stringify({
                scheduledDate,
                scheduledTime: scheduledTime.length === 5 ? `${scheduledTime}:00` : scheduledTime,
                adminNotes: notes.trim() || null
              })
            }
          )
        );
        toast.show("تم جدولة الاستشارة.");
      } else {
        setDetail(
          await adminJson<AdminConsultationDetail>(
            `/api/admin/consultations/${detail.id}/${confirm}`,
            "تعذر تحديث الاستشارة.",
            { method: "POST", body: JSON.stringify({ adminNotes: notes.trim() || null }) }
          )
        );
        toast.show("تم تحديث الاستشارة.");
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
        <PageHeader title="تفاصيل الاستشارة" />
        <LoadingState />
      </>
    );
  }

  if (detail === null) {
    return (
      <>
        <PageHeader title="تفاصيل الاستشارة" />
        <p className="text-sm text-muted">الاستشارة غير موجودة.</p>
      </>
    );
  }

  return (
    <>
      <PageHeader
        title={detail.requestNumber}
        description={detail.topic}
        actions={
          <Link href="/admin/consultations" className={secondaryButtonClassName}>
            العودة للقائمة
          </Link>
        }
      />
      {error ? <div className="mb-4"><ErrorState message={error} /></div> : null}
      <section className="grid gap-4 border border-border bg-surface px-5 py-6 text-sm sm:grid-cols-2">
        <Info label="الاسم" value={detail.fullName} />
        <Info label="الهاتف" value={detail.phoneNumber} />
        <Info label="واتساب" value={detail.whatsAppNumber} />
        <Info label="البريد" value={detail.email} />
        <Info label="الشركة" value={detail.companyName} />
        <Info label="النوع" value={labelOrRaw(consultationTypeLabel, detail.consultationType)} />
        <Info label="طريقة التواصل" value={labelOrRaw(communicationMethodLabel, detail.preferredCommunicationMethod)} />
        <Info label="التاريخ المفضل" value={formatDate(detail.preferredDate)} />
        <Info label="الوقت المفضل" value={formatTimeOnly(detail.preferredTime)} />
        <Info label="الموعد المجدول" value={formatBaghdadDateTime(detail.scheduledAt)} />
        <div>
          <p className="text-muted">الحالة</p>
          <div className="mt-1">
            <StatusBadge label={labelOrRaw(consultationStatusLabel, detail.status)} tone={statusTone(detail.status)} />
          </div>
        </div>
      </section>
      <section className="mt-6 border border-border px-5 py-4 text-sm">
        <p className="text-muted">الرسالة</p>
        <p className="mt-2 leading-7 whitespace-pre-wrap">{detail.message}</p>
      </section>
      <div className="mt-6 grid gap-4 sm:grid-cols-2">
        <Field label="ملاحظات الإدارة">
          <textarea className={inputClassName} rows={4} value={notes} onChange={(event) => setNotes(event.target.value)} />
        </Field>
        {detail.status === "Contacted" ? (
          <>
            <Field label="تاريخ الموعد">
              <input className={inputClassName} type="date" value={scheduledDate} onChange={(event) => setScheduledDate(event.target.value)} />
            </Field>
            <Field label="وقت الموعد">
              <input className={inputClassName} type="time" value={scheduledTime} onChange={(event) => setScheduledTime(event.target.value)} />
            </Field>
          </>
        ) : null}
      </div>
      <div className="mt-6 flex flex-wrap gap-2">
        {detail.status === "New" ? (
          <button type="button" className={primaryButtonClassName} onClick={() => setConfirm("contacted")}>
            تسجيل التواصل
          </button>
        ) : null}
        {detail.status === "Contacted" ? (
          <button type="button" className={primaryButtonClassName} onClick={() => setConfirm("schedule")}>
            جدولة الموعد
          </button>
        ) : null}
        {detail.status === "Scheduled" ? (
          <button type="button" className={primaryButtonClassName} onClick={() => setConfirm("complete")}>
            إكمال
          </button>
        ) : null}
        {detail.status === "New" || detail.status === "Contacted" || detail.status === "Scheduled" ? (
          <button type="button" className={secondaryButtonClassName} onClick={() => setConfirm("cancel")}>
            إلغاء
          </button>
        ) : null}
        {detail.status === "New" || detail.status === "Contacted" ? (
          <button type="button" className={secondaryButtonClassName} onClick={() => setConfirm("reject")}>
            رفض
          </button>
        ) : null}
      </div>
      <ConfirmDialog
        open={confirm !== null}
        title={
          confirm === "schedule"
            ? "تأكيد جدولة الموعد؟"
            : confirm === "contacted"
              ? "تأكيد التواصل؟"
              : confirm === "complete"
                ? "إكمال الاستشارة؟"
                : confirm === "cancel"
                  ? "إلغاء الاستشارة؟"
                  : "رفض الاستشارة؟"
        }
        description={
          confirm === "schedule"
            ? "سيتم حفظ التاريخ والوقت المحددين في حقلي الموعد، وليس داخل الملاحظات."
            : undefined
        }
        confirmLabel="تأكيد"
        tone={confirm === "reject" || confirm === "cancel" ? "danger" : "default"}
        pending={pending}
        onClose={() => !pending && setConfirm(null)}
        onConfirm={run}
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

function formatTimeOnly(value: string | null): string | null {
  if (!value) {
    return null;
  }

  return value.slice(0, 5);
}
