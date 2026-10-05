"use client";

import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { ConfirmDialog } from "@/components/admin/ConfirmDialog";
import { ForbiddenState } from "@/components/admin/ForbiddenState";
import { ErrorState, Field, LoadingState, PageHeader } from "@/components/admin/PageHeader";
import { StatusBadge } from "@/components/admin/StatusBadge";
import { useToast } from "@/components/admin/ToastProvider";
import { EmptyState } from "@/components/shared/EmptyState";
import { adminJson, adminVoid, errorMessage, isForbidden, isUnauthorized } from "@/lib/admin/http";
import { boolActiveLabel } from "@/lib/admin/labels";
import { inputClassName, primaryButtonClassName, secondaryButtonClassName } from "@/lib/admin/ui";
import type { AdminFaq, ReorderItem } from "@/lib/admin/types";
import { safeInternalPath } from "@/lib/safe-path";

export default function FaqAdminPage() {
  const router = useRouter();
  const toast = useToast();
  const [items, setItems] = useState<AdminFaq[] | null>(null);
  const [error, setError] = useState("");
  const [forbidden, setForbidden] = useState(false);
  const [pending, setPending] = useState(false);
  const [question, setQuestion] = useState("");
  const [answer, setAnswer] = useState("");
  const [showOnHome, setShowOnHome] = useState(false);
  const [editing, setEditing] = useState<AdminFaq | null>(null);
  const [confirm, setConfirm] = useState<{ id: string; kind: "activate" | "deactivate"; question: string } | null>(null);

  async function load() {
    try {
      const list = await adminJson<AdminFaq[]>("/api/admin/faq", "تعذر تحميل الأسئلة الشائعة.");
      setItems([...list].sort((a, b) => a.sortOrder - b.sortOrder));
    } catch (caught) {
      if (isUnauthorized(caught)) {
        router.replace(`/login?from=${encodeURIComponent(safeInternalPath("/admin/faq"))}`);
        return;
      }
      if (isForbidden(caught)) {
        setForbidden(true);
        return;
      }
      setError(errorMessage(caught, "تعذر تحميل الأسئلة الشائعة."));
    }
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  async function onCreate(event: FormEvent) {
    event.preventDefault();
    setPending(true);
    setError("");
    try {
      await adminJson("/api/admin/faq", "تعذر إضافة السؤال.", {
        method: "POST",
        body: JSON.stringify({ question, answer, showOnHome })
      });
      setQuestion("");
      setAnswer("");
      setShowOnHome(false);
      toast.show("تمت إضافة السؤال.");
      await load();
    } catch (caught) {
      setError(errorMessage(caught, "تعذر إضافة السؤال."));
    } finally {
      setPending(false);
    }
  }

  async function saveEdit() {
    if (!editing) {
      return;
    }

    setPending(true);
    setError("");
    try {
      await adminJson(`/api/admin/faq/${editing.id}`, "تعذر حفظ السؤال.", {
        method: "PUT",
        body: JSON.stringify({
          question: editing.question,
          answer: editing.answer,
          showOnHome: editing.showOnHome,
          sortOrder: editing.sortOrder
        })
      });
      toast.show("تم حفظ السؤال.");
      setEditing(null);
      await load();
    } catch (caught) {
      setError(errorMessage(caught, "تعذر حفظ السؤال."));
    } finally {
      setPending(false);
    }
  }

  async function reorder(next: AdminFaq[]) {
    setPending(true);
    setError("");
    try {
      const payload: { items: ReorderItem[] } = {
        items: next.map((item, index) => ({ id: item.id, sortOrder: index + 1 }))
      };
      await adminVoid("/api/admin/faq/reorder", "تعذر إعادة الترتيب.", {
        method: "POST",
        body: JSON.stringify(payload)
      });
      await load();
    } catch (caught) {
      setError(errorMessage(caught, "تعذر إعادة الترتيب."));
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
      await adminJson(`/api/admin/faq/${confirm.id}/${confirm.kind}`, "تعذر تحديث السؤال.", { method: "POST" });
      toast.show("تم تحديث السؤال.");
      setConfirm(null);
      await load();
    } catch (caught) {
      setError(errorMessage(caught, "تعذر تحديث السؤال."));
    } finally {
      setPending(false);
    }
  }

  if (forbidden) {
    return <ForbiddenState />;
  }

  return (
    <>
      <PageHeader title="الأسئلة الشائعة" description="إضافة وتعديل وترتيب الأسئلة." />
      {error ? <ErrorState message={error} /> : null}
      <form onSubmit={onCreate} className="mb-8 grid gap-3 border border-border bg-surface p-5">
        <Field label="السؤال">
          <input className={inputClassName} value={question} onChange={(event) => setQuestion(event.target.value)} required />
        </Field>
        <Field label="الإجابة">
          <textarea className={inputClassName} rows={4} value={answer} onChange={(event) => setAnswer(event.target.value)} required />
        </Field>
        <label className="flex items-center gap-2 text-sm">
          <input type="checkbox" checked={showOnHome} onChange={(event) => setShowOnHome(event.target.checked)} />
          عرض في الصفحة الرئيسية
        </label>
        <button type="submit" className={primaryButtonClassName} disabled={pending}>
          إضافة
        </button>
      </form>
      {!items && !error ? <LoadingState /> : null}
      {items && items.length === 0 ? <EmptyState title="لا توجد أسئلة" /> : null}
      {items && items.length > 0 ? (
        <ul className="space-y-4">
          {items.map((item, index) => (
            <li key={item.id} className="border border-border bg-surface p-4">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <p className="font-medium">{item.question}</p>
                  <p className="mt-2 text-sm leading-7 text-muted whitespace-pre-wrap">{item.answer}</p>
                </div>
                <StatusBadge label={boolActiveLabel(item.isActive)} tone={item.isActive ? "success" : "neutral"} />
              </div>
              {editing?.id === item.id ? (
                <div className="mt-4 grid gap-3">
                  <Field label="السؤال">
                    <input className={inputClassName} value={editing.question} onChange={(event) => setEditing({ ...editing, question: event.target.value })} />
                  </Field>
                  <Field label="الإجابة">
                    <textarea className={inputClassName} rows={4} value={editing.answer} onChange={(event) => setEditing({ ...editing, answer: event.target.value })} />
                  </Field>
                  <label className="flex items-center gap-2 text-sm">
                    <input type="checkbox" checked={editing.showOnHome} onChange={(event) => setEditing({ ...editing, showOnHome: event.target.checked })} />
                    عرض في الصفحة الرئيسية
                  </label>
                  <div className="flex gap-2">
                    <button type="button" className={primaryButtonClassName} disabled={pending} onClick={saveEdit}>
                      حفظ
                    </button>
                    <button type="button" className={secondaryButtonClassName} onClick={() => setEditing(null)}>
                      إلغاء
                    </button>
                  </div>
                </div>
              ) : (
                <div className="mt-4 flex flex-wrap gap-2">
                  <button type="button" className={secondaryButtonClassName} disabled={pending || index === 0} onClick={() => reorder(move(items, index, -1))}>
                    أعلى
                  </button>
                  <button type="button" className={secondaryButtonClassName} disabled={pending || index === items.length - 1} onClick={() => reorder(move(items, index, 1))}>
                    أسفل
                  </button>
                  <button type="button" className={secondaryButtonClassName} onClick={() => setEditing(item)}>
                    تعديل
                  </button>
                  {item.isActive ? (
                    <button type="button" className={secondaryButtonClassName} onClick={() => setConfirm({ id: item.id, kind: "deactivate", question: item.question })}>
                      إيقاف
                    </button>
                  ) : (
                    <button type="button" className={secondaryButtonClassName} onClick={() => setConfirm({ id: item.id, kind: "activate", question: item.question })}>
                      تفعيل
                    </button>
                  )}
                </div>
              )}
            </li>
          ))}
        </ul>
      ) : null}
      <ConfirmDialog
        open={confirm !== null}
        title={confirm?.kind === "activate" ? "تفعيل السؤال؟" : "إيقاف السؤال؟"}
        confirmLabel="تأكيد"
        pending={pending}
        onClose={() => !pending && setConfirm(null)}
        onConfirm={runConfirm}
      />
    </>
  );
}

function move<T>(items: T[], index: number, delta: number): T[] {
  const next = [...items];
  const target = index + delta;
  if (target < 0 || target >= next.length) {
    return next;
  }

  const current = next[index];
  next[index] = next[target];
  next[target] = current;
  return next;
}
