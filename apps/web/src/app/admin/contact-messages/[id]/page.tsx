"use client";

import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { ConfirmDialog } from "@/components/admin/ConfirmDialog";
import { ForbiddenState } from "@/components/admin/ForbiddenState";
import { ErrorState, LoadingState, PageHeader } from "@/components/admin/PageHeader";
import { StatusBadge } from "@/components/admin/StatusBadge";
import { useToast } from "@/components/admin/ToastProvider";
import { adminJson, errorMessage, isForbidden, isNotFound, isUnauthorized } from "@/lib/admin/http";
import { contactStatusLabel, labelOrRaw, statusTone } from "@/lib/admin/labels";
import { primaryButtonClassName, secondaryButtonClassName } from "@/lib/admin/ui";
import type { AdminContactDetail } from "@/lib/admin/types";
import { formatBaghdadDateTime } from "@/lib/format";
import { safeInternalPath } from "@/lib/safe-path";

export default function ContactMessageDetailPage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const toast = useToast();
  const [detail, setDetail] = useState<AdminContactDetail | null | undefined>(undefined);
  const [error, setError] = useState("");
  const [forbidden, setForbidden] = useState(false);
  const [pending, setPending] = useState(false);
  const [confirm, setConfirm] = useState<"mark-read" | "mark-replied" | "archive" | null>(null);

  async function load() {
    try {
      setDetail(
        await adminJson<AdminContactDetail>(`/api/admin/contact-messages/${params.id}`, "تعذر تحميل الرسالة.")
      );
    } catch (caught) {
      if (isUnauthorized(caught)) {
        router.replace(`/login?from=${encodeURIComponent(safeInternalPath(`/admin/contact-messages/${params.id}`))}`);
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
      setError(errorMessage(caught, "تعذر تحميل الرسالة."));
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
      setDetail(
        await adminJson<AdminContactDetail>(
          `/api/admin/contact-messages/${detail.id}/${confirm}`,
          "تعذر تحديث الرسالة.",
          { method: "POST" }
        )
      );
      toast.show("تم تحديث الرسالة.");
      setConfirm(null);
    } catch (caught) {
      setError(errorMessage(caught, "تعذر تحديث الرسالة."));
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
        <PageHeader title="رسالة تواصل" />
        <LoadingState />
      </>
    );
  }

  if (detail === null) {
    return (
      <>
        <PageHeader title="رسالة تواصل" />
        <p className="text-sm text-muted">الرسالة غير موجودة.</p>
      </>
    );
  }

  return (
    <>
      <PageHeader
        title={detail.subject}
        actions={
          <Link href="/admin/contact-messages" className={secondaryButtonClassName}>
            العودة للقائمة
          </Link>
        }
      />
      {error ? <div className="mb-4"><ErrorState message={error} /></div> : null}
      <section className="clinic-card grid gap-4 px-5 py-6 text-sm sm:grid-cols-2">
        <Info label="الاسم" value={detail.name} />
        <Info label="البريد" value={detail.email} />
        <Info label="الهاتف" value={detail.phone} />
        <div>
          <p className="text-muted">الحالة</p>
          <div className="mt-1">
            <StatusBadge label={labelOrRaw(contactStatusLabel, detail.status)} tone={statusTone(detail.status)} />
          </div>
        </div>
        <Info label="التاريخ" value={formatBaghdadDateTime(detail.createdAt)} />
      </section>
      <section className="clinic-card mt-6 px-5 py-4 text-sm">
        <p className="text-muted">الرسالة</p>
        <p className="mt-2 leading-7 whitespace-pre-wrap">{detail.message}</p>
      </section>
      <div className="mt-6 flex flex-wrap gap-2">
        {detail.status === "New" ? (
          <button type="button" className={primaryButtonClassName} onClick={() => setConfirm("mark-read")}>
            تعليم كمقروء
          </button>
        ) : null}
        {detail.status !== "Replied" && detail.status !== "Archived" ? (
          <button type="button" className={secondaryButtonClassName} onClick={() => setConfirm("mark-replied")}>
            تعليم كتم الرد
          </button>
        ) : null}
        {detail.status !== "Archived" ? (
          <button type="button" className={secondaryButtonClassName} onClick={() => setConfirm("archive")}>
            أرشفة
          </button>
        ) : null}
      </div>
      <ConfirmDialog
        open={confirm !== null}
        title={
          confirm === "mark-read" ? "تعليم الرسالة كمقروءة؟" : confirm === "mark-replied" ? "تعليم الرسالة كتم الرد؟" : "أرشفة الرسالة؟"
        }
        confirmLabel="تأكيد"
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
