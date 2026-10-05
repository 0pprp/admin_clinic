"use client";

import { useMemo, useState } from "react";
import { PageHeader } from "@/components/admin/PageHeader";
import { AdCarousel, Badge, Button, ClinicField, ClinicInput, ClinicTextarea } from "@/components/ui/clinic";
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
  }

  function startEdit(slide: AdSlide) {
    setEditingId(slide.id);
    setDraft(slide);
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
        description="إدارة شرائح الكاروسيل الظاهرة في الصفحة الرئيسية وفق تصميم Figma."
        actions={
          <Button variant="accent" onClick={startCreate}>
            إعلان جديد
          </Button>
        }
      />

      <div className="mb-8">
        <AdCarousel slides={preview} />
      </div>

      <div className="grid gap-6 lg:grid-cols-[1.1fr_0.9fr]">
        <div className="clinic-panel p-5">
          <div className="mb-4 flex items-center justify-between gap-3">
            <h2 className="text-lg font-semibold">الشرائح</h2>
            <Badge tone="orange">{slides.length} إعلان</Badge>
          </div>
          <ul className="space-y-3">
            {slides.map((slide) => (
              <li key={slide.id} className="rounded-md border border-border p-4">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <p className="font-medium">{slide.title}</p>
                    <p className="mt-1 text-xs text-muted">{slide.eyebrow}</p>
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
        </div>

        <div className="clinic-panel p-5">
          <h2 className="text-lg font-semibold">{editingId ? "تحرير إعلان" : "إعلان جديد"}</h2>
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
              <select
                className="w-full rounded-md border border-border bg-surface px-3.5 py-2.5 text-sm"
                value={draft.tone}
                onChange={(event) =>
                  setDraft((value) => ({ ...value, tone: event.target.value as AdSlide["tone"] }))
                }
              >
                <option value="navy">كحلي</option>
                <option value="light">فاتح</option>
              </select>
            </ClinicField>
            <Button variant="primary" onClick={saveDraft}>
              حفظ الإعلان
            </Button>
            {savedNote ? <p className="mt-3 text-xs leading-6 text-muted">{savedNote}</p> : null}
          </div>
        </div>
      </div>
    </div>
  );
}
