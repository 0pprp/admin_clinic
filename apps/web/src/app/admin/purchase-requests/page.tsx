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
import { purchaseLabel, statusTone } from "@/lib/admin/labels";
import { DEFAULT_PAGE_SIZE, readPage, readPageSize, withListQuery } from "@/lib/admin/query";
import type { AdminPurchaseSummary } from "@/lib/admin/types";
import type { Paged } from "@/lib/api/public-types";
import { formatBaghdadDateTime, formatIqd } from "@/lib/format";
import { safeInternalPath } from "@/lib/safe-path";

const STATUS_OPTIONS = [
  { value: "", label: "كل الحالات" },
  { value: "Pending", label: "جديد" },
  { value: "Contacted", label: "تم التواصل" },
  { value: "AwaitingPayment", label: "بانتظار الدفع" },
  { value: "PaymentReceived", label: "تم استلام الدفع" },
  { value: "ActivationCodeIssued", label: "تم إصدار الكود" },
  { value: "Completed", label: "مكتمل" },
  { value: "Rejected", label: "مرفوض" },
  { value: "Cancelled", label: "ملغي" }
];

export default function PurchaseRequestsPage() {
  return (
    <Suspense fallback={<LoadingState />}>
      <PurchaseRequestsContent />
    </Suspense>
  );
}

function PurchaseRequestsContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const page = readPage(searchParams.get("page"));
  const pageSize = readPageSize(searchParams.get("pageSize"));
  const search = searchParams.get("search") ?? "";
  const status = searchParams.get("status") ?? "";
  const courseId = searchParams.get("courseId") ?? "";
  const fromDate = searchParams.get("fromDate") ?? "";
  const toDate = searchParams.get("toDate") ?? "";
  const [data, setData] = useState<Paged<AdminPurchaseSummary> | null>(null);
  const [error, setError] = useState("");
  const [forbidden, setForbidden] = useState(false);

  useEffect(() => {
    const query = withListQuery("/api/admin/purchase-requests", {
      page,
      pageSize,
      search,
      status,
      courseId,
      fromDate,
      toDate: toDate ? `${toDate}T23:59:59.999+03:00` : undefined
    });
    adminJson<Paged<AdminPurchaseSummary>>(query, "تعذر تحميل طلبات الاشتراك.")
      .then(setData)
      .catch((caught) => {
        if (isUnauthorized(caught)) {
          router.replace(`/login?from=${encodeURIComponent(safeInternalPath("/admin/purchase-requests"))}`);
          return;
        }
        if (isForbidden(caught)) {
          setForbidden(true);
          return;
        }
        setError(errorMessage(caught, "تعذر تحميل طلبات الاشتراك."));
      });
  }, [page, pageSize, search, status, courseId, fromDate, toDate, router]);

  if (forbidden) {
    return <ForbiddenState />;
  }

  return (
    <>
      <PageHeader title="طلبات الاشتراك" description="متابعة طلبات الاشتراك وحالات الدفع والتفعيل." />
      <FilterBar
        pathname="/admin/purchase-requests"
        values={{ pageSize: String(pageSize || DEFAULT_PAGE_SIZE) }}
      >
        <FilterField label="بحث" name="search" defaultValue={search} placeholder="الاسم أو الهاتف أو رقم الطلب" />
        <FilterSelect label="الحالة" name="status" defaultValue={status} options={STATUS_OPTIONS} />
        <FilterField label="معرّف الدورة" name="courseId" defaultValue={courseId} placeholder="اختياري" />
        <FilterField label="من تاريخ" name="fromDate" type="date" defaultValue={fromDate} />
        <FilterField label="إلى تاريخ" name="toDate" type="date" defaultValue={toDate} />
      </FilterBar>
      {error ? <ErrorState message={error} /> : null}
      {!data && !error ? <LoadingState /> : null}
      {data && data.items.length === 0 ? (
        <EmptyState title="لا توجد طلبات" description="لا توجد نتائج مطابقة لعوامل التصفية الحالية." />
      ) : null}
      {data && data.items.length > 0 ? (
        <>
          <AdminTable>
            <table className="min-w-full text-sm">
              <thead className="bg-surface-warm text-start">
                <tr>
                  <th className="px-4 py-3 font-medium">رقم الطلب</th>
                  <th className="px-4 py-3 font-medium">الطالب</th>
                  <th className="px-4 py-3 font-medium">الهاتف</th>
                  <th className="px-4 py-3 font-medium">الدورة</th>
                  <th className="px-4 py-3 font-medium">المبلغ</th>
                  <th className="px-4 py-3 font-medium">الحالة</th>
                  <th className="px-4 py-3 font-medium">التاريخ</th>
                  <th className="px-4 py-3 font-medium">إجراءات</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {data.items.map((item) => (
                  <tr key={item.id}>
                    <td className="px-4 py-3 font-medium">{item.requestNumber}</td>
                    <td className="px-4 py-3">{item.fullName}</td>
                    <td className="px-4 py-3">{item.phoneNumber}</td>
                    <td className="px-4 py-3">{item.courseTitle}</td>
                    <td className="px-4 py-3">{formatIqd(item.amountIQD)}</td>
                    <td className="px-4 py-3">
                      <StatusBadge label={purchaseLabel(item.status)} tone={statusTone(item.status)} />
                    </td>
                    <td className="px-4 py-3 text-muted">{formatBaghdadDateTime(item.createdAt)}</td>
                    <td className="px-4 py-3">
                      <Link href={`/admin/purchase-requests/${item.id}`} className="text-accent hover:underline">
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
