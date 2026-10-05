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
import { courseLevelLabel, courseStatusLabel, featuredLabel, labelOrRaw, statusTone } from "@/lib/admin/labels";
import { readPage, readPageSize, withListQuery } from "@/lib/admin/query";
import { inputClassName, primaryButtonClassName } from "@/lib/admin/ui";
import type { AdminCourseDetail, AdminCourseSummary, SaveCourseRequest } from "@/lib/admin/types";
import type { Paged } from "@/lib/api/public-types";
import { formatBaghdadDateTime, formatIqd } from "@/lib/format";
import { safeInternalPath } from "@/lib/safe-path";

export default function CoursesPage() {
  return (
    <Suspense fallback={<LoadingState />}>
      <CoursesContent />
    </Suspense>
  );
}

function CoursesContent() {
  const router = useRouter();
  const toast = useToast();
  const searchParams = useSearchParams();
  const page = readPage(searchParams.get("page"));
  const pageSize = readPageSize(searchParams.get("pageSize"));
  const search = searchParams.get("search") ?? "";
  const status = searchParams.get("status") ?? "";
  const [data, setData] = useState<Paged<AdminCourseSummary> | null>(null);
  const [error, setError] = useState("");
  const [forbidden, setForbidden] = useState(false);
  const [creating, setCreating] = useState(false);
  const [pending, setPending] = useState(false);
  const [confirm, setConfirm] = useState<{ id: string; kind: "publish" | "unpublish" | "archive"; title: string } | null>(null);
  const [form, setForm] = useState<SaveCourseRequest>({
    title: "",
    slug: "",
    shortDescription: "",
    description: "",
    priceIQD: 0,
    thumbnailUrl: null,
    trailerUrl: null,
    level: "Beginner",
    accessType: "Lifetime",
    accessDurationDays: null,
    isFeatured: false
  });

  async function load() {
    try {
      setData(
        await adminJson<Paged<AdminCourseSummary>>(
          withListQuery("/api/admin/courses", { page, pageSize, search, status }),
          "تعذر تحميل الدورات."
        )
      );
    } catch (caught) {
      if (isUnauthorized(caught)) {
        router.replace(`/login?from=${encodeURIComponent(safeInternalPath("/admin/courses"))}`);
        return;
      }
      if (isForbidden(caught)) {
        setForbidden(true);
        return;
      }
      setError(errorMessage(caught, "تعذر تحميل الدورات."));
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
      const created = await adminJson<AdminCourseDetail>("/api/admin/courses", "تعذر إنشاء الدورة.", {
        method: "POST",
        body: JSON.stringify({
          ...form,
          thumbnailUrl: form.thumbnailUrl || null,
          trailerUrl: form.trailerUrl || null,
          accessDurationDays: form.accessType === "LimitedDuration" ? form.accessDurationDays : null
        })
      });
      toast.show("تم إنشاء الدورة.");
      router.push(`/admin/courses/${created.id}`);
    } catch (caught) {
      setError(errorMessage(caught, "تعذر إنشاء الدورة."));
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
      await adminJson(`/api/admin/courses/${confirm.id}/${confirm.kind}`, "تعذر تحديث الدورة.", { method: "POST" });
      toast.show("تم تحديث حالة الدورة.");
      setConfirm(null);
      await load();
    } catch (caught) {
      setError(errorMessage(caught, "تعذر تحديث الدورة."));
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
        title="الدورات"
        description="إدارة الدورات والنشر والأرشفة."
        actions={
          <button type="button" className={primaryButtonClassName} onClick={() => setCreating((value) => !value)}>
            {creating ? "إغلاق نموذج الإنشاء" : "إنشاء دورة"}
          </button>
        }
      />
      {creating ? (
        <form onSubmit={onCreate} className="mb-8 grid gap-4 border border-border bg-surface p-5 sm:grid-cols-2">
          <Field label="العنوان">
            <input
              className={inputClassName}
              value={form.title}
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  title: event.target.value,
                  slug: current.slug || slugify(event.target.value)
                }))
              }
              required
            />
          </Field>
          <Field label="المسار">
            <input className={inputClassName} value={form.slug} onChange={(event) => setForm((current) => ({ ...current, slug: event.target.value }))} required />
          </Field>
          <Field label="الوصف المختصر">
            <input className={inputClassName} value={form.shortDescription} onChange={(event) => setForm((current) => ({ ...current, shortDescription: event.target.value }))} required />
          </Field>
          <Field label="السعر (د.ع)">
            <input
              className={inputClassName}
              type="number"
              min={0}
              value={form.priceIQD}
              onChange={(event) => setForm((current) => ({ ...current, priceIQD: Number(event.target.value) }))}
            />
          </Field>
          <div className="sm:col-span-2">
            <Field label="الوصف">
              <textarea className={inputClassName} rows={4} value={form.description} onChange={(event) => setForm((current) => ({ ...current, description: event.target.value }))} required />
            </Field>
          </div>
          <button type="submit" className={primaryButtonClassName} disabled={pending}>
            {pending ? "جاري الإنشاء..." : "إنشاء والانتقال للمحرر"}
          </button>
        </form>
      ) : null}
      <FilterBar pathname="/admin/courses" values={{ pageSize: String(pageSize) }}>
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
      {data && data.items.length === 0 ? <EmptyState title="لا توجد دورات" /> : null}
      {data && data.items.length > 0 ? (
        <>
          <AdminTable>
            <table className="min-w-full text-sm">
              <thead className="bg-surface-warm">
                <tr>
                  <th className="px-4 py-3 text-start font-medium">العنوان</th>
                  <th className="px-4 py-3 text-start font-medium">الحالة</th>
                  <th className="px-4 py-3 text-start font-medium">السعر</th>
                  <th className="px-4 py-3 text-start font-medium">أقسام / دروس</th>
                  <th className="px-4 py-3 text-start font-medium">مميزة</th>
                  <th className="px-4 py-3 text-start font-medium">آخر تحديث</th>
                  <th className="px-4 py-3 text-start font-medium">إجراءات</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {data.items.map((item) => (
                  <tr key={item.id}>
                    <td className="px-4 py-3">
                      <p className="font-medium">{item.title}</p>
                      <p className="text-xs text-muted">{courseLevelLabel(item.level)}</p>
                    </td>
                    <td className="px-4 py-3">
                      <StatusBadge label={labelOrRaw(courseStatusLabel, item.status)} tone={statusTone(item.status)} />
                    </td>
                    <td className="px-4 py-3">{formatIqd(item.priceIQD)}</td>
                    <td className="px-4 py-3">
                      {item.sectionCount} / {item.lessonCount}
                    </td>
                    <td className="px-4 py-3">{featuredLabel(item.isFeatured)}</td>
                    <td className="px-4 py-3 text-muted">{formatBaghdadDateTime(item.updatedAt)}</td>
                    <td className="px-4 py-3">
                      <div className="flex flex-wrap gap-2">
                        <Link href={`/admin/courses/${item.id}`} className="text-accent hover:underline">
                          تعديل
                        </Link>
                        {item.status !== "Published" ? (
                          <button type="button" className="text-accent hover:underline" onClick={() => setConfirm({ id: item.id, kind: "publish", title: item.title })}>
                            نشر
                          </button>
                        ) : (
                          <button type="button" className="hover:underline" onClick={() => setConfirm({ id: item.id, kind: "unpublish", title: item.title })}>
                            إلغاء النشر
                          </button>
                        )}
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
        title={
          confirm?.kind === "publish"
            ? `نشر «${confirm.title}»؟`
            : confirm?.kind === "unpublish"
              ? `إلغاء نشر «${confirm.title}»؟`
              : confirm
                ? `أرشفة «${confirm.title}»؟`
                : ""
        }
        confirmLabel="تأكيد"
        pending={pending}
        onClose={() => !pending && setConfirm(null)}
        onConfirm={runConfirm}
      />
    </>
  );
}

function slugify(value: string): string {
  return value
    .trim()
    .toLowerCase()
    .replace(/\s+/g, "-")
    .replace(/[^\p{L}\p{N}-]+/gu, "");
}
