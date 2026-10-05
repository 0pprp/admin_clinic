"use client";

import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { Suspense, useEffect, useState } from "react";
import { AdminPagination } from "@/components/admin/AdminPagination";
import { FilterBar, FilterField, FilterSelect } from "@/components/admin/FilterBar";
import { ForbiddenState } from "@/components/admin/ForbiddenState";
import { AdminTable, ErrorState, LoadingState, PageHeader } from "@/components/admin/PageHeader";
import { StatusBadge } from "@/components/admin/StatusBadge";
import { EmptyState } from "@/components/shared/EmptyState";
import { adminJson, errorMessage, isForbidden, isUnauthorized } from "@/lib/admin/http";
import { consultationStatusLabel, consultationTypeLabel, labelOrRaw, statusTone } from "@/lib/admin/labels";
import { readPage, readPageSize, withListQuery } from "@/lib/admin/query";
import type { AdminConsultationSummary } from "@/lib/admin/types";
import type { Paged } from "@/lib/api/public-types";
import { formatDate } from "@/lib/format";
import { safeInternalPath } from "@/lib/safe-path";

export default function ConsultationsPage() {
  return (
    <Suspense fallback={<LoadingState />}>
      <ConsultationsContent />
    </Suspense>
  );
}

function ConsultationsContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const page = readPage(searchParams.get("page"));
  const pageSize = readPageSize(searchParams.get("pageSize"));
  const search = searchParams.get("search") ?? "";
  const status = searchParams.get("status") ?? "";
  const [data, setData] = useState<Paged<AdminConsultationSummary> | null>(null);
  const [error, setError] = useState("");
  const [forbidden, setForbidden] = useState(false);

  useEffect(() => {
    adminJson<Paged<AdminConsultationSummary>>(
      withListQuery("/api/admin/consultations", { page, pageSize, search, status }),
      "تعذر تحميل الاستشارات."
    )
      .then(setData)
      .catch((caught) => {
        if (isUnauthorized(caught)) {
          router.replace(`/login?from=${encodeURIComponent(safeInternalPath("/admin/consultations"))}`);
          return;
        }
        if (isForbidden(caught)) {
          setForbidden(true);
          return;
        }
        setError(errorMessage(caught, "تعذر تحميل الاستشارات."));
      });
  }, [page, pageSize, search, status, router]);

  if (forbidden) {
    return <ForbiddenState />;
  }

  return (
    <>
      <PageHeader title="الاستشارات" description="طلبات الاستشارة وحالات المتابعة." />
      <FilterBar pathname="/admin/consultations" values={{ pageSize: String(pageSize) }}>
        <FilterField label="بحث" name="search" defaultValue={search} />
        <FilterSelect
          label="الحالة"
          name="status"
          defaultValue={status}
          options={[
            { value: "", label: "كل الحالات" },
            { value: "New", label: "جديد" },
            { value: "Contacted", label: "تم التواصل" },
            { value: "Scheduled", label: "مجدول" },
            { value: "Completed", label: "مكتمل" },
            { value: "Cancelled", label: "ملغي" },
            { value: "Rejected", label: "مرفوض" }
          ]}
        />
      </FilterBar>
      {error ? <ErrorState message={error} /> : null}
      {!data && !error ? <LoadingState /> : null}
      {data && data.items.length === 0 ? <EmptyState title="لا توجد استشارات" /> : null}
      {data && data.items.length > 0 ? (
        <>
          <AdminTable>
            <table className="min-w-full text-sm">
              <thead className="bg-surface-warm">
                <tr>
                  <th className="px-4 py-3 text-start font-medium">الرقم</th>
                  <th className="px-4 py-3 text-start font-medium">الاسم</th>
                  <th className="px-4 py-3 text-start font-medium">الهاتف</th>
                  <th className="px-4 py-3 text-start font-medium">النوع</th>
                  <th className="px-4 py-3 text-start font-medium">التاريخ المفضل</th>
                  <th className="px-4 py-3 text-start font-medium">الحالة</th>
                  <th className="px-4 py-3 text-start font-medium">إجراءات</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {data.items.map((item) => (
                  <tr key={item.id}>
                    <td className="px-4 py-3 font-medium">{item.requestNumber}</td>
                    <td className="px-4 py-3">{item.fullName}</td>
                    <td className="px-4 py-3">{item.phoneNumber}</td>
                    <td className="px-4 py-3">{labelOrRaw(consultationTypeLabel, item.consultationType)}</td>
                    <td className="px-4 py-3 text-muted">{formatDate(item.preferredDate) || "—"}</td>
                    <td className="px-4 py-3">
                      <StatusBadge
                        label={labelOrRaw(consultationStatusLabel, item.status)}
                        tone={statusTone(item.status)}
                      />
                    </td>
                    <td className="px-4 py-3">
                      <Link href={`/admin/consultations/${item.id}`} className="text-accent hover:underline">
                        عرض
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </AdminTable>
          <AdminPagination page={data.page} pageSize={data.pageSize} totalCount={data.totalCount} />
        </>
      ) : null}
    </>
  );
}
