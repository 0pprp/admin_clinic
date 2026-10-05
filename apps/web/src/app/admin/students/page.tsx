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
import { accountStatusLabel, labelOrRaw, roleLabel, statusTone } from "@/lib/admin/labels";
import { readPage, readPageSize, withListQuery } from "@/lib/admin/query";
import type { AdminStudentSummary } from "@/lib/admin/types";
import type { Paged } from "@/lib/api/public-types";
import { formatBaghdadDateTime } from "@/lib/format";
import { safeInternalPath } from "@/lib/safe-path";

export default function StudentsPage() {
  return (
    <Suspense fallback={<LoadingState />}>
      <StudentsContent />
    </Suspense>
  );
}

function StudentsContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const page = readPage(searchParams.get("page"));
  const pageSize = readPageSize(searchParams.get("pageSize"));
  const search = searchParams.get("search") ?? "";
  const status = searchParams.get("status") ?? "";
  const [data, setData] = useState<Paged<AdminStudentSummary> | null>(null);
  const [error, setError] = useState("");
  const [forbidden, setForbidden] = useState(false);

  useEffect(() => {
    adminJson<Paged<AdminStudentSummary>>(
      withListQuery("/api/admin/students", { page, pageSize, search, status }),
      "تعذر تحميل الطلاب."
    )
      .then(setData)
      .catch((caught) => {
        if (isUnauthorized(caught)) {
          router.replace(`/login?from=${encodeURIComponent(safeInternalPath("/admin/students"))}`);
          return;
        }
        if (isForbidden(caught)) {
          setForbidden(true);
          return;
        }
        setError(errorMessage(caught, "تعذر تحميل الطلاب."));
      });
  }, [page, pageSize, search, status, router]);

  if (forbidden) {
    return <ForbiddenState />;
  }

  return (
    <>
      <PageHeader title="الطلاب" description="حسابات الطلاب وحالة الحساب." />
      <FilterBar pathname="/admin/students" values={{ pageSize: String(pageSize) }}>
        <FilterField label="بحث" name="search" defaultValue={search} placeholder="الاسم أو البريد أو الهاتف" />
        <FilterSelect
          label="الحالة"
          name="status"
          defaultValue={status}
          options={[
            { value: "", label: "كل الحالات" },
            { value: "Active", label: "نشط" },
            { value: "Suspended", label: "موقوف" },
            { value: "Blocked", label: "محظور" }
          ]}
        />
      </FilterBar>
      {error ? <ErrorState message={error} /> : null}
      {!data && !error ? <LoadingState /> : null}
      {data && data.items.length === 0 ? <EmptyState title="لا يوجد طلاب مطابقون" /> : null}
      {data && data.items.length > 0 ? (
        <>
          <AdminTable>
            <table className="min-w-full text-sm">
              <thead className="bg-surface-warm">
                <tr>
                  <th className="px-4 py-3 text-start font-medium">الاسم</th>
                  <th className="px-4 py-3 text-start font-medium">البريد</th>
                  <th className="px-4 py-3 text-start font-medium">الهاتف</th>
                  <th className="px-4 py-3 text-start font-medium">الحالة</th>
                  <th className="px-4 py-3 text-start font-medium">الأدوار</th>
                  <th className="px-4 py-3 text-start font-medium">آخر دخول</th>
                  <th className="px-4 py-3 text-start font-medium">إجراءات</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {data.items.map((item) => (
                  <tr key={item.id}>
                    <td className="px-4 py-3 font-medium">{item.fullName}</td>
                    <td className="px-4 py-3 break-all">{item.email}</td>
                    <td className="px-4 py-3">{item.phoneNumber || "—"}</td>
                    <td className="px-4 py-3">
                      <StatusBadge
                        label={labelOrRaw(accountStatusLabel, item.accountStatus)}
                        tone={statusTone(item.accountStatus)}
                      />
                    </td>
                    <td className="px-4 py-3">{item.roles.map((role) => labelOrRaw(roleLabel, role)).join("، ")}</td>
                    <td className="px-4 py-3 text-muted">{formatBaghdadDateTime(item.lastLoginAt) || "—"}</td>
                    <td className="px-4 py-3">
                      <Link href={`/admin/students/${item.id}`} className="text-accent hover:underline">
                        الملف
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
