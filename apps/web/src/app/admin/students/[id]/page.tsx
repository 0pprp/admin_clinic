"use client";

import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { ConfirmDialog } from "@/components/admin/ConfirmDialog";
import { ForbiddenState } from "@/components/admin/ForbiddenState";
import { AdminTable, ErrorState, LoadingState, PageHeader } from "@/components/admin/PageHeader";
import { StatusBadge } from "@/components/admin/StatusBadge";
import { useToast } from "@/components/admin/ToastProvider";
import { adminVoid, adminJson, errorMessage, isForbidden, isNotFound, isUnauthorized } from "@/lib/admin/http";
import {
  accountStatusLabel,
  enrollmentStatusLabel,
  labelOrRaw,
  purchaseLabel,
  roleLabel,
  statusTone
} from "@/lib/admin/labels";
import { dangerButtonClassName, secondaryButtonClassName } from "@/lib/admin/ui";
import type { AdminStudentDetail } from "@/lib/admin/types";
import { formatBaghdadDateTime, formatIqd } from "@/lib/format";
import { safeInternalPath } from "@/lib/safe-path";

type ConfirmAction =
  | { kind: "suspend-account" }
  | { kind: "restore-account" }
  | { kind: "suspend-course"; courseId: string; title: string }
  | { kind: "restore-course"; courseId: string; title: string }
  | { kind: "revoke-course"; courseId: string; title: string };

