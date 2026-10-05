"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { CourseThumbnail } from "@/components/dashboard/CourseThumbnail";
import { ProgressBar } from "@/components/dashboard/ProgressBar";
import { ApiRequestError, apiFetch, parseJson } from "@/lib/api/client";
import { getCurrentUser } from "@/lib/auth/session";
import { formatBaghdadDateTime } from "@/lib/format";
import {
  enrollmentRestrictionMessage,
  enrollmentStatusLabel,
  lessonHref,
  type StudentEnrollment
} from "@/lib/learning";
import { safeInternalPath } from "@/lib/safe-path";

export default function DashboardCoursesPage() {
  const router = useRouter();
  const [items, setItems] = useState<StudentEnrollment[] | null>(null);
  const [error, setError] = useState("");

  useEffect(() => {
    getCurrentUser().then(async (session) => {
      if (!session) {
        router.replace(`/login?from=${encodeURIComponent(safeInternalPath("/dashboard/courses"))}`);
        return;
      }

      try {
        const response = await apiFetch("/api/enrollments");
        if (response.status === 401) {
          router.replace(`/login?from=${encodeURIComponent(safeInternalPath("/dashboard/courses"))}`);
          return;
        }

        setItems(await parseJson<StudentEnrollment[]>(response, "تعذر تحميل دوراتك."));
      } catch (caught) {
        setError(caught instanceof ApiRequestError ? caught.message : "تعذر تحميل دوراتك.");
      }
    });
  }, [router]);

  if (error) {
    return (
      <div>
        <h1 className="text-3xl font-semibold">دوراتي</h1>
        <p className="mt-6 text-sm text-red-400">{error}</p>
      </div>
    );
  }

  if (!items) {
    return (
      <div>
        <h1 className="text-3xl font-semibold">دوراتي</h1>
        <p className="mt-6 text-sm text-muted">جاري تحميل الدورات...</p>
      </div>
    );
  }

  const active = items.filter((item) => item.canAccess);
  const previous = items.filter((item) => !item.canAccess);

  return (
    <div>
      <h1 className="text-3xl font-semibold">دوراتي</h1>
      <p className="mt-3 max-w-2xl text-sm leading-8 text-muted">كل الدورات المرتبطة بحسابك، مع تقدمك الحالي.</p>
      {items.length === 0 ? (
        <section className="mt-10 border border-dashed border-border bg-surface px-6 py-12 text-center">
          <p className="text-lg font-semibold">لا توجد دورات مفعلة في حسابك حالياً.</p>
          <Link href="/courses" className="mt-6 inline-flex border border-accent px-4 py-2 text-sm text-accent">
            استكشف الدورات
          </Link>
        </section>
      ) : (
        <>
          <section className="mt-10">
            <h2 className="text-xl font-semibold">الدورات النشطة</h2>
            {active.length === 0 ? (
              <p className="mt-4 text-sm text-muted">لا توجد دورات يمكن التعلّم منها حالياً.</p>
            ) : (
              <ul className="mt-6 grid gap-5 md:grid-cols-2">
                {active.map((item) => (
                  <CourseEnrollmentCard key={item.id} item={item} />
                ))}
              </ul>
            )}
          </section>
          {previous.length > 0 ? (
            <section className="mt-12">
              <h2 className="text-xl font-semibold">الدورات السابقة</h2>
              <ul className="mt-6 grid gap-5 md:grid-cols-2">
                {previous.map((item) => (
                  <CourseEnrollmentCard key={item.id} item={item} />
                ))}
              </ul>
            </section>
          ) : null}
        </>
      )}
    </div>
  );
}

function CourseEnrollmentCard({ item }: { item: StudentEnrollment }) {
  const restriction = enrollmentRestrictionMessage(item.status, item.canAccess, item.expiresAt);
  const continueHref =
    item.canAccess && item.continueLessonId ? lessonHref(item.courseSlug, item.continueLessonId) : null;

  return (
    <li className="overflow-hidden border border-border bg-surface">
      <CourseThumbnail title={item.courseTitle} src={item.thumbnailUrl} />
      <div className="p-5">
        <div className="flex items-start justify-between gap-3">
          <h3 className="text-lg font-semibold">{item.courseTitle}</h3>
          <p className="text-xs text-muted">{enrollmentStatusLabel(item.status, item.canAccess, item.expiresAt)}</p>
        </div>
        <p className="mt-3 text-xs text-muted">بدأت في {formatBaghdadDateTime(item.startedAt)}</p>
        {item.expiresAt ? <p className="mt-1 text-xs text-muted">تنتهي في {formatBaghdadDateTime(item.expiresAt)}</p> : null}
        <div className="mt-5">
          <ProgressBar value={item.progressPercent} label={`${item.completedLessons} من ${item.totalLessons} دروس`} />
        </div>
        {restriction ? <p className="mt-4 text-sm leading-7 text-muted">{restriction}</p> : null}
        <div className="mt-5 flex flex-wrap gap-3 text-sm">
          {item.canAccess ? (
            <>
              <Link href={`/dashboard/courses/${item.courseSlug}`} className="border border-accent px-3 py-1.5 text-accent">
                فتح الدورة
              </Link>
              {continueHref ? (
                <Link href={continueHref} className="border border-border px-3 py-1.5">
                  متابعة التعلم
                </Link>
              ) : null}
            </>
          ) : item.status === "Suspended" ? (
            <Link href="/contact" className="border border-border px-3 py-1.5">
              تواصل مع الدعم
            </Link>
          ) : null}
        </div>
      </div>
    </li>
  );
}
