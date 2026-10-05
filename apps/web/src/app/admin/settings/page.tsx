"use client";

import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { ConfirmDialog } from "@/components/admin/ConfirmDialog";
import { ForbiddenState } from "@/components/admin/ForbiddenState";
import { ErrorState, Field, LoadingState, PageHeader } from "@/components/admin/PageHeader";
import { StatusBadge } from "@/components/admin/StatusBadge";
import { useToast } from "@/components/admin/ToastProvider";
import { adminJson, adminVoid, errorMessage, isForbidden, isUnauthorized } from "@/lib/admin/http";
import { boolActiveLabel } from "@/lib/admin/labels";
import { canManageContent, isAdmin } from "@/lib/admin/roles";
import { inputClassName, primaryButtonClassName, secondaryButtonClassName } from "@/lib/admin/ui";
import type {
  AdminExpertise,
  AdminSiteSettings,
  AdminStatistic,
  AdminTestimonial,
  ReorderItem
} from "@/lib/admin/types";
import { getCurrentUser } from "@/lib/auth/session";
import { safeInternalPath } from "@/lib/safe-path";

export default function SettingsPage() {
  const router = useRouter();
  const toast = useToast();
  const [roles, setRoles] = useState<string[] | null>(null);
  const [settings, setSettings] = useState<AdminSiteSettings | null>(null);
  const [error, setError] = useState("");
  const [forbidden, setForbidden] = useState(false);
  const [pending, setPending] = useState(false);
  const [tab, setTab] = useState<"identity" | "payment" | "home">("identity");

  async function load() {
    try {
      const session = await getCurrentUser();
      if (!session) {
        router.replace(`/login?from=${encodeURIComponent(safeInternalPath("/admin/settings"))}`);
        return;
      }

      setRoles(session.roles);
      setSettings(await adminJson<AdminSiteSettings>("/api/admin/settings", "تعذر تحميل الإعدادات."));
    } catch (caught) {
      if (isUnauthorized(caught)) {
        router.replace(`/login?from=${encodeURIComponent(safeInternalPath("/admin/settings"))}`);
        return;
      }
      if (isForbidden(caught)) {
        setForbidden(true);
        return;
      }
      setError(errorMessage(caught, "تعذر تحميل الإعدادات."));
    }
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  async function saveContent(event: FormEvent) {
    event.preventDefault();
    if (!settings) {
      return;
    }

    setPending(true);
    setError("");
    try {
      const saved = await adminJson<AdminSiteSettings>("/api/admin/settings/content", "تعذر حفظ إعدادات المحتوى.", {
        method: "PUT",
        body: JSON.stringify({
          brandName: settings.brandName,
          brandNameEnglish: settings.brandNameEnglish,
          publicPhone: settings.publicPhone,
          publicWhatsApp: settings.publicWhatsApp,
          publicEmail: settings.publicEmail,
          socialLinks: settings.socialLinks,
          footerText: settings.footerText,
          consultationInfo: settings.consultationInfo
        })
      });
      setSettings((current) => ({ ...current, ...saved }));
      toast.show("تم حفظ إعدادات المحتوى.");
    } catch (caught) {
      setError(errorMessage(caught, "تعذر حفظ إعدادات المحتوى."));
    } finally {
      setPending(false);
    }
  }

  async function savePayment(event: FormEvent) {
    event.preventDefault();
    if (!settings) {
      return;
    }

    setPending(true);
    setError("");
    try {
      const saved = await adminJson<AdminSiteSettings>("/api/admin/settings/payment", "تعذر حفظ إعدادات الدفع.", {
        method: "PUT",
        body: JSON.stringify({
          paymentMethods: settings.paymentMethods,
          transferInstructions: settings.transferInstructions,
          supportPhone: settings.supportPhone,
          supportWhatsApp: settings.supportWhatsApp
        })
      });
      setSettings((current) => ({ ...current, ...saved }));
      toast.show("تم حفظ إعدادات الدفع.");
    } catch (caught) {
      setError(errorMessage(caught, "تعذر حفظ إعدادات الدفع."));
    } finally {
      setPending(false);
    }
  }

  if (forbidden) {
    return <ForbiddenState />;
  }

  if (!settings || !roles) {
    return (
      <>
        <PageHeader title="الإعدادات" />
        {error ? <ErrorState message={error} /> : <LoadingState />}
      </>
    );
  }

  const contentEditable = canManageContent(roles);
  const paymentEditable = isAdmin(roles);

  const tabs: Array<{ id: typeof tab; label: string; hint: string }> = [
    { id: "identity", label: "الهوية والنصوص", hint: "اسم العيادة، التواصل، التذييل، معلومات الاستشارة" },
    { id: "payment", label: "الدفع والتحويل", hint: "طرق الدفع وتعليمات التحويل للدعم" },
    { id: "home", label: "عناصر الصفحة الرئيسية", hint: "مجالات الخبرة، الشهادات، الإحصاءات" }
  ];

  return (
    <>
      <PageHeader
        title="النصوص والإعدادات الظاهرة"
        description="كل ما يظهر للزائر ويتغير من اللوحة: الهوية، الدفع، وعناصر الصفحة الرئيسية. اختر التبويب المناسب."
      />
      {error ? (
        <div className="mb-4">
          <ErrorState message={error} />
        </div>
      ) : null}
      <div className="mb-6 flex flex-wrap gap-2" role="tablist" aria-label="تبويبات الإعدادات">
        {tabs.map((item) => {
          const active = tab === item.id;
          return (
            <button
              key={item.id}
              type="button"
              role="tab"
              aria-selected={active}
              className={`border px-4 py-2 text-sm ${active ? "border-accent bg-accent text-primary-foreground" : "border-border bg-surface text-muted hover:text-foreground"}`}
              onClick={() => setTab(item.id)}
            >
              {item.label}
            </button>
          );
        })}
      </div>
      <p className="mb-4 text-sm text-muted">{tabs.find((item) => item.id === tab)?.hint}</p>

      {tab === "identity" ? (
        <form onSubmit={saveContent} className="grid gap-4 border border-border bg-surface p-5">
          <h2 className="text-lg font-semibold">الهوية والنصوص الظاهرة</h2>
          <Field label="اسم العلامة">
            <input className={inputClassName} value={settings.brandName ?? ""} disabled={!contentEditable} onChange={(event) => setSettings({ ...settings, brandName: event.target.value })} />
          </Field>
          <Field label="الاسم بالإنجليزية">
            <input className={inputClassName} value={settings.brandNameEnglish ?? ""} disabled={!contentEditable} onChange={(event) => setSettings({ ...settings, brandNameEnglish: event.target.value })} />
          </Field>
          <Field label="الهاتف العام">
            <input className={inputClassName} value={settings.publicPhone ?? ""} disabled={!contentEditable} onChange={(event) => setSettings({ ...settings, publicPhone: event.target.value })} />
          </Field>
          <Field label="واتساب العام">
            <input className={inputClassName} value={settings.publicWhatsApp ?? ""} disabled={!contentEditable} onChange={(event) => setSettings({ ...settings, publicWhatsApp: event.target.value })} />
          </Field>
          <Field label="البريد العام">
            <input className={inputClassName} value={settings.publicEmail ?? ""} disabled={!contentEditable} onChange={(event) => setSettings({ ...settings, publicEmail: event.target.value })} />
          </Field>
          <Field label="روابط التواصل (JSON أو نص)">
            <textarea className={inputClassName} rows={3} value={settings.socialLinks ?? ""} disabled={!contentEditable} onChange={(event) => setSettings({ ...settings, socialLinks: event.target.value })} />
          </Field>
          <Field label="نص التذييل">
            <textarea className={inputClassName} rows={3} value={settings.footerText ?? ""} disabled={!contentEditable} onChange={(event) => setSettings({ ...settings, footerText: event.target.value })} />
          </Field>
          <Field label="معلومات الاستشارة">
            <textarea className={inputClassName} rows={4} value={settings.consultationInfo ?? ""} disabled={!contentEditable} onChange={(event) => setSettings({ ...settings, consultationInfo: event.target.value })} />
          </Field>
          {contentEditable ? (
            <button type="submit" className={primaryButtonClassName} disabled={pending}>
              حفظ الهوية والنصوص
            </button>
          ) : (
            <p className="text-sm text-muted">عرض فقط. التعديل متاح لمدير المنصة ومدير المحتوى.</p>
          )}
        </form>
      ) : null}

      {tab === "payment" ? (
        paymentEditable ? (
          <form onSubmit={savePayment} className="grid gap-4 border border-border bg-surface p-5">
            <h2 className="text-lg font-semibold">الدفع والتحويل</h2>
            <Field label="طرق الدفع">
              <textarea className={inputClassName} rows={3} value={settings.paymentMethods ?? ""} onChange={(event) => setSettings({ ...settings, paymentMethods: event.target.value })} />
            </Field>
            <Field label="تعليمات التحويل">
              <textarea className={inputClassName} rows={5} value={settings.transferInstructions ?? ""} onChange={(event) => setSettings({ ...settings, transferInstructions: event.target.value })} />
            </Field>
            <Field label="هاتف الدعم">
              <input className={inputClassName} value={settings.supportPhone ?? ""} onChange={(event) => setSettings({ ...settings, supportPhone: event.target.value })} />
            </Field>
            <Field label="واتساب الدعم">
              <input className={inputClassName} value={settings.supportWhatsApp ?? ""} onChange={(event) => setSettings({ ...settings, supportWhatsApp: event.target.value })} />
            </Field>
            <button type="submit" className={primaryButtonClassName} disabled={pending}>
              حفظ إعدادات الدفع
            </button>
          </form>
        ) : (
          <p className="text-sm text-muted">قسم الدفع متاح للمدير فقط.</p>
        )
      ) : null}

      {tab === "home" ? contentEditable ? <HomeContentLists /> : <p className="text-sm text-muted">عرض عناصر الصفحة الرئيسية متاح لمدير المحتوى.</p> : null}
    </>
  );
}

function HomeContentLists() {
  const toast = useToast();
  const [expertise, setExpertise] = useState<AdminExpertise[]>([]);
  const [testimonials, setTestimonials] = useState<AdminTestimonial[]>([]);
  const [statistics, setStatistics] = useState<AdminStatistic[]>([]);
  const [error, setError] = useState("");
  const [pending, setPending] = useState(false);
  const [expTitle, setExpTitle] = useState("");
  const [expDescription, setExpDescription] = useState("");
  const [testName, setTestName] = useState("");
  const [testTitle, setTestTitle] = useState("");
  const [testBody, setTestBody] = useState("");
  const [confirm, setConfirm] = useState<{ kind: string; id: string } | null>(null);

  async function loadLists() {
    try {
      const [exp, tes, stats] = await Promise.all([
        adminJson<AdminExpertise[]>("/api/admin/expertise", "تعذر تحميل مجالات الخبرة."),
        adminJson<AdminTestimonial[]>("/api/admin/testimonials", "تعذر تحميل الشهادات."),
        adminJson<AdminStatistic[]>("/api/admin/statistics", "تعذر تحميل الإحصاءات.")
      ]);
      setExpertise([...exp].sort((a, b) => a.sortOrder - b.sortOrder));
      setTestimonials([...tes].sort((a, b) => a.sortOrder - b.sortOrder));
      setStatistics([...stats].sort((a, b) => a.sortOrder - b.sortOrder));
    } catch (caught) {
      setError(errorMessage(caught, "تعذر تحميل عناصر الصفحة الرئيسية."));
    }
  }

  useEffect(() => {
    loadLists();
  }, []);

  async function addExpertise(event: FormEvent) {
    event.preventDefault();
    setPending(true);
    setError("");
    try {
      await adminJson("/api/admin/expertise", "تعذر الإضافة.", {
        method: "POST",
        body: JSON.stringify({ title: expTitle, description: expDescription })
      });
      setExpTitle("");
      setExpDescription("");
      toast.show("تمت إضافة مجال الخبرة.");
      await loadLists();
    } catch (caught) {
      setError(errorMessage(caught, "تعذر الإضافة."));
    } finally {
      setPending(false);
    }
  }

  async function addTestimonial(event: FormEvent) {
    event.preventDefault();
    setPending(true);
    setError("");
    try {
      await adminJson("/api/admin/testimonials", "تعذر الإضافة.", {
        method: "POST",
        body: JSON.stringify({ authorDisplayName: testName, authorTitle: testTitle || null, body: testBody })
      });
      setTestName("");
      setTestTitle("");
      setTestBody("");
      toast.show("تمت إضافة الشهادة.");
      await loadLists();
    } catch (caught) {
      setError(errorMessage(caught, "تعذر الإضافة."));
    } finally {
      setPending(false);
    }
  }

  async function reorderExpertise(next: AdminExpertise[]) {
    const payload: { items: ReorderItem[] } = { items: next.map((item, index) => ({ id: item.id, sortOrder: index + 1 })) };
    setPending(true);
    try {
      await adminVoid("/api/admin/expertise/reorder", "تعذر إعادة الترتيب.", { method: "POST", body: JSON.stringify(payload) });
      await loadLists();
    } catch (caught) {
      setError(errorMessage(caught, "تعذر إعادة الترتيب."));
    } finally {
      setPending(false);
    }
  }

  async function saveStatistic(item: AdminStatistic) {
    setPending(true);
    setError("");
    try {
      await adminJson(`/api/admin/statistics/${item.id}`, "تعذر حفظ الإحصاء.", {
        method: "PUT",
        body: JSON.stringify({
          label: item.label,
          displayValue: item.displayValue,
          isPlaceholder: item.isPlaceholder,
          sortOrder: item.sortOrder,
          isActive: item.isActive
        })
      });
      toast.show("تم حفظ الإحصاء.");
      await loadLists();
    } catch (caught) {
      setError(errorMessage(caught, "تعذر حفظ الإحصاء."));
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
      if (confirm.kind === "exp-activate" || confirm.kind === "exp-deactivate") {
        await adminJson(`/api/admin/expertise/${confirm.id}/${confirm.kind === "exp-activate" ? "activate" : "deactivate"}`, "تعذر التحديث.", { method: "POST" });
      } else {
        await adminJson(`/api/admin/testimonials/${confirm.id}/${confirm.kind === "tes-publish" ? "publish" : "unpublish"}`, "تعذر التحديث.", { method: "POST" });
      }
      toast.show("تم التحديث.");
      setConfirm(null);
      await loadLists();
    } catch (caught) {
      setError(errorMessage(caught, "تعذر التحديث."));
    } finally {
      setPending(false);
    }
  }

  return (
    <div className="mt-10 space-y-10">
      {error ? <ErrorState message={error} /> : null}
      <section className="border border-border bg-surface p-5">
        <h2 className="text-lg font-semibold">مجالات الخبرة</h2>
        <form onSubmit={addExpertise} className="mt-4 grid gap-3 sm:grid-cols-2">
          <input className={inputClassName} placeholder="العنوان" value={expTitle} onChange={(event) => setExpTitle(event.target.value)} required />
          <input className={inputClassName} placeholder="الوصف" value={expDescription} onChange={(event) => setExpDescription(event.target.value)} required />
          <button type="submit" className={secondaryButtonClassName} disabled={pending}>إضافة</button>
        </form>
        <ul className="mt-4 space-y-3">
          {expertise.map((item, index) => (
            <li key={item.id} className="flex flex-wrap items-center justify-between gap-3 border-t border-border pt-3">
              <div>
                <p className="font-medium">{item.title}</p>
                <p className="text-sm text-muted">{item.description}</p>
              </div>
              <div className="flex flex-wrap gap-2">
                <StatusBadge label={boolActiveLabel(item.isActive)} tone={item.isActive ? "success" : "neutral"} />
                <button type="button" className={secondaryButtonClassName} disabled={index === 0} onClick={() => reorderExpertise(move(expertise, index, -1))}>أعلى</button>
                <button type="button" className={secondaryButtonClassName} disabled={index === expertise.length - 1} onClick={() => reorderExpertise(move(expertise, index, 1))}>أسفل</button>
                <button type="button" className={secondaryButtonClassName} onClick={() => setConfirm({ kind: item.isActive ? "exp-deactivate" : "exp-activate", id: item.id })}>
                  {item.isActive ? "إيقاف" : "تفعيل"}
                </button>
              </div>
            </li>
          ))}
        </ul>
      </section>
      <section className="border border-border bg-surface p-5">
        <h2 className="text-lg font-semibold">الشهادات</h2>
        <form onSubmit={addTestimonial} className="mt-4 grid gap-3">
          <input className={inputClassName} placeholder="الاسم الظاهر" value={testName} onChange={(event) => setTestName(event.target.value)} required />
          <input className={inputClassName} placeholder="الصفة" value={testTitle} onChange={(event) => setTestTitle(event.target.value)} />
          <textarea className={inputClassName} placeholder="النص" rows={3} value={testBody} onChange={(event) => setTestBody(event.target.value)} required />
          <button type="submit" className={secondaryButtonClassName} disabled={pending}>إضافة شهادة</button>
        </form>
        <ul className="mt-4 space-y-3">
          {testimonials.map((item) => (
            <li key={item.id} className="border-t border-border pt-3">
              <p className="font-medium">{item.authorDisplayName}{item.authorTitle ? ` · ${item.authorTitle}` : ""}</p>
              <p className="mt-1 text-sm leading-7 text-muted">{item.body}</p>
              <div className="mt-2 flex gap-2">
                <StatusBadge label={item.isPublished ? "منشور" : "غير منشور"} tone={item.isPublished ? "success" : "neutral"} />
                <button type="button" className={secondaryButtonClassName} onClick={() => setConfirm({ kind: item.isPublished ? "tes-unpublish" : "tes-publish", id: item.id })}>
                  {item.isPublished ? "إلغاء النشر" : "نشر"}
                </button>
              </div>
            </li>
          ))}
        </ul>
      </section>
      <section className="border border-border bg-surface p-5">
        <h2 className="text-lg font-semibold">الإحصاءات</h2>
        <ul className="mt-4 space-y-4">
          {statistics.map((item) => (
            <li key={item.id} className="grid gap-3 border-t border-border pt-3 sm:grid-cols-2">
              <Field label="التسمية">
                <input className={inputClassName} value={item.label} onChange={(event) => setStatistics((current) => current.map((row) => (row.id === item.id ? { ...row, label: event.target.value } : row)))} />
              </Field>
              <Field label="القيمة المعروضة">
                <input className={inputClassName} value={item.displayValue} onChange={(event) => setStatistics((current) => current.map((row) => (row.id === item.id ? { ...row, displayValue: event.target.value } : row)))} />
              </Field>
              <label className="flex items-center gap-2 text-sm">
                <input type="checkbox" checked={item.isPlaceholder} onChange={(event) => setStatistics((current) => current.map((row) => (row.id === item.id ? { ...row, isPlaceholder: event.target.checked } : row)))} />
                قيمة تجريبية
              </label>
              <label className="flex items-center gap-2 text-sm">
                <input type="checkbox" checked={item.isActive} onChange={(event) => setStatistics((current) => current.map((row) => (row.id === item.id ? { ...row, isActive: event.target.checked } : row)))} />
                نشط
              </label>
              <button type="button" className={secondaryButtonClassName} disabled={pending} onClick={() => saveStatistic(item)}>
                حفظ الإحصاء
              </button>
            </li>
          ))}
        </ul>
      </section>
      <ConfirmDialog
        open={confirm !== null}
        title="تأكيد التحديث؟"
        confirmLabel="تأكيد"
        pending={pending}
        onClose={() => !pending && setConfirm(null)}
        onConfirm={runConfirm}
      />
    </div>
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
