import Link from "next/link";
import { lessonHref, type StudentSection } from "@/lib/learning";
import { formatDuration } from "@/lib/format";
import { ProgressBar } from "@/components/dashboard/ProgressBar";

export function CourseCurriculumNav({
  courseTitle,
  courseSlug,
  sections,
  progressPercent,
  currentLessonId,
  collapsible = false
}: {
  courseTitle: string;
  courseSlug: string;
  sections: StudentSection[];
  progressPercent: number;
  currentLessonId?: string;
  collapsible?: boolean;
}) {
  const content = (
    <div>
      <p className="text-sm font-bold">{courseTitle}</p>
      <div className="mt-4">
        <ProgressBar value={progressPercent} />
      </div>
      <ol className="mt-6 space-y-5">
        {sections.map((section) => (
          <li key={section.id}>
            <p className="text-xs font-bold text-accent">{section.title}</p>
            <ul className="mt-2 space-y-1">
              {section.lessons.map((lesson) => {
                const current = lesson.id === currentLessonId;
                const label = `${lesson.title}${lesson.isCompleted ? "، مكتمل" : ""}`;
                return (
                  <li key={lesson.id}>
                    {lesson.canAccess ? (
                      <Link
                        href={lessonHref(courseSlug, lesson.id)}
                        aria-current={current ? "page" : undefined}
                        className={`flex items-start justify-between gap-3 rounded-xl px-2.5 py-2 text-sm transition ${
                          current
                            ? "bg-accent/10 font-semibold text-accent"
                            : "hover:bg-surface-warm"
                        }`}
                      >
                        <span>
                          {lesson.isCompleted ? "✓ " : ""}
                          {lesson.title}
                        </span>
                        <span className="shrink-0 text-xs text-muted">{formatDuration(lesson.durationSeconds)}</span>
                      </Link>
                    ) : (
                      <p
                        className="flex items-start justify-between gap-3 rounded-xl px-2.5 py-2 text-sm text-muted"
                        aria-label={`${label}، مقفل`}
                      >
                        <span>مقفل · {lesson.title}</span>
                        <span className="shrink-0 text-xs">{formatDuration(lesson.durationSeconds)}</span>
                      </p>
                    )}
                  </li>
                );
              })}
            </ul>
          </li>
        ))}
      </ol>
    </div>
  );

  if (!collapsible) {
    return <nav aria-label="منهج الدورة">{content}</nav>;
  }

  return (
    <details className="clinic-card px-4 py-3 lg:hidden">
      <summary className="cursor-pointer text-sm font-bold">منهج الدورة</summary>
      <nav aria-label="منهج الدورة" className="mt-4">
        {content}
      </nav>
    </details>
  );
}
