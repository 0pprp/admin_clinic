"use client";

import { useMemo, useState } from "react";
import { PageHeader } from "@/components/admin/PageHeader";
import { AdCarousel, Badge, Button, ClinicField, ClinicInput, ClinicSelect, ClinicTextarea } from "@/components/ui/clinic";
import { homeAdSlides, type AdSlide } from "@/lib/content/ads";

const emptyDraft: AdSlide = {
  id: "",
  eyebrow: "",
  title: "",
  body: "",
  ctaLabel: "اعرف المزيد",
  ctaHref: "/courses",
  tone: "navy"
};

export default function AdminAdsPage() {
  const [slides, setSlides] = useState<AdSlide[]>(homeAdSlides);
  const [draft, setDraft] = useState<AdSlide>(emptyDraft);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [savedNote, setSavedNote] = useState("");

  const preview = useMemo(() => slides, [slides]);

  function startCreate() {
    setEditingId(null);
    setDraft({ ...emptyDraft, id: `ad-${Date.now()}` });
    setSavedNote("");
  }

  function startEdit(slide: AdSlide) {
    setEditingId(slide.id);
    setDraft(slide);
    setSavedNote("");
  }

  function saveDraft() {
    if (!draft.id || !draft.title.trim()) {
      return;
    }
    setSlides((current) => {
      const exists = current.some((item) => item.id === draft.id);
      if (exists) {
        return current.map((item) => (item.id === draft.id ? draft : item));
      }
      return [...current, draft];
    });
    setSavedNote("تم حفظ الإعلان محلياً في لوحة الإدارة. لنشر دائم على السيرفر اربط لاحقاً بتخزين الموقع.");
    setEditingId(draft.id);
  }

  function removeSlide(id: string) {
    setSlides((current) => current.filter((item) => item.id !== id));
    if (editingId === id) {
      setDraft(emptyDraft);
      setEditingId(null);
    }
  }

  return (
    <div>
      <PageHeader
        title="الإعلانات"
        description="إدارة شرائح الكاروسيل الظاهرة في الصفحة الرئيسية وفق تصميم العيادة."
        actions={
          <Button variant="accent" onClick={startCreate}>
            إعلان جديد
          </Button>
        }
      />

      <section className="clinic-card mb-8 overflow-hidden p-4 sm:p-5">
        <div className="mb-4 flex items-center justify-between gap-3">
          <h2 className="text-base font-extrabold tracking-tight">معاينة مباشرة</h2>
          <Badge tone="orange">{preview.length} شريحة</Badge>
        </div>
        <AdCarousel slides={preview} embedded />
      </section>

      <div className="grid gap-6 lg:grid-cols-[1.1fr_0.9fr]">
        <div className="clinic-card p-5">
          <div className="mb-4 flex items-center justify-between gap-3">
            <h2 className="text-lg font-extrabold tracking-tight">الشرائح</h2>
            <Badge tone="navy">{slides.length} إعلان</Badge>
          </div>
          {slides.length === 0 ? (
            <p className="rounded-xl bg-surface-warm px-4 py-8 text-center text-sm text-muted">لا توجد إعلانات بعد. أضف شريحة جديدة.</p>
          ) : (
            <ul className="space-y-3">
              {slides.map((slide, index) => (
                <li
                  key={slide.id}
                  className={`rounded-xl border p-4 transition ${
                    editingId === slide.id ? "border-accent bg-[#fff7f2]" : "border-border hover:border-accent/40"
                  }`}
                >
                  <div className="flex flex-wrap items-start justify-between gap-3">
                    <div className="min-w-0">
                      <p className="text-[11px] font-bold text-accent">A{String(index + 1).padStart(2, "0")}</p>
                      <p className="mt-1 font-semibold">{slide.title}</p>
                      <p className="mt-1 text-xs text-muted">{slide.eyebrow || "بدون عنوان فرعي"}</p>
                    </div>
                    <div className="flex gap-2">
                      <Button size="sm" variant="soft" onClick={() => startEdit(slide)}>
                        تحرير
                      </Button>
                      <Button size="sm" variant="ghost" onClick={() => removeSlide(slide.id)}>
                        حذف
                      </Button>
                    </div>
                  </div>
                </li>
              ))}
            </ul>
          )}
        </div>

        <div className="clinic-card p-5">
          <h2 className="text-lg font-extrabold tracking-tight">{editingId ? "تحرير إعلان" : "إعلان جديد"}</h2>
          <p className="mt-1 text-xs text-muted">الحقول تطابق شريحة الكاروسيل في الصفحة الرئيسية.</p>
          <div className="mt-4 space-y-1">
            <ClinicField label="عنوان فرعي">
              <ClinicInput
                value={draft.eyebrow}
                onChange={(event) => setDraft((value) => ({ ...value, eyebrow: event.target.value }))}
              />
            </ClinicField>
            <ClinicField label="العنوان">
              <ClinicInput
                value={draft.title}
                onChange={(event) => setDraft((value) => ({ ...value, title: event.target.value }))}
              />
            </ClinicField>
            <ClinicField label="الوصف">
              <ClinicTextarea
                value={draft.body}
                onChange={(event) => setDraft((value) => ({ ...value, body: event.target.value }))}
              />
            </ClinicField>
            <ClinicField label="نص الزر">
              <ClinicInput
                value={draft.ctaLabel}
                onChange={(event) => setDraft((value) => ({ ...value, ctaLabel: event.target.value }))}
              />
            </ClinicField>
            <ClinicField label="رابط الزر">
              <ClinicInput
                value={draft.ctaHref}
                onChange={(event) => setDraft((value) => ({ ...value, ctaHref: event.target.value }))}
              />
            </ClinicField>
            <ClinicField label="النمط">
              <ClinicSelect
                value={draft.tone}
                onChange={(next) => setDraft((value) => ({ ...value, tone: next as AdSlide["tone"] }))}
                options={[
                  { value: "navy", label: "كحلي" },
                  { value: "light", label: "فاتح" }
                ]}
              />
            </ClinicField>
            <Button variant="accent" onClick={saveDraft} disabled={!draft.title.trim()}>
              حفظ الإعلان
            </Button>
            {savedNote ? <p className="mt-3 text-xs leading-6 text-muted">{savedNote}</p> : null}
          </div>
        </div>
      </div>
    </div>
  );
}
