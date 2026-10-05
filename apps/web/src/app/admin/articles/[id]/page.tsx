"use client";

import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { FormEvent, useEffect, useState } from "react";
import { ConfirmDialog } from "@/components/admin/ConfirmDialog";
import { ForbiddenState } from "@/components/admin/ForbiddenState";
import { ErrorState, Field, LoadingState, PageHeader } from "@/components/admin/PageHeader";
import { StatusBadge } from "@/components/admin/StatusBadge";
import { useToast } from "@/components/admin/ToastProvider";
import { adminJson, errorMessage, isForbidden, isNotFound, isUnauthorized } from "@/lib/admin/http";
import { contentStatusLabel, labelOrRaw, statusTone } from "@/lib/admin/labels";
import { inputClassName, primaryButtonClassName, secondaryButtonClassName } from "@/lib/admin/ui";
import type { AdminArticleDetail } from "@/lib/admin/types";
import { formatBaghdadDateTime } from "@/lib/format";
import { safeInternalPath } from "@/lib/safe-path";

export default function ArticleEditorPage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const toast = useToast();
  const [article, setArticle] = useState<AdminArticleDetail | null | undefined>(undefined);
  const [title, setTitle] = useState("");
  const [slug, setSlug] = useState("");
  const [excerpt, setExcerpt] = useState("");
  const [content, setContent] = useState("");
  const [coverImage, setCoverImage] = useState("");
  const [error, setError] = useState("");
  const [forbidden, setForbidden] = useState(false);
  const [pending, setPending] = useState(false);
  const [confirm, setConfirm] = useState<"publish" | "archive" | null>(null);

  async function load() {
    try {
      const data = await adminJson<AdminArticleDetail>(`/api/admin/articles/${params.id}`, "تعذر تحميل المقال.");
      setArticle(data);
      setTitle(data.title);
      setSlug(data.slug);
      setExcerpt(data.excerpt);
      setContent(data.content);
      setCoverImage(data.coverImage ?? "");
    } catch (caught) {
      if (isUnauthorized(caught)) {
        router.replace(`/login?from=${encodeURIComponent(safeInternalPath(`/admin/articles/${params.id}`))}`);
        return;
      }
      if (isForbidden(caught)) {
        setForbidden(true);
        return;
      }
      if (isNotFound(caught)) {
        setArticle(null);
        return;
      }
      setError(errorMessage(caught, "تعذر تحميل المقال."));
    }
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [params.id]);

  async function onSave(event: FormEvent) {
    event.preventDefault();
    setPending(true);
    setError("");
    try {
      const saved = await adminJson<AdminArticleDetail>(`/api/admin/articles/${params.id}`, "تعذر حفظ المقال.", {
        method: "PUT",
        body: JSON.stringify({
          title,
          slug,
          excerpt,
          content,
          coverImage: coverImage.trim() || null
        })
      });
      setArticle(saved);
      toast.show("تم حفظ المقال.");
    } catch (caught) {
      setError(errorMessage(caught, "تعذر حفظ المقال."));
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
      const saved = await adminJson<AdminArticleDetail>(`/api/admin/articles/${params.id}/${confirm}`, "تعذر تحديث المقال.", {
        method: "POST"
      });
      setArticle(saved);
      setConfirm(null);
      toast.show("تم تحديث المقال.");
    } catch (caught) {
      setError(errorMessage(caught, "تعذر تحديث المقال."));
    } finally {
      setPending(false);
    }
  }

  if (forbidden) {
    return <ForbiddenState />;
  }

  if (article === undefined) {
    return (
      <>
        <PageHeader title="محرر المقال" />
        <LoadingState />
      </>
    );
  }

  if (article === null) {
    return (
      <>
        <PageHeader title="محرر المقال" />
        <p className="text-sm text-muted">المقال غير موجود.</p>
      </>
    );
  }

  return (
    <>
      <PageHeader
        title={article.title}
        description={`آخر تحديث ${formatBaghdadDateTime(article.updatedAt)}`}
        actions={
          <Link href="/admin/articles" className={secondaryButtonClassName}>
            العودة للقائمة
          </Link>
        }
      />
      <StatusBadge label={labelOrRaw(contentStatusLabel, article.status)} tone={statusTone(article.status)} />
      {error ? <div className="mt-4"><ErrorState message={error} /></div> : null}
      <form onSubmit={onSave} className="mt-6 grid gap-4">
        <Field label="العنوان">
          <input className={inputClassName} value={title} onChange={(event) => setTitle(event.target.value)} required />
        </Field>
        <Field label="المسار">
          <input className={inputClassName} value={slug} onChange={(event) => setSlug(event.target.value)} required />
        </Field>
        <Field label="المقتطف">
          <input className={inputClassName} value={excerpt} onChange={(event) => setExcerpt(event.target.value)} required />
        </Field>
        <Field label="صورة الغلاف (رابط)">
          <input className={inputClassName} value={coverImage} onChange={(event) => setCoverImage(event.target.value)} />
        </Field>
        <Field label="المحتوى (نص أو ماركداون)">
          <textarea className={inputClassName} rows={16} value={content} onChange={(event) => setContent(event.target.value)} required />
        </Field>
        <div className="flex flex-wrap gap-2">
          <button type="submit" className={primaryButtonClassName} disabled={pending}>
            حفظ
          </button>
          {article.status !== "Published" ? (
            <button type="button" className={secondaryButtonClassName} onClick={() => setConfirm("publish")}>
              نشر
            </button>
          ) : null}
          {article.status !== "Archived" ? (
            <button type="button" className={secondaryButtonClassName} onClick={() => setConfirm("archive")}>
              أرشفة
            </button>
          ) : null}
        </div>
      </form>
      <ConfirmDialog
        open={confirm !== null}
        title={confirm === "publish" ? "نشر المقال؟" : "أرشفة المقال؟"}
        confirmLabel="تأكيد"
        pending={pending}
        onClose={() => !pending && setConfirm(null)}
        onConfirm={runStatus}
      />
    </>
  );
}
