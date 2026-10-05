"use client";

import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { FormEvent, Suspense, useEffect, useState } from "react";
import { AdminPagination } from "@/components/admin/AdminPagination";
import { ConfirmDialog } from "@/components/admin/ConfirmDialog";
import { FilterBar, FilterField, FilterSelect } from "@/components/admin/FilterBar";
import { ForbiddenState } from "@/components/admin/ForbiddenState";
import { AdminTable, ErrorState, Field, LoadingState, PageHeader } from "@/components/admin/PageHeader";
import { StatusBadge } from "@/components/admin/StatusBadge";
import { useToast } from "@/components/admin/ToastProvider";
import { EmptyState } from "@/components/shared/EmptyState";
import { adminJson, errorMessage, isForbidden, isUnauthorized } from "@/lib/admin/http";
import { contentStatusLabel, labelOrRaw, statusTone } from "@/lib/admin/labels";
import { readPage, readPageSize, withListQuery } from "@/lib/admin/query";
import { inputClassName, primaryButtonClassName } from "@/lib/admin/ui";
import type { AdminArticleDetail, AdminArticleSummary } from "@/lib/admin/types";
import type { Paged } from "@/lib/api/public-types";
import { formatBaghdadDateTime } from "@/lib/format";
import { safeInternalPath } from "@/lib/safe-path";

export default function ArticlesPage() {
  return (
    <Suspense fallback={<LoadingState />}>
      <ArticlesContent />
    </Suspense>
  );
}

