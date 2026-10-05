"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { Suspense, useEffect, useState } from "react";
import { AdminPagination } from "@/components/admin/AdminPagination";
import { ConfirmDialog } from "@/components/admin/ConfirmDialog";
import { FilterBar, FilterField, FilterSelect } from "@/components/admin/FilterBar";
import { ForbiddenState } from "@/components/admin/ForbiddenState";
import { AdminTable, ErrorState, LoadingState, PageHeader } from "@/components/admin/PageHeader";
import { StatusBadge } from "@/components/admin/StatusBadge";
import { useToast } from "@/components/admin/ToastProvider";
import { EmptyState } from "@/components/shared/EmptyState";
import { adminJson, adminVoid, errorMessage, isForbidden, isUnauthorized } from "@/lib/admin/http";
import { activationStatusLabel, labelOrRaw, statusTone } from "@/lib/admin/labels";
import { readPage, readPageSize, withListQuery } from "@/lib/admin/query";
import type { AdminActivationCodeSummary } from "@/lib/admin/types";
import type { Paged } from "@/lib/api/public-types";
import { formatBaghdadDateTime } from "@/lib/format";
import { safeInternalPath } from "@/lib/safe-path";

export default function ActivationCodesPage() {
  return (
    <Suspense fallback={<LoadingState />}>
      <ActivationCodesContent />
    </Suspense>
  );
}

function ActivationCodesContent() {
  const router = useRouter();
  const toast = useToast();
  const searchParams = useSearchParams();
  const page = readPage(searchParams.get("page"));
  const pageSize = readPageSize(searchParams.get("pageSize"));
  const status = searchParams.get("status") ?? "";
  const courseId = searchParams.get("courseId") ?? "";
  const userId = searchParams.get("userId") ?? "";
  const [data, setData] = useState<Paged<AdminActivationCodeSummary> | null>(null);
  const [error, setError] = useState("");
  const [forbidden, setForbidden] = useState(false);
  const [pending, setPending] = useState(false);
  const [revokeId, setRevokeId] = useState<string | null>(null);

  async function load() {
    try {
      setData(
        await adminJson<Paged<AdminActivationCodeSummary>>(
          withListQuery("/api/admin/activation-codes", { page, pageSize, status, courseId, userId }),
          "تعذر تحميل أكواد التفعيل."
        )
      );
    } catch (caught) {
      if (isUnauthorized(caught)) {
        router.replace(`/login?from=${encodeURIComponent(safeInternalPath("/admin/activation-codes"))}`);
        return;
      }
      if (isForbidden(caught)) {
        setForbidden(true);
        return;
      }
      setError(errorMessage(caught, "تعذر تحميل أكواد التفعيل."));
    }
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page, pageSize, status, courseId, userId]);

  async function revoke() {
    if (!revokeId) {
      return;
    }

    setPending(true);
    setError("");
    try {
      await adminVoid(`/api/admin/activation-codes/${revokeId}/revoke`, "تعذر إلغاء الكود.", { method: "POST" });
      toast.show("تم إلغاء الكود.");
      setRevokeId(null);
      await load();
    } catch (caught) {
      setError(errorMessage(caught, "تعذر إلغاء الكود."));
    } finally {
      setPending(false);
    }
  }

  if (forbidden) {
    return <ForbiddenState />;
  }

  return (
    <>
      <PageHeader
        title="أكواد التفعيل"
        description="لا تُعرض الأكواد النصية هنا. يُعرض الكود مرة واحدة فقط عند الإصدار من صفحة الطلب."
      />
      <FilterBar pathname="/admin/activation-codes" values={{ pageSize: String(pageSize) }}>
        <FilterSelect
          label="الحالة"
          name="status"
          defaultValue={status}
          options={[
            { value: "", label: "كل الحالات" },
            { value: "Active", label: "نشط" },
            { value: "Used", label: "مستخدم" },
            { value: "Expired", label: "منتهٍ" },
            { value: "Revoked", label: "ملغى" }
          ]}
        />
        <FilterField label="معرّف الدورة" name="courseId" defaultValue={courseId} />
        <FilterField label="معرّف المستخدم" name="userId" defaultValue={userId} />
      </FilterBar>
      {error ? <ErrorState message={error} /> : null}
      {!data && !error ? <LoadingState /> : null}
      {data && data.items.length === 0 ? <EmptyState title="لا توجد أكواد" /> : null}
      {data && data.items.length > 0 ? (
        <>
          <AdminTable>
            <table className="min-w-full text-sm">
              <thead className="bg-surface-warm">
                <tr>
                  <th className="px-4 py-3 text-start font-medium">الطالب</th>
                  <th className="px-4 py-3 text-start font-medium">الدورة</th>
                  <th className="px-4 py-3 text-start font-medium">رقم الطلب</th>
                  <th className="px-4 py-3 text-start font-medium">الحالة</th>
                  <th className="px-4 py-3 text-start font-medium">تاريخ الإصدار</th>
                  <th className="px-4 py-3 text-start font-medium">الانتهاء</th>
                  <th className="px-4 py-3 text-start font-medium">إجراءات</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {data.items.map((item) => (
                  <tr key={item.id}>
                    <td className="px-4 py-3">
                      <p className="font-medium">{item.userFullName}</p>
                      <p className="text-xs text-muted">{item.userEmail}</p>
                    </td>
                    <td className="px-4 py-3">{item.courseTitle}</td>
                    <td className="px-4 py-3">{item.requestNumber}</td>
                    <td className="px-4 py-3">
                      <StatusBadge label={labelOrRaw(activationStatusLabel, item.status)} tone={statusTone(item.status)} />
                    </td>
                    <td className="px-4 py-3 text-muted">{formatBaghdadDateTime(item.createdAt)}</td>
                    <td className="px-4 py-3 text-muted">{formatBaghdadDateTime(item.expiresAt) || "—"}</td>
                    <td className="px-4 py-3">
                      {item.status === "Active" ? (
                        <button type="button" className="text-sm hover:underline" onClick={() => setRevokeId(item.id)}>
                          إلغاء
                        </button>
                      ) : (
                        "—"
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </AdminTable>
          <AdminPagination page={data.page} pageSize={data.pageSize} totalCount={data.totalCount} />
        </>
      ) : null}
      <ConfirmDialog
        open={revokeId !== null}
        title="إلغاء كود التفعيل؟"
        description="لن يعود هذا الكود صالحاً للاستخدام."
        confirmLabel="إلغاء الكود"
        tone="danger"
        pending={pending}
        onClose={() => !pending && setRevokeId(null)}
        onConfirm={revoke}
      />
    </>
  );
}
