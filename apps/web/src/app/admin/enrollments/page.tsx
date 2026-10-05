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
import { activationMethodLabel, enrollmentStatusLabel, labelOrRaw, statusTone } from "@/lib/admin/labels";
import { readPage, readPageSize, withListQuery } from "@/lib/admin/query";
import type { AdminEnrollmentSummary } from "@/lib/admin/types";
import type { Paged } from "@/lib/api/public-types";
import { formatBaghdadDateTime } from "@/lib/format";
import { safeInternalPath } from "@/lib/safe-path";

export default function EnrollmentsPage() {
  return (
    <Suspense fallback={<LoadingState />}>
      <EnrollmentsContent />
    </Suspense>
  );
}

function EnrollmentsContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const page = readPage(searchParams.get("page"));
  const pageSize = readPageSize(searchParams.get("pageSize"));
  const search = searchParams.get("search") ?? "";
  const status = searchParams.get("status") ?? "";
  const courseId = searchParams.get("courseId") ?? "";
  const userId = searchParams.get("userId") ?? "";
  const [data, setData] = useState<Paged<AdminEnrollmentSummary> | null>(null);
  const [error, setError] = useState("");
  const [forbidden, setForbidden] = useState(false);

  useEffect(() => {
    adminJson<Paged<AdminEnrollmentSummary>>(
      withListQuery("/api/admin/enrollments", { page, pageSize, search, status, courseId, userId }),
      "تعذر تحميل الاشتراكات."
    )
      .then(setData)
      .catch((caught) => {
        if (isUnauthorized(caught)) {
          router.replace(`/login?from=${encodeURIComponent(safeInternalPath("/admin/enrollments"))}`);
          return;
        }
        if (isForbidden(caught)) {
          setForbidden(true);
          return;
        }
        setError(errorMessage(caught, "تعذر تحميل الاشتراكات."));
      });
  }, [page, pageSize, search, status, courseId, userId, router]);

  if (forbidden) {
    return <ForbiddenState />;
  }

  return (
    <>
      <PageHeader title="الاشتراكات" description="اشتراكات الطلاب في الدورات." />
      <FilterBar pathname="/admin/enrollments" values={{ pageSize: String(pageSize) }}>
        <FilterField label="بحث" name="search" defaultValue={search} placeholder="اسم الطالب أو الدورة" />
        <FilterSelect
          label="الحالة"
          name="status"
          defaultValue={status}
          options={[
            { value: "", label: "كل الحالات" },
            { value: "Active", label: "نشط" },
            { value: "Suspended", label: "موقوف" },
            { value: "Revoked", label: "ملغى" },
            { value: "Expired", label: "منتهٍ" }
          ]}
        />
        <FilterField label="معرّف الدورة" name="courseId" defaultValue={courseId} />
        <FilterField label="معرّف المستخدم" name="userId" defaultValue={userId} />
      </FilterBar>
      {error ? <ErrorState message={error} /> : null}
      {!data && !error ? <LoadingState /> : null}
      {data && data.items.length === 0 ? <EmptyState title="لا توجد اشتراكات" /> : null}
      {data && data.items.length > 0 ? (
        <>
          <AdminTable>
            <table className="min-w-full text-sm">
              <thead className="bg-surface-warm">
                <tr>
                  <th className="px-4 py-3 text-start font-medium">الطالب</th>
                  <th className="px-4 py-3 text-start font-medium">الدورة</th>
                  <th className="px-4 py-3 text-start font-medium">الحالة</th>
                  <th className="px-4 py-3 text-start font-medium">طريقة التفعيل</th>
                  <th className="px-4 py-3 text-start font-medium">البداية</th>
                  <th className="px-4 py-3 text-start font-medium">الانتهاء</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {data.items.map((item) => (
                  <tr key={item.id}>
                    <td className="px-4 py-3">
                      <Link href={`/admin/students/${item.userId}`} className="font-medium text-accent hover:underline">
                        {item.studentName}
                      </Link>
                      <p className="text-xs text-muted">{item.studentEmail}</p>
                    </td>
                    <td className="px-4 py-3">{item.courseTitle}</td>
                    <td className="px-4 py-3">
                      <StatusBadge label={labelOrRaw(enrollmentStatusLabel, item.status)} tone={statusTone(item.status)} />
                    </td>
                    <td className="px-4 py-3">
                      {labelOrRaw(activationMethodLabel, item.activationMethod)}
                      {item.purchaseRequestNumber ? ` · ${item.purchaseRequestNumber}` : ""}
                    </td>
                    <td className="px-4 py-3 text-muted">{formatBaghdadDateTime(item.startedAt)}</td>
                    <td className="px-4 py-3 text-muted">{formatBaghdadDateTime(item.expiresAt) || "—"}</td>
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
