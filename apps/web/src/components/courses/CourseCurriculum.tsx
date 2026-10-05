"use client";

import Link from "next/link";
import { useId, useState } from "react";
import type { CourseSectionSummary } from "@/lib/api/public-types";
import { formatDuration } from "@/lib/format";

export function CourseCurriculum({
  slug,
  sections
}: {
  slug: string;
  sections: CourseSectionSummary[];
}) {
  const baseId = useId();
  const [openIds, setOpenIds] = useState<string[]>(sections[0] ? [sections[0].id] : []);

  if (sections.length === 0) {
    return (
      <p className="clinic-card border-dashed px-5 py-8 text-sm text-muted">
        منهج هذه الدورة سيُعرض هنا عند اعتماد الأقسام والدروس.
      </p>
    );
  }

  return (
    <div className="clinic-card divide-y divide-border overflow-hidden rounded-[1.25rem]">
      {sections.map((section, index) => {
        const panelId = `${baseId}-panel-${index}`;
        const buttonId = `${baseId}-button-${index}`;
        const open = openIds.includes(section.id);

        return (
          <div key={section.id} className="px-5 sm:px-6">
            <h3>
              <button
                id={buttonId}
                type="button"
                className="flex w-full items-start justify-between gap-6 py-5 text-right"
                aria-expanded={open}
                aria-controls={panelId}
                onClick={() =>
                  setOpenIds((current) =>
                    current.includes(section.id) ? current.filter((id) => id !== section.id) : [...current, section.id]
                  )
                }
              >
                <span>
                  <span className="block text-base font-bold sm:text-lg">{section.title}</span>
                  <span className="mt-1 block text-xs text-muted">
                    {section.lessonCount} دروس · {formatDuration(section.totalDurationSeconds)}
                  </span>
                </span>
                <span aria-hidden="true" className="mt-1 text-lg font-bold text-accent">
                  {open ? "−" : "+"}
                </span>
              </button>
            </h3>
            <div id={panelId} role="region" aria-labelledby={buttonId} hidden={!open} className="pb-5">
              <ul className="space-y-3">
                {section.lessons.map((lesson) => (
                  <li key={lesson.id} className="flex items-center justify-between gap-4 rounded-xl bg-surface-warm/60 px-3 py-3 text-sm">
                    <div>
                      <p className="font-semibold">
                        {lesson.title}
                        {lesson.isFreePreview ? (
                          <span className="ms-2 align-middle text-[11px] font-bold text-accent">معاينة</span>
                        ) : null}
                      </p>
                      <p className="mt-0.5 text-xs text-muted">{formatDuration(lesson.durationSeconds)}</p>
                    </div>
                    {lesson.isFreePreview ? (
                      <Link
                        href={`/courses/${encodeURIComponent(slug)}/preview/${lesson.id}`}
                        className="shrink-0 rounded-xl bg-accent px-3 py-1.5 text-xs font-semibold text-primary-foreground transition hover:bg-accent-soft"
                      >
                        معاينة
                      </Link>
                    ) : (
                      <span className="inline-flex shrink-0 items-center gap-1 text-xs text-muted" aria-label="درس محمي">
                        <svg viewBox="0 0 16 16" className="h-3.5 w-3.5" aria-hidden="true">
                          <path
                            fill="currentColor"
                            d="M4.5 7V5.2A3.5 3.5 0 0 1 8 1.7a3.5 3.5 0 0 1 3.5 3.5V7h.8A1.7 1.7 0 0 1 14 8.7v4.6A1.7 1.7 0 0 1 12.3 15H3.7A1.7 1.7 0 0 1 2 13.3V8.7A1.7 1.7 0 0 1 3.7 7Zm1.6 0h3.8V5.2a1.9 1.9 0 0 0-3.8 0Z"
                          />
                        </svg>
                        مقفل
                      </span>
                    )}
                  </li>
                ))}
              </ul>
            </div>
          </div>
        );
      })}
    </div>
  );
}
