"use client";

import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { AccessDenied } from "@/components/dashboard/AccessDenied";
import { CourseThumbnail } from "@/components/dashboard/CourseThumbnail";
import { CourseCurriculumNav } from "@/components/learning/CourseCurriculumNav";
import { ApiRequestError, apiFetch, parseJson } from "@/lib/api/client";
import { getCurrentUser } from "@/lib/auth/session";
import { formatBaghdadDateTime } from "@/lib/format";
import { lessonHref, type StudentCourseLearning } from "@/lib/learning";
import { safeInternalPath } from "@/lib/safe-path";

export default function StudentCoursePage() {
  const router = useRouter();
  const params = useParams<{ slug: string }>();
  const slug = params.slug;
  const [course, setCourse] = useState<StudentCourseLearning | null>(null);
  const [denied, setDenied] = useState(false);
  const [missing, setMissing] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!slug) {
      return;
    }

    const from = `/dashboard/courses/${slug}`;
    getCurrentUser().then(async (session) => {
      if (!session) {
        router.replace(`/login?from=${encodeURIComponent(safeInternalPath(from))}`);
        return;
      }

      try {
        const response = await apiFetch(`/api/student/courses/${slug}`);
        if (response.status === 401) {
          router.replace(`/login?from=${encodeURIComponent(safeInternalPath(from))}`);
          return;
        }

        if (response.status === 403) {
          setDenied(true);
          return;
        }

        if (response.status === 404) {
          setMissing(true);
          return;
        }

        setCourse(await parseJson<StudentCourseLearning>(response, "تعذر تحميل الدورة."));
      } catch (caught) {
        setError(caught instanceof ApiRequestError ? caught.message : "تعذر تحميل الدورة.");
      }
    });
  }, [router, slug]);

  if (denied) {
    return <AccessDenied title="ليس لديك صلاحية للوصول إلى هذه الدورة." />;
  }

  if (missing) {
    return <AccessDenied title="الدورة غير موجودة." description="تحقق من الرابط أو عد إلى قائمة دوراتك." />;
  }

  if (error) {
    return <p className="text-sm text-red-400">{error}</p>;
  }

  if (!course) {
    return <p className="text-sm text-muted">جاري تحميل الدورة...</p>;
  }

  const continueHref = course.continueLesson
    ? lessonHref(course.slug, course.continueLesson.lessonId)
    : course.sections[0]?.lessons[0]
      ? lessonHref(course.slug, course.sections[0].lessons[0].id)
      : null;

  return (
    <div className="grid gap-8 lg:grid-cols-[minmax(0,1fr)_18rem]">
      <div>
        <nav className="text-sm text-muted" aria-label="مسار التنقل">
          <Link href="/dashboard/courses" className="hover:text-foreground">
            دوراتي
          </Link>
          <span className="mx-2">/</span>
          <span className="text-foreground">{course.title}</span>
        </nav>
        <div className="mt-6 overflow-hidden border border-border">
          <CourseThumbnail title={course.title} src={course.thumbnailUrl} sizes="(max-width: 1024px) 100vw, 720px" />
        </div>
        <h1 className="mt-6 text-3xl font-semibold">{course.title}</h1>
        {course.courseCompleted ? <p className="mt-3 text-sm text-accent">أكملت الدورة</p> : null}
        <p className="mt-4 max-w-2xl text-sm leading-8 text-muted">{course.description}</p>
        <p className="mt-4 text-xs text-muted">بدأت في {formatBaghdadDateTime(course.startedAt)}</p>
        {course.expiresAt ? (
          <p className="mt-1 text-xs text-muted">تنتهي في {formatBaghdadDateTime(course.expiresAt)}</p>
        ) : null}
        {continueHref ? (
          <Link href={continueHref} className="mt-8 inline-flex border border-accent px-4 py-2 text-sm text-accent">
            {course.courseCompleted ? "مراجعة الدورة" : "متابعة التعلم"}
          </Link>
        ) : null}
        <div className="mt-10 lg:hidden">
          <CourseCurriculumNav
            courseTitle={course.title}
            courseSlug={course.slug}
            sections={course.sections}
            progressPercent={course.progressPercent}
            collapsible
          />
        </div>
      </div>
      <aside className="hidden lg:block">
        <CourseCurriculumNav
          courseTitle={course.title}
          courseSlug={course.slug}
          sections={course.sections}
          progressPercent={course.progressPercent}
        />
      </aside>
    </div>
  );
}
