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
import { contactStatusLabel, labelOrRaw, statusTone } from "@/lib/admin/labels";
import { readPage, readPageSize, withListQuery } from "@/lib/admin/query";
import type { AdminContactSummary } from "@/lib/admin/types";
import type { Paged } from "@/lib/api/public-types";
import { formatBaghdadDateTime } from "@/lib/format";
import { safeInternalPath } from "@/lib/safe-path";

export default function ContactMessagesPage() {
  return (
    <Suspense fallback={<LoadingState />}>
      <ContactMessagesContent />
    </Suspense>
  );
}

function ContactMessagesContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const page = readPage(searchParams.get("page"));
  const pageSize = readPageSize(searchParams.get("pageSize"));
  const search = searchParams.get("search") ?? "";
  const status = searchParams.get("status") ?? "";
  const [data, setData] = useState<Paged<AdminContactSummary> | null>(null);
  const [error, setError] = useState("");
  const [forbidden, setForbidden] = useState(false);

  useEffect(() => {
    adminJson<Paged<AdminContactSummary>>(
      withListQuery("/api/admin/contact-messages", { page, pageSize, search, status }),
      "تعذر تحميل الرسائل."
    )
      .then(setData)
      .catch((caught) => {
        if (isUnauthorized(caught)) {
          router.replace(`/login?from=${encodeURIComponent(safeInternalPath("/admin/contact-messages"))}`);
          return;
        }
        if (isForbidden(caught)) {
          setForbidden(true);
          return;
        }
        setError(errorMessage(caught, "تعذر تحميل الرسائل."));
      });
  }, [page, pageSize, search, status, router]);

  if (forbidden) {
    return <ForbiddenState />;
  }

  return (
    <>
      <PageHeader title="رسائل التواصل" />
      <FilterBar pathname="/admin/contact-messages" values={{ pageSize: String(pageSize) }}>
        <FilterField label="بحث" name="search" defaultValue={search} />
        <FilterSelect
          label="الحالة"
          name="status"
          defaultValue={status}
          options={[
            { value: "", label: "كل الحالات" },
            { value: "New", label: "جديد" },
            { value: "Read", label: "مقروء" },
            { value: "Replied", label: "تم الرد" },
            { value: "Archived", label: "مؤرشف" }
          ]}
        />
      </FilterBar>
      {error ? <ErrorState message={error} /> : null}
      {!data && !error ? <LoadingState /> : null}
      {data && data.items.length === 0 ? <EmptyState title="لا توجد رسائل" /> : null}
      {data && data.items.length > 0 ? (
        <>
          <AdminTable>
            <table className="min-w-full text-sm">
              <thead className="bg-surface-warm">
                <tr>
                  <th className="px-4 py-3 text-start font-medium">الاسم</th>
                  <th className="px-4 py-3 text-start font-medium">البريد</th>
                  <th className="px-4 py-3 text-start font-medium">الموضوع</th>
                  <th className="px-4 py-3 text-start font-medium">الحالة</th>
                  <th className="px-4 py-3 text-start font-medium">التاريخ</th>
                  <th className="px-4 py-3 text-start font-medium">إجراءات</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {data.items.map((item) => (
                  <tr key={item.id}>
                    <td className="px-4 py-3 font-medium">{item.name}</td>
                    <td className="px-4 py-3 break-all">{item.email}</td>
                    <td className="px-4 py-3">{item.subject}</td>
                    <td className="px-4 py-3">
                      <StatusBadge label={labelOrRaw(contactStatusLabel, item.status)} tone={statusTone(item.status)} />
                    </td>
                    <td className="px-4 py-3 text-muted">{formatBaghdadDateTime(item.createdAt)}</td>
                    <td className="px-4 py-3">
                      <Link href={`/admin/contact-messages/${item.id}`} className="text-accent hover:underline">
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
