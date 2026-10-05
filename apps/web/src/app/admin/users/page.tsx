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
import { adminJson, errorMessage, isForbidden, isUnauthorized } from "@/lib/admin/http";
import { accountStatusLabel, labelOrRaw, roleLabel, statusTone } from "@/lib/admin/labels";
import { readPage, readPageSize, withListQuery } from "@/lib/admin/query";
import type { AdminStudentSummary } from "@/lib/admin/types";
import type { Paged } from "@/lib/api/public-types";
import { formatBaghdadDateTime } from "@/lib/format";
import { safeInternalPath } from "@/lib/safe-path";

const ROLE_OPTIONS = ["Admin", "ContentManager", "Support", "Student"] as const;

export default function UsersPage() {
  return (
    <Suspense fallback={<LoadingState />}>
      <UsersContent />
    </Suspense>
  );
}

function UsersContent() {
  const router = useRouter();
  const toast = useToast();
  const searchParams = useSearchParams();
  const page = readPage(searchParams.get("page"));
  const pageSize = readPageSize(searchParams.get("pageSize"));
  const search = searchParams.get("search") ?? "";
  const status = searchParams.get("status") ?? "";
  const [data, setData] = useState<Paged<AdminStudentSummary> | null>(null);
  const [error, setError] = useState("");
  const [forbidden, setForbidden] = useState(false);
  const [editing, setEditing] = useState<{ id: string; name: string; roles: string[] } | null>(null);
  const [pending, setPending] = useState(false);

  async function load() {
    try {
      setData(
        await adminJson<Paged<AdminStudentSummary>>(
          withListQuery("/api/admin/users", { page, pageSize, search, status }),
          "تعذر تحميل المستخدمين."
        )
      );
    } catch (caught) {
      if (isUnauthorized(caught)) {
        router.replace(`/login?from=${encodeURIComponent(safeInternalPath("/admin/users"))}`);
        return;
      }
      if (isForbidden(caught)) {
        setForbidden(true);
        return;
      }
      setError(errorMessage(caught, "تعذر تحميل المستخدمين."));
    }
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page, pageSize, search, status]);

  async function saveRoles() {
    if (!editing) {
      return;
    }

    setPending(true);
    setError("");
    try {
      await adminJson(`/api/admin/users/${editing.id}/roles`, "تعذر حفظ الأدوار.", {
        method: "POST",
        body: JSON.stringify({ roles: editing.roles })
      });
      toast.show("تم تحديث الأدوار.");
      setEditing(null);
      await load();
    } catch (caught) {
      setError(errorMessage(caught, "تعذر حفظ الأدوار."));
    } finally {
      setPending(false);
    }
  }

  if (forbidden) {
    return <ForbiddenState />;
  }

  return (
    <>
      <PageHeader title="المستخدمون" description="إدارة أدوار الحسابات." />
      <FilterBar pathname="/admin/users" values={{ pageSize: String(pageSize) }}>
        <FilterField label="بحث" name="search" defaultValue={search} />
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
      {data && data.items.length === 0 ? <EmptyState title="لا يوجد مستخدمون" /> : null}
      {data && data.items.length > 0 ? (
        <>
          <AdminTable>
            <table className="min-w-full text-sm">
              <thead className="bg-surface-warm">
                <tr>
                  <th className="px-4 py-3 text-start font-medium">الاسم</th>
                  <th className="px-4 py-3 text-start font-medium">البريد</th>
                  <th className="px-4 py-3 text-start font-medium">الحالة</th>
                  <th className="px-4 py-3 text-start font-medium">الأدوار</th>
                  <th className="px-4 py-3 text-start font-medium">تاريخ الإنشاء</th>
                  <th className="px-4 py-3 text-start font-medium">إجراءات</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {data.items.map((item) => (
                  <tr key={item.id}>
                    <td className="px-4 py-3 font-medium">{item.fullName}</td>
                    <td className="px-4 py-3 break-all">{item.email}</td>
                    <td className="px-4 py-3">
                      <StatusBadge
                        label={labelOrRaw(accountStatusLabel, item.accountStatus)}
                        tone={statusTone(item.accountStatus)}
                      />
                    </td>
                    <td className="px-4 py-3">{item.roles.map((role) => labelOrRaw(roleLabel, role)).join("، ")}</td>
                    <td className="px-4 py-3 text-muted">{formatBaghdadDateTime(item.createdAt)}</td>
                    <td className="px-4 py-3">
                      <button
                        type="button"
                        className="text-accent hover:underline"
                        onClick={() => setEditing({ id: item.id, name: item.fullName, roles: [...item.roles] })}
                      >
                        تعديل الأدوار
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
      <ConfirmDialog
        open={editing !== null}
        title={editing ? `حفظ أدوار ${editing.name}؟` : ""}
        description="سيتم تطبيق الأدوار المحددة فوراً."
        confirmLabel="حفظ الأدوار"
        pending={pending}
        onClose={() => !pending && setEditing(null)}
        onConfirm={saveRoles}
      >
        {editing ? (
          <fieldset className="space-y-2">
            <legend className="mb-2 text-sm font-medium">الأدوار</legend>
            {ROLE_OPTIONS.map((role) => (
              <label key={role} className="flex items-center gap-2 text-sm">
                <input
                  type="checkbox"
                  checked={editing.roles.includes(role)}
                  onChange={(event) => {
                    setEditing({
                      ...editing,
                      roles: event.target.checked
                        ? [...editing.roles, role]
                        : editing.roles.filter((item) => item !== role)
                    });
                  }}
                />
                {labelOrRaw(roleLabel, role)}
              </label>
            ))}
          </fieldset>
        ) : null}
      </ConfirmDialog>
    </>
  );
}