function ArticlesContent() {
  const router = useRouter();
  const toast = useToast();
  const searchParams = useSearchParams();
  const page = readPage(searchParams.get("page"));
  const pageSize = readPageSize(searchParams.get("pageSize"));
  const search = searchParams.get("search") ?? "";
  const status = searchParams.get("status") ?? "";
  const [data, setData] = useState<Paged<AdminArticleSummary> | null>(null);
  const [error, setError] = useState("");
  const [forbidden, setForbidden] = useState(false);
  const [creating, setCreating] = useState(false);
  const [pending, setPending] = useState(false);
  const [confirm, setConfirm] = useState<{ id: string; kind: "publish" | "archive"; title: string } | null>(null);
  const [title, setTitle] = useState("");
  const [slug, setSlug] = useState("");
  const [excerpt, setExcerpt] = useState("");
  const [content, setContent] = useState("");

  async function load() {
    try {
      setData(
        await adminJson<Paged<AdminArticleSummary>>(
          withListQuery("/api/admin/articles", { page, pageSize, search, status }),
          "تعذر تحميل المقالات."
        )
      );
    } catch (caught) {
      if (isUnauthorized(caught)) {
        router.replace(`/login?from=${encodeURIComponent(safeInternalPath("/admin/articles"))}`);
        return;
      }
      if (isForbidden(caught)) {
        setForbidden(true);
        return;
      }
      setError(errorMessage(caught, "تعذر تحميل المقالات."));
    }
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page, pageSize, search, status]);

  async function onCreate(event: FormEvent) {
    event.preventDefault();
    setPending(true);
    setError("");
    try {
      const created = await adminJson<AdminArticleDetail>("/api/admin/articles", "تعذر إنشاء المقال.", {
        method: "POST",
        body: JSON.stringify({ title, slug, excerpt, content, coverImage: null })
      });
      toast.show("تم إنشاء المقال.");
      router.push(`/admin/articles/${created.id}`);
    } catch (caught) {
      setError(errorMessage(caught, "تعذر إنشاء المقال."));
    } finally {
      setPending(false);
    }
  }

  async function runConfirm() {
    if (!confirm) {
      return;
    }

    setPending(true);
    setError("");
    try {
      await adminJson(`/api/admin/articles/${confirm.id}/${confirm.kind}`, "تعذر تحديث المقال.", { method: "POST" });
      toast.show("تم تحديث المقال.");
      setConfirm(null);
      await load();
    } catch (caught) {
      setError(errorMessage(caught, "تعذر تحديث المقال."));
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
        title="المقالات"
        actions={
          <button type="button" className={primaryButtonClassName} onClick={() => setCreating((value) => !value)}>
            {creating ? "إغلاق" : "مقال جديد"}
          </button>
        }
      />
      {creating ? (
        <form onSubmit={onCreate} className="clinic-card mb-8 grid gap-4 p-5">
          <Field label="العنوان">
            <input className={inputClassName} value={title} onChange={(event) => setTitle(event.target.value)} required />
          </Field>
          <Field label="المسار">
            <input className={inputClassName} value={slug} onChange={(event) => setSlug(event.target.value)} required />
          </Field>
          <Field label="المقتطف">
            <input className={inputClassName} value={excerpt} onChange={(event) => setExcerpt(event.target.value)} required />
          </Field>
          <Field label="المحتوى">
            <textarea className={inputClassName} rows={6} value={content} onChange={(event) => setContent(event.target.value)} required />
          </Field>
          <button type="submit" className={primaryButtonClassName} disabled={pending}>
            إنشاء والانتقال للمحرر
          </button>
        </form>
      ) : null}
      <FilterBar pathname="/admin/articles" values={{ pageSize: String(pageSize) }}>
        <FilterField label="بحث" name="search" defaultValue={search} />
        <FilterSelect
          label="الحالة"
          name="status"
          defaultValue={status}
          options={[
            { value: "", label: "كل الحالات" },
            { value: "Draft", label: "مسودة" },
            { value: "Published", label: "منشور" },
            { value: "Archived", label: "مؤرشف" }
          ]}
        />
      </FilterBar>
      {error ? <ErrorState message={error} /> : null}
      {!data && !error ? <LoadingState /> : null}
      {data && data.items.length === 0 ? <EmptyState title="لا توجد مقالات" /> : null}
      {data && data.items.length > 0 ? (
        <>
          <AdminTable>
            <table className="min-w-full text-sm">
              <thead className="bg-surface-warm">
                <tr>
                  <th className="px-4 py-3 text-start font-medium">العنوان</th>
                  <th className="px-4 py-3 text-start font-medium">الحالة</th>
                  <th className="px-4 py-3 text-start font-medium">آخر تحديث</th>
                  <th className="px-4 py-3 text-start font-medium">إجراءات</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {data.items.map((item) => (
                  <tr key={item.id}>
                    <td className="px-4 py-3 font-medium">{item.title}</td>
                    <td className="px-4 py-3">
                      <StatusBadge label={labelOrRaw(contentStatusLabel, item.status)} tone={statusTone(item.status)} />
                    </td>
                    <td className="px-4 py-3 text-muted">{formatBaghdadDateTime(item.updatedAt)}</td>
                    <td className="px-4 py-3">
                      <div className="flex flex-wrap gap-2">
                        <Link href={`/admin/articles/${item.id}`} className="text-accent hover:underline">
                          تعديل
                        </Link>
                        {item.status !== "Published" ? (
                          <button type="button" className="text-accent hover:underline" onClick={() => setConfirm({ id: item.id, kind: "publish", title: item.title })}>
                            نشر
                          </button>
                        ) : null}
                        {item.status !== "Archived" ? (
                          <button type="button" className="hover:underline" onClick={() => setConfirm({ id: item.id, kind: "archive", title: item.title })}>
                            أرشفة
                          </button>
                        ) : null}
                      </div>
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
        open={confirm !== null}
        title={confirm?.kind === "publish" ? `نشر «${confirm.title}»؟` : confirm ? `أرشفة «${confirm.title}»؟` : ""}
        confirmLabel="تأكيد"
        pending={pending}
        onClose={() => !pending && setConfirm(null)}
        onConfirm={runConfirm}
      />
    </>
  );
}
