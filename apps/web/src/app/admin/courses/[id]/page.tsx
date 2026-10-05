"use client";

import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { FormEvent, useEffect, useState } from "react";
import { ConfirmDialog } from "@/components/admin/ConfirmDialog";
import { CourseCurriculumEditor } from "@/components/admin/CourseCurriculumEditor";
import { ForbiddenState } from "@/components/admin/ForbiddenState";
import { ErrorState, Field, LoadingState, PageHeader } from "@/components/admin/PageHeader";
import { StatusBadge } from "@/components/admin/StatusBadge";
import { useToast } from "@/components/admin/ToastProvider";
import { adminJson, errorMessage, isForbidden, isNotFound, isUnauthorized } from "@/lib/admin/http";
import { accessTypeLabel, courseLevelLabel, courseStatusLabel, featuredLabel, labelOrRaw, statusTone } from "@/lib/admin/labels";
import { inputClassName, primaryButtonClassName, secondaryButtonClassName } from "@/lib/admin/ui";
import type { AdminCourseDetail, SaveCourseRequest } from "@/lib/admin/types";
import { formatBaghdadDateTime } from "@/lib/format";
import { safeInternalPath } from "@/lib/safe-path";

export default function CourseEditorPage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const toast = useToast();
  const [course, setCourse] = useState<AdminCourseDetail | null | undefined>(undefined);
  const [form, setForm] = useState<SaveCourseRequest | null>(null);
  const [error, setError] = useState("");
  const [forbidden, setForbidden] = useState(false);
  const [pending, setPending] = useState(false);
  const [confirm, setConfirm] = useState<"publish" | "unpublish" | "archive" | null>(null);

  async function load() {
    try {
      const data = await adminJson<AdminCourseDetail>(`/api/admin/courses/${params.id}`, "تعذر تحميل الدورة.");
      setCourse(data);
      setForm(toForm(data));
    } catch (caught) {
      if (isUnauthorized(caught)) {
        router.replace(`/login?from=${encodeURIComponent(safeInternalPath(`/admin/courses/${params.id}`))}`);
        return;
      }
      if (isForbidden(caught)) {
        setForbidden(true);
        return;
      }
      if (isNotFound(caught)) {
        setCourse(null);
        return;
      }
      setError(errorMessage(caught, "تعذر تحميل الدورة."));
    }
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [params.id]);

  async function onSave(event: FormEvent) {
    event.preventDefault();
    if (!form) {
      return;
    }

    setPending(true);
    setError("");
    try {
      const saved = await adminJson<AdminCourseDetail>(`/api/admin/courses/${params.id}`, "تعذر حفظ الدورة.", {
        method: "PUT",
        body: JSON.stringify({
          ...form,
          thumbnailUrl: form.thumbnailUrl || null,
          trailerUrl: form.trailerUrl || null,
          accessDurationDays: form.accessType === "LimitedDuration" ? form.accessDurationDays : null
        })
      });
      setCourse(saved);
      setForm(toForm(saved));
      toast.show("تم حفظ الدورة.");
    } catch (caught) {
      setError(errorMessage(caught, "تعذر حفظ الدورة."));
    } finally {
      setPending(false);
    }
  }

  async function runStatus() {
    if (!confirm) {
      return;
    }

    setPending(true);
    setError("");
    try {
      const saved = await adminJson<AdminCourseDetail>(`/api/admin/courses/${params.id}/${confirm}`, "تعذر تحديث الحالة.", {
        method: "POST"
      });
      setCourse(saved);
      setForm(toForm(saved));
      setConfirm(null);
      toast.show("تم تحديث حالة الدورة.");
    } catch (caught) {
      setError(errorMessage(caught, "تعذر تحديث الحالة."));
    } finally {
      setPending(false);
    }
  }

  if (forbidden) {
    return <ForbiddenState />;
  }

  if (course === undefined || !form) {
    return (
      <>
        <PageHeader title="محرر الدورة" />
        {error ? <ErrorState message={error} /> : <LoadingState />}
      </>
    );
  }

  if (course === null) {
    return (
      <>
        <PageHeader title="محرر الدورة" />
        <p className="text-sm text-muted">الدورة غير موجودة.</p>
      </>
    );
  }

  return (
    <>
      <PageHeader
        title={course.title}
        description={`آخر تحديث ${formatBaghdadDateTime(course.updatedAt)}`}
        actions={
          <Link href="/admin/courses" className={secondaryButtonClassName}>
            العودة للقائمة
          </Link>
        }
      />
      <div className="mb-6 flex flex-wrap items-center gap-3">
        <StatusBadge label={labelOrRaw(courseStatusLabel, course.status)} tone={statusTone(course.status)} />
        <span className="text-sm text-muted">{featuredLabel(course.isFeatured)}</span>
        <span className="text-sm text-muted">{courseLevelLabel(course.level)}</span>
        <span className="text-sm text-muted">{accessTypeLabel(course.accessType)}</span>
      </div>
      {error ? <div className="mb-4"><ErrorState message={error} /></div> : null}
      <form onSubmit={onSave} className="clinic-card grid gap-4 p-5 sm:grid-cols-2">
        <Field label="العنوان">
          <input className={inputClassName} value={form.title} onChange={(event) => setForm({ ...form, title: event.target.value })} required />
        </Field>
        <Field label="المسار">
          <input className={inputClassName} value={form.slug} onChange={(event) => setForm({ ...form, slug: event.target.value })} required />
        </Field>
        <div className="sm:col-span-2">
          <Field label="الوصف المختصر">
            <input className={inputClassName} value={form.shortDescription} onChange={(event) => setForm({ ...form, shortDescription: event.target.value })} required />
          </Field>
        </div>
        <div className="sm:col-span-2">
          <Field label="الوصف">
            <textarea className={inputClassName} rows={6} value={form.description} onChange={(event) => setForm({ ...form, description: event.target.value })} required />
          </Field>
        </div>
        <Field label="السعر (د.ع)">
          <input className={inputClassName} type="number" min={0} value={form.priceIQD} onChange={(event) => setForm({ ...form, priceIQD: Number(event.target.value) })} />
        </Field>
        <Field label="المستوى">
          <select className={inputClassName} value={form.level} onChange={(event) => setForm({ ...form, level: event.target.value })}>
            <option value="Beginner">مبتدئ</option>
            <option value="Intermediate">متوسط</option>
            <option value="Advanced">متقدم</option>
          </select>
        </Field>
        <Field label="نوع الوصول">
          <select
            className={inputClassName}
            value={form.accessType}
            onChange={(event) =>
              setForm({
                ...form,
                accessType: event.target.value,
                accessDurationDays: event.target.value === "Lifetime" ? null : form.accessDurationDays ?? 30
              })
            }
          >
            <option value="Lifetime">مدى الحياة</option>
            <option value="LimitedDuration">مدة محدودة</option>
          </select>
        </Field>
        {form.accessType === "LimitedDuration" ? (
          <Field label="مدة الوصول بالأيام">
            <input
              className={inputClassName}
              type="number"
              min={1}
              value={form.accessDurationDays ?? 30}
              onChange={(event) => setForm({ ...form, accessDurationDays: Number(event.target.value) })}
            />
          </Field>
        ) : null}
        <Field label="صورة الغلاف (رابط)">
          <input className={inputClassName} value={form.thumbnailUrl ?? ""} onChange={(event) => setForm({ ...form, thumbnailUrl: event.target.value })} />
        </Field>
        <Field label="رابط المقدمة">
          <input className={inputClassName} value={form.trailerUrl ?? ""} onChange={(event) => setForm({ ...form, trailerUrl: event.target.value })} />
        </Field>
        <label className="flex items-center gap-2 text-sm">
          <input type="checkbox" checked={form.isFeatured} onChange={(event) => setForm({ ...form, isFeatured: event.target.checked })} />
          دورة مميزة
        </label>
        <div className="sm:col-span-2 flex flex-wrap gap-2">
          <button type="submit" className={primaryButtonClassName} disabled={pending}>
            {pending ? "جاري الحفظ..." : "حفظ الدورة"}
          </button>
          {course.status !== "Published" ? (
            <button type="button" className={secondaryButtonClassName} onClick={() => setConfirm("publish")}>
              نشر
            </button>
          ) : (
            <button type="button" className={secondaryButtonClassName} onClick={() => setConfirm("unpublish")}>
              إلغاء النشر
            </button>
          )}
          {course.status !== "Archived" ? (
            <button type="button" className={secondaryButtonClassName} onClick={() => setConfirm("archive")}>
              أرشفة
            </button>
          ) : null}
        </div>
      </form>
      <CourseCurriculumEditor course={course} onReload={load} />
      <ConfirmDialog
        open={confirm !== null}
        title={confirm === "publish" ? "نشر الدورة؟" : confirm === "unpublish" ? "إلغاء نشر الدورة؟" : "أرشفة الدورة؟"}
        confirmLabel="تأكيد"
        pending={pending}
        onClose={() => !pending && setConfirm(null)}
        onConfirm={runStatus}
      />
    </>
  );
}

function toForm(course: AdminCourseDetail): SaveCourseRequest {
  return {
    title: course.title,
    slug: course.slug,
    shortDescription: course.shortDescription,
    description: course.description,
    priceIQD: course.priceIQD,
    thumbnailUrl: course.thumbnailUrl,
    trailerUrl: course.trailerUrl,
    level: course.level,
    accessType: course.accessType,
    accessDurationDays: course.accessDurationDays,
    isFeatured: course.isFeatured
  };
}