export default function StudentDetailPage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const toast = useToast();
  const [detail, setDetail] = useState<AdminStudentDetail | null | undefined>(undefined);
  const [error, setError] = useState("");
  const [forbidden, setForbidden] = useState(false);
  const [pending, setPending] = useState(false);
  const [confirm, setConfirm] = useState<ConfirmAction | null>(null);

  async function load() {
    try {
      setDetail(await adminJson<AdminStudentDetail>(`/api/admin/students/${params.id}`, "تعذر تحميل ملف الطالب."));
    } catch (caught) {
      if (isUnauthorized(caught)) {
        router.replace(`/login?from=${encodeURIComponent(safeInternalPath(`/admin/students/${params.id}`))}`);
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
      setError(errorMessage(caught, "تعذر تحميل ملف الطالب."));
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
      const userId = detail.id;
      if (confirm.kind === "suspend-account") {
        await adminVoid(`/api/admin/students/${userId}/suspend-account`, "تعذر إيقاف الحساب.", { method: "POST" });
        toast.show("تم إيقاف الحساب.");
      } else if (confirm.kind === "restore-account") {
        await adminVoid(`/api/admin/students/${userId}/restore-account`, "تعذر استعادة الحساب.", { method: "POST" });
        toast.show("تم استعادة الحساب.");
      } else if (confirm.kind === "suspend-course") {
        await adminVoid(
          `/api/admin/students/${userId}/courses/${confirm.courseId}/suspend`,
          "تعذر إيقاف الاشتراك.",
          { method: "POST" }
        );
        toast.show("تم إيقاف الاشتراك.");
      } else if (confirm.kind === "restore-course") {
        await adminVoid(
          `/api/admin/students/${userId}/courses/${confirm.courseId}/restore`,
          "تعذر استعادة الاشتراك.",
          { method: "POST" }
        );
        toast.show("تم استعادة الاشتراك.");
      } else {
        await adminVoid(
          `/api/admin/students/${userId}/courses/${confirm.courseId}/revoke`,
          "تعذر إلغاء الاشتراك.",
          { method: "POST" }
        );
        toast.show("تم إلغاء الاشتراك.");
      }
      setConfirm(null);
      await load();
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
        <PageHeader title="ملف الطالب" />
        <LoadingState />
      </>
    );
  }

  if (detail === null) {
    return (
      <>
        <PageHeader title="ملف الطالب" />
        <p className="text-sm text-muted">الطالب غير موجود.</p>
      </>
    );
  }

  const confirmTitle =
    confirm?.kind === "suspend-account"
      ? "إيقاف الحساب؟"
      : confirm?.kind === "restore-account"
        ? "استعادة الحساب؟"
        : confirm?.kind === "suspend-course"
          ? `إيقاف اشتراك «${confirm.title}»؟`
          : confirm?.kind === "restore-course"
            ? `استعادة اشتراك «${confirm.title}»؟`
            : confirm
              ? `إلغاء اشتراك «${confirm.title}»؟`
              : "";

  return (
    <>
      <PageHeader
        title={detail.fullName}
        description={detail.email}
        actions={
          <Link href="/admin/students" className={secondaryButtonClassName}>
            العودة للقائمة
          </Link>
        }
      />
      {error ? <div className="mb-4"><ErrorState message={error} /></div> : null}
      <section className="clinic-card grid gap-4 px-5 py-6 text-sm sm:grid-cols-2">
        <Info label="الهاتف" value={detail.phoneNumber} />
        <Info label="واتساب" value={detail.whatsAppNumber} />
        <Info label="المحافظة" value={detail.governorate} />
        <div>
          <p className="text-muted">حالة الحساب</p>
          <div className="mt-1">
            <StatusBadge
              label={labelOrRaw(accountStatusLabel, detail.accountStatus)}
              tone={statusTone(detail.accountStatus)}
            />
          </div>
        </div>
        <Info label="الأدوار" value={detail.roles.map((role) => labelOrRaw(roleLabel, role)).join("، ")} />
        <Info label="تاريخ الإنشاء" value={formatBaghdadDateTime(detail.createdAt)} />
        <Info label="آخر دخول" value={formatBaghdadDateTime(detail.lastLoginAt)} />
      </section>
      <div className="mt-4 flex flex-wrap gap-2">
        {detail.accountStatus === "Active" ? (
          <button type="button" className={dangerButtonClassName} onClick={() => setConfirm({ kind: "suspend-account" })}>
            إيقاف الحساب
          </button>
        ) : (
          <button type="button" className={secondaryButtonClassName} onClick={() => setConfirm({ kind: "restore-account" })}>
            استعادة الحساب
          </button>
        )}
      </div>
      <h2 className="mt-10 text-lg font-extrabold tracking-tight">طلبات الاشتراك</h2>
      {detail.purchaseRequests.length === 0 ? (
        <p className="mt-3 text-sm text-muted">لا توجد طلبات.</p>
      ) : (
        <AdminTable>
          <table className="mt-3 min-w-full text-sm">
            <thead className="bg-surface-warm">
              <tr>
                <th className="px-4 py-3 text-start font-medium">الطلب</th>
                <th className="px-4 py-3 text-start font-medium">الدورة</th>
                <th className="px-4 py-3 text-start font-medium">المبلغ</th>
                <th className="px-4 py-3 text-start font-medium">الحالة</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {detail.purchaseRequests.map((item) => (
                <tr key={item.id}>
                  <td className="px-4 py-3">
                    <Link href={`/admin/purchase-requests/${item.id}`} className="text-accent hover:underline">
                      {item.requestNumber}
                    </Link>
                  </td>
                  <td className="px-4 py-3">{item.courseTitle}</td>
                  <td className="px-4 py-3">{formatIqd(item.amountIQD)}</td>
                  <td className="px-4 py-3">
                    <StatusBadge label={purchaseLabel(item.status)} tone={statusTone(item.status)} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </AdminTable>
      )}
      <h2 className="mt-10 text-lg font-extrabold tracking-tight">الاشتراكات والتقدم</h2>
      {detail.enrollments.length === 0 ? (
        <p className="mt-3 text-sm text-muted">لا توجد اشتراكات.</p>
      ) : (
        <AdminTable>
          <table className="mt-3 min-w-full text-sm">
            <thead className="bg-surface-warm">
              <tr>
                <th className="px-4 py-3 text-start font-medium">الدورة</th>
                <th className="px-4 py-3 text-start font-medium">الحالة</th>
                <th className="px-4 py-3 text-start font-medium">التقدم</th>
                <th className="px-4 py-3 text-start font-medium">البداية</th>
                <th className="px-4 py-3 text-start font-medium">إجراءات</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {detail.enrollments.map((item) => (
                <tr key={item.id}>
                  <td className="px-4 py-3">{item.courseTitle}</td>
                  <td className="px-4 py-3">
                    <StatusBadge
                      label={labelOrRaw(enrollmentStatusLabel, item.status)}
                      tone={statusTone(item.status)}
                    />
                  </td>
                  <td className="px-4 py-3">
                    {item.progressPercent}% ({item.completedLessons}/{item.totalLessons})
                  </td>
                  <td className="px-4 py-3 text-muted">{formatBaghdadDateTime(item.startedAt)}</td>
                  <td className="px-4 py-3">
                    <div className="flex flex-wrap gap-2">
                      {item.status === "Active" ? (
                        <button
                          type="button"
                          className="text-sm text-accent hover:underline"
                          onClick={() => setConfirm({ kind: "suspend-course", courseId: item.courseId, title: item.courseTitle })}
                        >
                          إيقاف
                        </button>
                      ) : null}
                      {item.status === "Suspended" ? (
                        <button
                          type="button"
                          className="text-sm text-accent hover:underline"
                          onClick={() => setConfirm({ kind: "restore-course", courseId: item.courseId, title: item.courseTitle })}
                        >
                          استعادة
                        </button>
                      ) : null}
                      {item.status !== "Revoked" ? (
                        <button
                          type="button"
                          className="text-sm hover:underline"
                          onClick={() => setConfirm({ kind: "revoke-course", courseId: item.courseId, title: item.courseTitle })}
                        >
                          إلغاء الدورة
                        </button>
                      ) : null}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </AdminTable>
      )}
      <ConfirmDialog
        open={confirm !== null}
        title={confirmTitle}
        description="لن يتم تنفيذ الإجراء إلا بعد التأكيد."
        confirmLabel="تأكيد"
        tone={confirm?.kind === "revoke-course" || confirm?.kind === "suspend-account" ? "danger" : "default"}
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
