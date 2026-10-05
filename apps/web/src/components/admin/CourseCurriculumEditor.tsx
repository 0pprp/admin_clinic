"use client";

import { FormEvent, useState } from "react";
import { ConfirmDialog } from "@/components/admin/ConfirmDialog";
import { Field } from "@/components/admin/PageHeader";
import { StatusBadge } from "@/components/admin/StatusBadge";
import { useToast } from "@/components/admin/ToastProvider";
import { ClinicSelect } from "@/components/ui/clinic";
import { adminJson, adminVoid, errorMessage } from "@/lib/admin/http";
import { labelOrRaw, lessonStatusLabel, statusTone, videoProcessingLabel, videoProviderLabel } from "@/lib/admin/labels";
import { inputClassName, primaryButtonClassName, secondaryButtonClassName } from "@/lib/admin/ui";
import type { AdminCourseDetail, AdminLesson, AdminLessonVideoStatus, AdminSection, ReorderItem } from "@/lib/admin/types";
import { formatDuration } from "@/lib/format";
import { apiUpload, readApiError } from "@/lib/api/client";

const VIDEO_PROVIDERS = ["SelfHostedHls", "None", "BunnyStream"];
const VIDEO_PROVIDER_OPTIONS = VIDEO_PROVIDERS.map((provider) => ({
  value: provider,
  label: labelOrRaw(videoProviderLabel, provider)
}));

