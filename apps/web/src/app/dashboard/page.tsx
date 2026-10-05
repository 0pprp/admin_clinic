"use client";

import Link from "next/link";
import { ButtonLink } from "@/components/ui/clinic";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { ApiRequestError, apiFetch, parseJson } from "@/lib/api/client";
import { getCurrentUser } from "@/lib/auth/session";
import { formatBaghdadDateTime } from "@/lib/format";
import { lessonHref, type DashboardSummary } from "@/lib/learning";
import { safeInternalPath } from "@/lib/safe-path";

export default function DashboardPage() {
  const router = useRouter();
  const [name, setName] = useState<string | null>(null);
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [error, setError] = useState("");

  useEffect(() => {
    getCurrentUser().then(async (session) => {
      if (!session) {
        router.replace(`/login?from=${encodeURIComponent(safeInternalPath("/dashboard"))}`);
        return;
      }

      setName(session.fullName);
      try {
        const response = await apiFetch("/api/dashboard/summary");
        if (response.status === 401) {
          router.replace(`/login?from=${encodeURIComponent(safeInternalPath("/dashboard"))}`);
          return;
        }

        setSummary(await parseJson<DashboardSummary>(response, "تعذر تحميل نظرة عامة."));
      } catch (caught) {
        setError(caught instanceof ApiRequestError ? caught.message : "تعذر تحميل نظرة عامة.");
      }
    });
  }, [router]);

  if (error) {
    return (
      <div>
        <h1 className="text-3xl font-semibold">لوحة التحكم</h1>
        <p className="mt-6 text-sm text-red-400">{error}</p>
      </div>
    );
  }

  if (!summary || !name) {
    return (
      <div className="animate-pulse space-y-4">
        <div className="h-8 w-56 bg-surface-warm" />
        <div className="h-4 w-80 bg-surface-warm" />
      </div>
    );
  }

  const cards = [
    { label: "دوراتي", value: String(summary.activeCoursesCount), href: "/dashboard/courses" },
    {
      label: "الدروس المكتملة",
      value: `${summary.completedLessonsCount} / ${summary.totalAccessibleLessonsCount}`,
      href: "/dashboard/courses"
    },
    { label: "طلبات الاشتراك", value: String(summary.openPurchaseRequestsCount), href: "/dashboard/orders" }
  ];

  return (
    <div>
      <h1 className="text-3xl font-semibold">مرحباً، {name}</h1>
      <p className="mt-3 max-w-2xl text-sm leading-8 text-muted">تابع دوراتك وتقدمك التعليمي من مكان واحد.</p>
      <div className="mt-8 grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {cards.map((card) => (
          <Link
            key={card.label}
            href={card.href}
            className="clinic-card px-5 py-6 transition hover:border-accent/40 hover:shadow-[0_12px_40px_rgba(7,27,51,0.08)]"
          >
            <p className="text-xs tracking-[0.16em] text-muted">{card.label}</p>
            <p className="mt-3 text-2xl font-semibold">{card.value}</p>
          </Link>
        ))}
        <section className="clinic-card px-5 py-6 sm:col-span-2 xl:col-span-1">
          <p className="text-xs tracking-[0.16em] text-muted">متابعة التعلم</p>
          {summary.continueLearning ? (
            <>
              <p className="mt-3 font-semibold">{summary.continueLearning.courseTitle}</p>
              <p className="mt-1 text-sm text-muted">{summary.continueLearning.lessonTitle}</p>
              {summary.continueLearning.lastWatchedAt ? (
                <p className="mt-2 text-xs text-muted">{formatBaghdadDateTime(summary.continueLearning.lastWatchedAt)}</p>
              ) : null}
              <ButtonLink
                href={lessonHref(summary.continueLearning.courseSlug, summary.continueLearning.lessonId)}
                variant="ghost"
                size="sm"
                className="mt-4 px-0 text-accent hover:bg-transparent"
              >
                متابعة التعلم
              </ButtonLink>
            </>
          ) : (
            <>
              <p className="mt-3 text-sm leading-7 text-muted">لا توجد دورة جاهزة للمتابعة حالياً.</p>
              <Link href="/courses" className="mt-4 inline-flex text-sm text-accent hover:underline">
                استكشف الدورات
              </Link>
            </>
          )}
        </section>
      </div>
      {summary.lastLearningActivity ? (
        <p className="mt-8 text-xs text-muted">آخر نشاط تعليمي: {formatBaghdadDateTime(summary.lastLearningActivity)}</p>
      ) : null}
    </div>
  );
}
