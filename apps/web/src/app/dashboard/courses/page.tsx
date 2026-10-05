"use client";

import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { CourseThumbnail } from "@/components/dashboard/CourseThumbnail";
import { ProgressBar } from "@/components/dashboard/ProgressBar";
import { PageIntro } from "@/components/shared/PageIntro";
import { Badge, ButtonLink } from "@/components/ui/clinic";
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

function statusTone(status: string, canAccess: boolean): "orange" | "mint" | "sand" | "rose" | "navy" {
  if (canAccess) {
    return "mint";
  }
  if (status === "Suspended") {
    return "sand";
  }
  if (status === "Revoked") {
    return "rose";
  }
  return "navy";
}

/** S02 · كورساتي — قائمة الدورات مع التقدّم */
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
        <PageIntro embedded eyebrow="مساحة المتعلم" title="كورساتي" />
        <p className="mt-6 text-sm text-red-600">{error}</p>
      </div>
    );
  }

  if (!items) {
    return (
      <div>
        <PageIntro embedded eyebrow="مساحة المتعلم" title="كورساتي" description="جاري تحميل الدورات..." />
      </div>
    );
  }

  const active = items.filter((item) => item.canAccess);
  const previous = items.filter((item) => !item.canAccess);

  return (
    <div>
      <PageIntro
        embedded
        eyebrow="مساحة المتعلم"
        title="كورساتي"
        description="كل الدورات المرتبطة بحسابك، مع تقدمك الحالي."
      />

      {items.length === 0 ? (
        <section className="clinic-panel mt-10 border-dashed px-6 py-12 text-center">
          <p className="text-lg font-bold">لا توجد دورات مفعلة في حسابك حالياً.</p>
          <p className="mx-auto mt-3 max-w-md text-sm leading-7 text-muted">
            استكشف الكتالوج وأرسل طلب اشتراك، أو فعّل كوداً استلمته من الفريق.
          </p>
          <div className="mt-6 flex flex-wrap justify-center gap-3">
            <ButtonLink href="/courses" variant="accent" size="md">
              استكشف الدورات
            </ButtonLink>
            <ButtonLink href="/dashboard/activate" variant="soft" size="md">
              تفعيل كود
            </ButtonLink>
          </div>
        </section>
      ) : (
        <>
          <section className="mt-10">
            <h2 className="text-xl font-extrabold tracking-tight">الدورات النشطة</h2>
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
              <h2 className="text-xl font-extrabold tracking-tight">الدورات السابقة</h2>
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
    <li className="clinic-card overflow-hidden">
      <CourseThumbnail title={item.courseTitle} src={item.thumbnailUrl} />
      <div className="p-5">
        <div className="flex items-start justify-between gap-3">
          <h3 className="text-lg font-bold leading-7">{item.courseTitle}</h3>
          <Badge tone={statusTone(item.status, item.canAccess)}>
            {enrollmentStatusLabel(item.status, item.canAccess, item.expiresAt)}
          </Badge>
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
              <ButtonLink href={`/dashboard/courses/${item.courseSlug}`} variant="accent" size="sm">
                فتح الدورة
              </ButtonLink>
              {continueHref ? (
                <ButtonLink href={continueHref} variant="soft" size="sm">
                  متابعة التعلم
                </ButtonLink>
              ) : null}
            </>
          ) : item.status === "Suspended" ? (
            <ButtonLink href="/contact" variant="soft" size="sm">
              تواصل مع الدعم
            </ButtonLink>
          ) : null}
        </div>
      </div>
    </li>
  );
}
