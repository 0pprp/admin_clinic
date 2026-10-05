"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { Suspense, useEffect, useState } from "react";
import { AdminPagination } from "@/components/admin/AdminPagination";
import { FilterBar, FilterField } from "@/components/admin/FilterBar";
import { ForbiddenState } from "@/components/admin/ForbiddenState";
import { AdminTable, ErrorState, LoadingState, PageHeader } from "@/components/admin/PageHeader";
import { EmptyState } from "@/components/shared/EmptyState";
import { adminJson, errorMessage, isForbidden, isUnauthorized } from "@/lib/admin/http";
import { secondaryButtonClassName } from "@/lib/admin/ui";
import { readPage, readPageSize, withListQuery } from "@/lib/admin/query";
import type { AdminAuditLogDetail, AdminAuditLogSummary } from "@/lib/admin/types";
import type { Paged } from "@/lib/api/public-types";
import { formatBaghdadDateTime } from "@/lib/format";
import { safeInternalPath } from "@/lib/safe-path";

export default function AuditLogsPage() {
  return (
    <Suspense fallback={<LoadingState />}>
      <AuditLogsContent />
    </Suspense>
  );
}

function AuditLogsContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const page = readPage(searchParams.get("page"));
  const pageSize = readPageSize(searchParams.get("pageSize"));
  const search = searchParams.get("search") ?? "";
  const action = searchParams.get("action") ?? "";
  const entityType = searchParams.get("entityType") ?? "";
  const fromDate = searchParams.get("fromDate") ?? "";
  const toDate = searchParams.get("toDate") ?? "";
  const [data, setData] = useState<Paged<AdminAuditLogSummary> | null>(null);
  const [error, setError] = useState("");
  const [forbidden, setForbidden] = useState(false);
  const [detail, setDetail] = useState<AdminAuditLogDetail | null>(null);
  const [detailError, setDetailError] = useState("");

  useEffect(() => {
    adminJson<Paged<AdminAuditLogSummary>>(
      withListQuery("/api/admin/audit-logs", {
        page,
        pageSize,
        search,
        action,
        entityType,
        fromDate,
        toDate: toDate ? `${toDate}T23:59:59.999+03:00` : undefined
      }),
      "تعذر تحميل سجل العمليات."
    )
      .then(setData)
      .catch((caught) => {
        if (isUnauthorized(caught)) {
          router.replace(`/login?from=${encodeURIComponent(safeInternalPath("/admin/audit-logs"))}`);
          return;
        }
        if (isForbidden(caught)) {
          setForbidden(true);
          return;
        }
        setError(errorMessage(caught, "تعذر تحميل سجل العمليات."));
      });
  }, [page, pageSize, search, action, entityType, fromDate, toDate, router]);

  async function openDetail(id: string) {
    setDetailError("");
    try {
      setDetail(await adminJson<AdminAuditLogDetail>(`/api/admin/audit-logs/${id}`, "تعذر تحميل التفاصيل."));
    } catch (caught) {
      setDetailError(errorMessage(caught, "تعذر تحميل التفاصيل."));
    }
  }

  if (forbidden) {
    return <ForbiddenState />;
  }

  return (
    <>
      <PageHeader title="سجل العمليات" description="سجل إجراءات الإدارة. البيانات الحساسة مُنقّاة مسبقاً." />
      <FilterBar pathname="/admin/audit-logs" values={{ pageSize: String(pageSize) }}>
        <FilterField label="بحث" name="search" defaultValue={search} />
        <FilterField label="الإجراء" name="action" defaultValue={action} />
        <FilterField label="نوع الكيان" name="entityType" defaultValue={entityType} />
        <FilterField label="من تاريخ" name="fromDate" type="date" defaultValue={fromDate} />
        <FilterField label="إلى تاريخ" name="toDate" type="date" defaultValue={toDate} />
      </FilterBar>
      {error ? <ErrorState message={error} /> : null}
      {!data && !error ? <LoadingState /> : null}
      {data && data.items.length === 0 ? <EmptyState title="لا توجد سجلات" /> : null}
      {data && data.items.length > 0 ? (
        <>
          <AdminTable>
            <table className="min-w-full text-sm">
              <thead className="bg-surface-warm">
                <tr>
                  <th className="px-4 py-3 text-start font-medium">التاريخ</th>
                  <th className="px-4 py-3 text-start font-medium">المنفّذ</th>
                  <th className="px-4 py-3 text-start font-medium">الإجراء</th>
                  <th className="px-4 py-3 text-start font-medium">الكيان</th>
                  <th className="px-4 py-3 text-start font-medium">الوصف</th>
                  <th className="px-4 py-3 text-start font-medium">إجراءات</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {data.items.map((item) => (
                  <tr key={item.id}>
                    <td className="px-4 py-3 text-muted whitespace-nowrap">{formatBaghdadDateTime(item.createdAt)}</td>
                    <td className="px-4 py-3">{item.actorName}</td>
                    <td className="px-4 py-3">{item.action}</td>
                    <td className="px-4 py-3">{item.entityType}</td>
                    <td className="px-4 py-3">{item.description}</td>
                    <td className="px-4 py-3">
                      <button type="button" className="text-accent hover:underline" onClick={() => openDetail(item.id)}>
                        التفاصيل
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </AdminTable>
          <AdminPagination page={data.page} pageSize={data.pageSize} totalCount={data.totalCount} />
        </>
      ) : null}
      {detailError ? <div className="mt-4"><ErrorState message={detailError} /></div> : null}
      {detail ? (
        <section className="mt-6 border border-border bg-surface p-5 text-sm">
          <div className="flex items-start justify-between gap-3">
            <h2 className="text-lg font-semibold">تفاصيل العملية</h2>
            <button type="button" className={secondaryButtonClassName} onClick={() => setDetail(null)}>
              إغلاق
            </button>
          </div>
          <p className="mt-3 text-muted">{detail.action} · {detail.entityType}</p>
          <p className="mt-2">{detail.description}</p>
          {detail.ipAddress ? <p className="mt-2 text-muted">عنوان الشبكة: {detail.ipAddress}</p> : null}
          {detail.metadataJson ? (
            <pre className="mt-4 overflow-x-auto border border-border bg-background p-3 text-xs leading-6 whitespace-pre-wrap">
              {prettyJson(detail.metadataJson)}
            </pre>
          ) : (
            <p className="mt-4 text-muted">لا توجد بيانات إضافية.</p>
          )}
        </section>
      ) : null}
    </>
  );
}

function prettyJson(value: string): string {
  try {
    return JSON.stringify(JSON.parse(value), null, 2);
  } catch {
    return value;
  }
}
