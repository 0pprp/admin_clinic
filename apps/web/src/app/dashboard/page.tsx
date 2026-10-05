"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { CourseThumbnail } from "@/components/dashboard/CourseThumbnail";
import { ProgressBar } from "@/components/dashboard/ProgressBar";
import { PageIntro } from "@/components/shared/PageIntro";
import { Badge, ButtonLink, Metric } from "@/components/ui/clinic";
import { ApiRequestError, apiFetch, parseJson } from "@/lib/api/client";
import { getCurrentUser } from "@/lib/auth/session";
import { formatBaghdadDateTime } from "@/lib/format";
import {
  enrollmentStatusLabel,
  lessonHref,
  shortDisplayName,
  type DashboardSummary,
  type StudentEnrollment
} from "@/lib/learning";
import { safeInternalPath } from "@/lib/safe-path";

/** S01 · نظرة عامة — ترحيب + مقاييس + بطاقات الدورات والتقدّم */
export default function DashboardPage() {
  const router = useRouter();
  const [name, setName] = useState<string | null>(null);
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [courses, setCourses] = useState<StudentEnrollment[] | null>(null);
  const [error, setError] = useState("");

  useEffect(() => {
    getCurrentUser().then(async (session) => {
      if (!session) {
        router.replace(`/login?from=${encodeURIComponent(safeInternalPath("/dashboard"))}`);
        return;
      }

      setName(session.fullName);
      try {
        const [summaryRes, enrollmentsRes] = await Promise.all([
          apiFetch("/api/dashboard/summary"),
          apiFetch("/api/enrollments")
        ]);

        if (summaryRes.status === 401 || enrollmentsRes.status === 401) {
          router.replace(`/login?from=${encodeURIComponent(safeInternalPath("/dashboard"))}`);
          return;
        }

        setSummary(await parseJson<DashboardSummary>(summaryRes, "تعذر تحميل نظرة عامة."));
        setCourses(await parseJson<StudentEnrollment[]>(enrollmentsRes, "تعذر تحميل دوراتك."));
      } catch (caught) {
        setError(caught instanceof ApiRequestError ? caught.message : "تعذر تحميل نظرة عامة.");
      }
    });
  }, [router]);

  if (error) {
    return (
      <div>
        <PageIntro embedded eyebrow="مساحة المتعلم" title="نظرة عامة" />
        <p className="mt-6 text-sm text-red-600">{error}</p>
      </div>
    );
  }

  if (!summary || !name || !courses) {
    return (
      <div className="animate-pulse space-y-4">
        <div className="h-8 w-56 rounded-lg bg-surface-warm" />
        <div className="h-4 w-80 rounded-lg bg-surface-warm" />
        <div className="mt-8 grid gap-4 sm:grid-cols-3">
          <div className="h-24 rounded-2xl bg-surface-warm" />
          <div className="h-24 rounded-2xl bg-surface-warm" />
          <div className="h-24 rounded-2xl bg-surface-warm" />
        </div>
      </div>
    );
  }

  const greet = shortDisplayName(name);
  const activeCourses = courses.filter((item) => item.canAccess).slice(0, 4);

  return (
    <div>
      <PageIntro
        embedded
        eyebrow="مساحة المتعلم"
        title={`مرحباً، ${greet}`}
        description="تابع دوراتك وتقدمك التعليمي من مكان واحد."
      />

      <div className="mt-8 grid gap-4 sm:grid-cols-3">
        <Link href="/dashboard/courses" className="block transition hover:opacity-95">
          <Metric value={summary.activeCoursesCount} label="دوراتي النشطة" className="clinic-card rounded-[1rem] border-0 shadow-[var(--shadow-card)]" />
        </Link>
        <Link href="/dashboard/courses" className="block transition hover:opacity-95">
          <Metric
            value={`${summary.completedLessonsCount} / ${summary.totalAccessibleLessonsCount}`}
            label="الدروس المكتملة"
            className="clinic-card rounded-[1rem] border-0 shadow-[var(--shadow-card)]"
          />
        </Link>
        <Link href="/dashboard/orders" className="block transition hover:opacity-95">
          <Metric
            value={summary.openPurchaseRequestsCount}
            label="طلبات الاشتراك"
            className="clinic-card rounded-[1rem] border-0 shadow-[var(--shadow-card)]"
          />
        </Link>
      </div>

      <section className="clinic-card mt-8 px-5 py-6 sm:px-6">
        <p className="text-sm font-bold text-accent">متابعة التعلم</p>
        {summary.continueLearning ? (
          <div className="mt-4 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
            <div className="min-w-0">
              <p className="text-lg font-bold">{summary.continueLearning.courseTitle}</p>
              <p className="mt-1 text-sm text-muted">{summary.continueLearning.lessonTitle}</p>
              {summary.continueLearning.lastWatchedAt ? (
                <p className="mt-2 text-xs text-muted">{formatBaghdadDateTime(summary.continueLearning.lastWatchedAt)}</p>
              ) : null}
            </div>
            <ButtonLink
              href={lessonHref(summary.continueLearning.courseSlug, summary.continueLearning.lessonId)}
              variant="accent"
              size="md"
            >
              متابعة التعلم
            </ButtonLink>
          </div>
        ) : (
          <div className="mt-4 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <p className="text-sm leading-7 text-muted">لا توجد دورة جاهزة للمتابعة حالياً.</p>
            <ButtonLink href="/courses" variant="soft" size="md">
              استكشف الدورات
            </ButtonLink>
          </div>
        )}
      </section>

      <section className="mt-10">
        <div className="flex items-end justify-between gap-3">
          <div>
            <p className="text-sm font-bold text-accent">كورساتي</p>
            <h2 className="mt-1 text-xl font-extrabold tracking-tight">الدورات النشطة</h2>
          </div>
          <Link href="/dashboard/courses" className="text-sm font-semibold text-accent hover:underline">
            عرض الكل
          </Link>
        </div>

        {activeCourses.length === 0 ? (
          <div className="clinic-panel mt-5 border-dashed px-5 py-10 text-center">
            <p className="font-semibold">لا توجد دورات مفعلة حالياً.</p>
            <ButtonLink href="/courses" variant="accent" size="md" className="mt-5">
              استكشف الدورات
            </ButtonLink>
          </div>
        ) : (
          <ul className="mt-5 grid gap-5 md:grid-cols-2">
            {activeCourses.map((item) => (
              <li key={item.id} className="clinic-card overflow-hidden">
                <CourseThumbnail title={item.courseTitle} src={item.thumbnailUrl} />
                <div className="p-5">
                  <div className="flex items-start justify-between gap-3">
                    <h3 className="text-base font-bold leading-7">{item.courseTitle}</h3>
                    <Badge tone="orange">{enrollmentStatusLabel(item.status, item.canAccess, item.expiresAt)}</Badge>
                  </div>
                  <div className="mt-4">
                    <ProgressBar value={item.progressPercent} label={`${item.completedLessons} من ${item.totalLessons} دروس`} />
                  </div>
                  <div className="mt-5 flex flex-wrap gap-2">
                    <ButtonLink href={`/dashboard/courses/${item.courseSlug}`} variant="accent" size="sm">
                      فتح الدورة
                    </ButtonLink>
                    {item.continueLessonId ? (
                      <ButtonLink href={lessonHref(item.courseSlug, item.continueLessonId)} variant="soft" size="sm">
                        متابعة
                      </ButtonLink>
                    ) : null}
                  </div>
                </div>
              </li>
            ))}
          </ul>
        )}
      </section>

      {summary.lastLearningActivity ? (
        <p className="mt-8 text-xs text-muted">آخر نشاط تعليمي: {formatBaghdadDateTime(summary.lastLearningActivity)}</p>
      ) : null}
    </div>
  );
}