export function CourseCurriculumEditor({
  course,
  onReload
}: {
  course: AdminCourseDetail;
  onReload: () => Promise<void>;
}) {
  const toast = useToast();
  const [error, setError] = useState("");
  const [pending, setPending] = useState(false);
  const [sectionTitle, setSectionTitle] = useState("");
  const [sectionDescription, setSectionDescription] = useState("");
  const [editingSection, setEditingSection] = useState<AdminSection | null>(null);
  const [lessonForm, setLessonForm] = useState<LessonDraft | null>(null);
  const [confirmDelete, setConfirmDelete] = useState<AdminSection | null>(null);
  const [uploadFile, setUploadFile] = useState<File | null>(null);
  const [uploadMessage, setUploadMessage] = useState("");

  const sections = [...course.sections].sort((a, b) => a.sortOrder - b.sortOrder);

  async function addSection(event: FormEvent) {
    event.preventDefault();
    setPending(true);
    setError("");
    try {
      await adminJson(`/api/admin/courses/${course.id}/sections`, "تعذر إضافة القسم.", {
        method: "POST",
        body: JSON.stringify({ title: sectionTitle.trim(), description: sectionDescription.trim() || null })
      });
      setSectionTitle("");
      setSectionDescription("");
      toast.show("تمت إضافة القسم.");
      await onReload();
    } catch (caught) {
      setError(errorMessage(caught, "تعذر إضافة القسم."));
    } finally {
      setPending(false);
    }
  }

  async function saveSection() {
    if (!editingSection) {
      return;
    }

    setPending(true);
    setError("");
    try {
      await adminJson(`/api/admin/sections/${editingSection.id}`, "تعذر حفظ القسم.", {
        method: "PUT",
        body: JSON.stringify({
          title: editingSection.title.trim(),
          description: editingSection.description?.trim() || null,
          sortOrder: editingSection.sortOrder
        })
      });
      toast.show("تم حفظ القسم.");
      setEditingSection(null);
      await onReload();
    } catch (caught) {
      setError(errorMessage(caught, "تعذر حفظ القسم."));
    } finally {
      setPending(false);
    }
  }

  async function deleteSection() {
    if (!confirmDelete) {
      return;
    }

    setPending(true);
    setError("");
    try {
      await adminVoid(`/api/admin/sections/${confirmDelete.id}`, "تعذر حذف القسم.", { method: "DELETE" });
      toast.show("تم حذف القسم.");
      setConfirmDelete(null);
      await onReload();
    } catch (caught) {
      setError(errorMessage(caught, "تعذر حذف القسم."));
    } finally {
      setPending(false);
    }
  }

  async function reorderSections(next: AdminSection[]) {
    setPending(true);
    setError("");
    try {
      await adminVoid(`/api/admin/courses/${course.id}/sections/reorder`, "تعذر إعادة الترتيب.", {
        method: "POST",
        body: JSON.stringify({ items: toReorder(next) })
      });
      await onReload();
    } catch (caught) {
      setError(errorMessage(caught, "تعذر إعادة الترتيب."));
    } finally {
      setPending(false);
    }
  }

  async function saveLesson() {
    if (!lessonForm) {
      return;
    }

    setPending(true);
    setError("");
    setUploadMessage("");
    try {
      const selfHostedWithoutReadyVideo =
        lessonForm.videoProvider === "SelfHostedHls" && !lessonForm.videoKey.trim();
      const payload = {
        title: lessonForm.title.trim(),
        description: lessonForm.description.trim() || null,
        durationSeconds: Number(lessonForm.durationSeconds) || 0,
        isFreePreview: lessonForm.isFreePreview,
        // لا تُعلَّم SelfHosted قبل نجاح الرفع الفعلي؛ وإلا يظهر الدرس بلا فيديو.
        videoProvider: selfHostedWithoutReadyVideo ? "None" : lessonForm.videoProvider,
        videoKey: selfHostedWithoutReadyVideo
          ? null
          : lessonForm.videoKey.trim() || null
      };
      let lessonId = lessonForm.id;
      if (lessonForm.id) {
        await adminJson(`/api/admin/lessons/${lessonForm.id}`, "تعذر حفظ الدرس.", {
          method: "PUT",
          body: JSON.stringify(payload)
        });
      } else {
        const created = await adminJson<AdminLesson>(`/api/admin/sections/${lessonForm.sectionId}/lessons`, "تعذر إضافة الدرس.", {
          method: "POST",
          body: JSON.stringify(payload)
        });
        lessonId = created.id;
      }

      if (lessonForm.videoProvider === "SelfHostedHls" && !uploadFile && !lessonForm.videoKey.trim()) {
        throw new Error("اختر ملف فيديو ثم احفظ الدرس ليتم الرفع والمعالجة.");
      }

      if (uploadFile && lessonId) {
        const sizeMb = (uploadFile.size / (1024 * 1024)).toFixed(1);
        setUploadMessage(`جاري رفع الفيديو (${sizeMb} MB)... 0%`);
        const form = new FormData();
        form.append("file", uploadFile, uploadFile.name);
        const response = await apiUpload(`/api/admin/lessons/${lessonId}/video`, form, (percent) => {
          setUploadMessage(`جاري رفع الفيديو (${sizeMb} MB)... ${percent}%`);
        });
        if (!response.ok) {
          const detail = await readApiError(response, "تعذر رفع الفيديو.");
          throw new Error(detail);
        }
        const status = (await response.json()) as AdminLessonVideoStatus;
        setUploadMessage(status.message ?? "تم استلام الفيديو وبدأت المعالجة في الخلفية.");
        toast.show("تم رفع الفيديو. التحويل للجودة المتعددة يعمل الآن في الخلفية.");
      } else {
        toast.show("تم حفظ الدرس.");
      }

      setLessonForm(null);
      setUploadFile(null);
      await onReload();
    } catch (caught) {
      setError(errorMessage(caught, "تعذر حفظ الدرس أو رفع الفيديو."));
    } finally {
      setPending(false);
    }
  }

  async function lessonAction(id: string, action: "publish" | "archive") {
    setPending(true);
    setError("");
    try {
      await adminJson(`/api/admin/lessons/${id}/${action}`, "تعذر تحديث الدرس.", { method: "POST" });
      toast.show(action === "publish" ? "تم نشر الدرس." : "تمت أرشفة الدرس.");
      await onReload();
    } catch (caught) {
      setError(errorMessage(caught, "تعذر تحديث الدرس."));
    } finally {
      setPending(false);
    }
  }

  async function reorderLessons(section: AdminSection, next: AdminLesson[]) {
    setPending(true);
    setError("");
    try {
      await adminVoid(`/api/admin/sections/${section.id}/lessons/reorder`, "تعذر إعادة ترتيب الدروس.", {
        method: "POST",
        body: JSON.stringify({ items: toReorder(next) })
      });
      await onReload();
    } catch (caught) {
      setError(errorMessage(caught, "تعذر إعادة ترتيب الدروس."));
    } finally {
      setPending(false);
    }
  }

  return (
    <section className="mt-10">
      <h2 className="text-lg font-semibold">الأقسام والدروس</h2>
      {error ? <p className="mt-3 text-sm text-red-700">{error}</p> : null}
      <form onSubmit={addSection} className="clinic-card mt-4 grid gap-3 p-4 sm:grid-cols-[1fr_1fr_auto]">
        <input className={inputClassName} placeholder="عنوان قسم جديد" value={sectionTitle} onChange={(event) => setSectionTitle(event.target.value)} required />
        <input className={inputClassName} placeholder="وصف اختياري" value={sectionDescription} onChange={(event) => setSectionDescription(event.target.value)} />
        <button type="submit" className={secondaryButtonClassName} disabled={pending}>
          إضافة قسم
        </button>
      </form>
      <div className="mt-6 space-y-6">
        {sections.map((section, index) => {
          const lessons = [...section.lessons].sort((a, b) => a.sortOrder - b.sortOrder);
          return (
            <article key={section.id} className="clinic-card p-4">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <h3 className="font-semibold">{section.title}</h3>
                  {section.description ? <p className="mt-1 text-sm text-muted">{section.description}</p> : null}
                </div>
                <div className="flex flex-wrap gap-2">
                  <button type="button" className={secondaryButtonClassName} disabled={pending || index === 0} onClick={() => reorderSections(moveItem(sections, index, -1))}>
                    أعلى
                  </button>
                  <button type="button" className={secondaryButtonClassName} disabled={pending || index === sections.length - 1} onClick={() => reorderSections(moveItem(sections, index, 1))}>
                    أسفل
                  </button>
                  <button type="button" className={secondaryButtonClassName} onClick={() => setEditingSection(section)}>
                    تعديل
                  </button>
                  <button type="button" className={secondaryButtonClassName} onClick={() => setConfirmDelete(section)}>
                    حذف
                  </button>
                  <button
                    type="button"
                    className={primaryButtonClassName}
                    onClick={() => {
                      setLessonForm({
                        id: null,
                        sectionId: section.id,
                        title: "",
                        description: "",
                        durationSeconds: 0,
                        isFreePreview: false,
                        videoProvider: "SelfHostedHls",
                        videoKey: ""
                      });
                      setUploadFile(null);
                      setUploadMessage("");
                    }}
                  >
                    إضافة درس
                  </button>
                </div>
              </div>
              {editingSection?.id === section.id ? (
                <div className="mt-4 grid gap-3 sm:grid-cols-2">
                  <Field label="عنوان القسم">
                    <input className={inputClassName} value={editingSection.title} onChange={(event) => setEditingSection({ ...editingSection, title: event.target.value })} />
                  </Field>
                  <Field label="الوصف">
                    <input className={inputClassName} value={editingSection.description ?? ""} onChange={(event) => setEditingSection({ ...editingSection, description: event.target.value })} />
                  </Field>
                  <div className="flex gap-2">
                    <button type="button" className={primaryButtonClassName} disabled={pending} onClick={saveSection}>
                      حفظ القسم
                    </button>
                    <button type="button" className={secondaryButtonClassName} onClick={() => setEditingSection(null)}>
                      إلغاء
                    </button>
                  </div>
                </div>
              ) : null}
              <ul className="mt-4 divide-y divide-border">
                {lessons.length === 0 ? <li className="py-3 text-sm text-muted">لا توجد دروس في هذا القسم.</li> : null}
                {lessons.map((lesson, lessonIndex) => (
                  <li key={lesson.id} className="flex flex-col gap-2 py-3 sm:flex-row sm:items-center sm:justify-between">
                    <div>
                      <p className="font-medium">{lesson.title}</p>
                      <p className="text-xs text-muted">
                        {formatDuration(lesson.durationSeconds)} · {labelOrRaw(videoProviderLabel, lesson.videoProvider)}
                        {lesson.videoProcessingStatus
                          ? ` · ${labelOrRaw(videoProcessingLabel, lesson.videoProcessingStatus)}`
                          : ""}
                        {lesson.isFreePreview ? " · معاينة مجانية" : ""}
                      </p>
                      {lesson.videoProcessingMessage ? (
                        <p className="mt-1 text-xs text-accent">{lesson.videoProcessingMessage}</p>
                      ) : null}
                    </div>
                    <div className="flex flex-wrap items-center gap-2">
                      <StatusBadge label={labelOrRaw(lessonStatusLabel, lesson.status)} tone={statusTone(lesson.status)} />
                      <button type="button" className={secondaryButtonClassName} disabled={pending || lessonIndex === 0} onClick={() => reorderLessons(section, moveItem(lessons, lessonIndex, -1))}>
                        أعلى
                      </button>
                      <button type="button" className={secondaryButtonClassName} disabled={pending || lessonIndex === lessons.length - 1} onClick={() => reorderLessons(section, moveItem(lessons, lessonIndex, 1))}>
                        أسفل
                      </button>
                      <button
                        type="button"
                        className={secondaryButtonClassName}
                        onClick={() => {
                          setLessonForm(fromLesson(lesson));
                          setUploadFile(null);
                          setUploadMessage(lesson.videoProcessingMessage ?? "");
                        }}
                      >
                        تعديل
                      </button>
                      {lesson.status !== "Published" ? (
                        <button type="button" className={secondaryButtonClassName} disabled={pending} onClick={() => lessonAction(lesson.id, "publish")}>
                          نشر
                        </button>
                      ) : null}
                      {lesson.status !== "Archived" ? (
                        <button type="button" className={secondaryButtonClassName} disabled={pending} onClick={() => lessonAction(lesson.id, "archive")}>
                          أرشفة
                        </button>
                      ) : null}
                    </div>
                  </li>
                ))}
              </ul>
            </article>
          );
        })}
      </div>
      {lessonForm ? (
        <div className="fixed inset-0 z-40 flex items-center justify-center bg-surface-dark/50 p-4">
          <div className="clinic-card max-h-[90vh] w-full max-w-lg overflow-y-auto p-6 shadow-[0_24px_64px_rgba(15,23,42,0.22)]" role="dialog" aria-modal="true">
            <h3 className="text-lg font-semibold">{lessonForm.id ? "تعديل الدرس" : "درس جديد"}</h3>
            <div className="mt-4 space-y-3">
              <Field label="العنوان">
                <input className={inputClassName} value={lessonForm.title} onChange={(event) => setLessonForm({ ...lessonForm, title: event.target.value })} />
              </Field>
              <Field label="الوصف">
                <textarea className={inputClassName} rows={3} value={lessonForm.description} onChange={(event) => setLessonForm({ ...lessonForm, description: event.target.value })} />
              </Field>
              <Field label="المدة بالثواني">
                <input className={inputClassName} type="number" min={0} value={lessonForm.durationSeconds} onChange={(event) => setLessonForm({ ...lessonForm, durationSeconds: Number(event.target.value) })} />
              </Field>
              <label className="flex items-center gap-2 text-sm">
                <input type="checkbox" checked={lessonForm.isFreePreview} onChange={(event) => setLessonForm({ ...lessonForm, isFreePreview: event.target.checked })} />
                معاينة مجانية
              </label>
              <Field label="مصدر الفيديو">
                <ClinicSelect
                  value={lessonForm.videoProvider}
                  onChange={(videoProvider) => setLessonForm({ ...lessonForm, videoProvider })}
                  options={VIDEO_PROVIDER_OPTIONS}
                />
              </Field>
              {lessonForm.videoProvider === "SelfHostedHls" ? (
                <Field label="رفع فيديو إلى سيرفر العيادة">
                  <input
                    className={inputClassName}
                    type="file"
                    accept="video/mp4,video/webm,video/quicktime,.mp4,.mov,.mkv,.webm,.m4v"
                    onChange={(event) => setUploadFile(event.target.files?.[0] ?? null)}
                  />
                  <p className="mt-2 text-xs leading-6 text-muted">
                    بعد الحفظ يُحوَّل الفيديو تلقائياً إلى عدة جودات (360/720/1080) ويختار المشغّل الأنسب حسب سرعة النت مثل يوتيوب.
                  </p>
                  {uploadFile ? <p className="mt-1 text-xs text-accent">الملف المختار: {uploadFile.name}</p> : null}
                  {uploadMessage ? <p className="mt-1 text-xs text-muted">{uploadMessage}</p> : null}
                </Field>
              ) : (
                <Field label="مفتاح الفيديو الخارجي (إن وُجد)">
                  <input className={inputClassName} value={lessonForm.videoKey} onChange={(event) => setLessonForm({ ...lessonForm, videoKey: event.target.value })} />
                </Field>
              )}
            </div>
            <div className="mt-6 flex justify-end gap-2">
              <button type="button" className={secondaryButtonClassName} onClick={() => setLessonForm(null)}>
                إلغاء
              </button>
              <button type="button" className={primaryButtonClassName} disabled={pending} onClick={saveLesson}>
                حفظ الدرس
              </button>
            </div>
          </div>
        </div>
      ) : null}
      <ConfirmDialog
        open={confirmDelete !== null}
        title={confirmDelete ? `حذف قسم «${confirmDelete.title}»؟` : ""}
        description="سيتم حذف القسم ودروسه."
        confirmLabel="حذف"
        tone="danger"
        pending={pending}
        onClose={() => !pending && setConfirmDelete(null)}
        onConfirm={deleteSection}
      />
    </section>
  );
}

type LessonDraft = {
  id: string | null;
  sectionId: string;
  title: string;
  description: string;
  durationSeconds: number;
  isFreePreview: boolean;
  videoProvider: string;
  videoKey: string;
};

function fromLesson(lesson: AdminLesson): LessonDraft {
  return {
    id: lesson.id,
    sectionId: lesson.sectionId,
    title: lesson.title,
    description: lesson.description ?? "",
    durationSeconds: lesson.durationSeconds,
    isFreePreview: lesson.isFreePreview,
    videoProvider: lesson.videoProvider,
    videoKey: lesson.videoKey ?? ""
  };
}

function moveItem<T>(items: T[], index: number, delta: number): T[] {
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

function toReorder(items: Array<{ id: string }>): ReorderItem[] {
  return items.map((item, index) => ({ id: item.id, sortOrder: index + 1 }));
}
