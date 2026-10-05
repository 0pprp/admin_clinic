"use client";

import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { AccessDenied } from "@/components/dashboard/AccessDenied";
import { CourseCurriculumNav } from "@/components/learning/CourseCurriculumNav";
import { HlsLessonPlayer } from "@/components/learning/HlsLessonPlayer";
import { ApiRequestError, apiFetch, parseJson } from "@/lib/api/client";
import { getCurrentUser } from "@/lib/auth/session";
import { formatDuration } from "@/lib/format";
import { lessonHref, type LessonPlayback, type StudentLessonLearning } from "@/lib/learning";
import { safeInternalPath } from "@/lib/safe-path";

function isTrustedBunnyUrl(url: string): boolean {
  try {
    const parsed = new URL(url);
    return parsed.protocol === "https:" && parsed.hostname === "iframe.mediadelivery.net";
  } catch {
    return false;
  }
}

function LessonPlayer({ playback, title }: { playback: LessonPlayback | null; title: string }) {
  if (playback && !playback.playbackUnavailable && playback.kind === "hls" && playback.playbackUrl) {
    return <HlsLessonPlayer src={playback.playbackUrl} title={title} />;
  }

  if (playback && !playback.playbackUnavailable && playback.kind === "iframe" && playback.playbackUrl && isTrustedBunnyUrl(playback.playbackUrl)) {
    return (
      <iframe
        src={playback.playbackUrl}
        title={title}
        className="h-full w-full"
        allow="accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture"
        allowFullScreen
      />
    );
  }

  return (
    <div className="flex h-full items-center justify-center bg-surface px-6 text-center">
      <p className="max-w-md text-sm leading-8 text-muted">
        {playback?.message ??
          "لا يوجد فيديو جاهز لهذا الدرس. من لوحة الإدارة → الكورسات → عدّل الدرس وارفع ملف الفيديو ثم انتظر انتهاء المعالجة."}
      </p>
    </div>
  );
}

export default function StudentLessonPage() {
  const router = useRouter();
  const params = useParams<{ slug: string; lessonId: string }>();
  const [lesson, setLesson] = useState<StudentLessonLearning | null>(null);
  const [playback, setPlayback] = useState<LessonPlayback | null>(null);
  const [denied, setDenied] = useState(false);
  const [missing, setMissing] = useState(false);
  const [error, setError] = useState("");
  const [progressError, setProgressError] = useState("");
  const [pending, setPending] = useState(false);

  useEffect(() => {
    if (!params.slug || !params.lessonId) {
      return;
    }

    const from = `/dashboard/courses/${params.slug}/lessons/${params.lessonId}`;
    getCurrentUser().then(async (session) => {
      if (!session) {
        router.replace(`/login?from=${encodeURIComponent(safeInternalPath(from))}`);
        return;
      }

      try {
        const response = await apiFetch(`/api/student/courses/${params.slug}/lessons/${params.lessonId}`);
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

        const body = await parseJson<StudentLessonLearning>(response, "تعذر تحميل الدرس.");
        setLesson(body);
        const play = await apiFetch(`/api/lessons/${params.lessonId}/playback`);
        if (play.ok) {
          setPlayback(await play.json());
        }
      } catch (caught) {
        setError(caught instanceof ApiRequestError ? caught.message : "تعذر تحميل الدرس.");
      }
    });
  }, [params.lessonId, params.slug, router]);

  async function markComplete() {
    if (!lesson || pending) {
      return;
    }

    setProgressError("");
    setPending(true);
    try {
      const response = await apiFetch(`/api/progress/${lesson.id}/complete`, { method: "POST" });
      await parseJson(response, "تعذر تحديث تقدم الدرس.");
      const refreshed = await apiFetch(`/api/student/courses/${lesson.courseSlug}/lessons/${lesson.id}`);
      setLesson(await parseJson<StudentLessonLearning>(refreshed, "تعذر تحديث تقدم الدرس."));
    } catch (caught) {
      setProgressError(caught instanceof ApiRequestError ? caught.message : "تعذر تحديث تقدم الدرس.");
    } finally {
      setPending(false);
    }
  }

  if (denied) {
    return <AccessDenied />;
  }

  if (missing) {
    return <AccessDenied title="الدرس غير موجود." description="قد يكون الرابط غير صحيح أو الدرس من دورة أخرى." />;
  }

  if (error) {
    return <p className="text-sm text-red-400">{error}</p>;
  }

  if (!lesson) {
    return <p className="text-sm text-muted">جاري تحميل الدرس...</p>;
  }

  const nextHref = lesson.nextLesson ? lessonHref(lesson.courseSlug, lesson.nextLesson.id) : `/dashboard/courses/${lesson.courseSlug}`;
  const allComplete =
    lesson.sections.flatMap((section) => section.lessons).every((item) => item.isCompleted) &&
    lesson.sections.some((section) => section.lessons.length > 0);

  return (
    <div className="grid gap-8 lg:grid-cols-[minmax(0,1fr)_18rem]">
      <div>
        <nav className="text-sm text-muted" aria-label="مسار التنقل">
          <Link href="/dashboard/courses" className="hover:text-foreground">
            دوراتي
          </Link>
          <span className="mx-2">/</span>
          <Link href={`/dashboard/courses/${lesson.courseSlug}`} className="hover:text-foreground">
            {lesson.courseTitle}
          </Link>
          <span className="mx-2">/</span>
          <span className="text-foreground">{lesson.title}</span>
        </nav>
        <div className="mt-6 overflow-hidden border border-border bg-black">
          <div className="aspect-video">
            <LessonPlayer playback={playback} title={lesson.title} />
          </div>
        </div>
        <h1 className="mt-6 text-3xl font-semibold">{lesson.title}</h1>
        <p className="mt-2 text-sm text-muted">{formatDuration(lesson.durationSeconds)}</p>
        {lesson.description ? <p className="mt-4 max-w-2xl text-sm leading-8 text-muted">{lesson.description}</p> : null}
        <div className="mt-8 flex flex-wrap items-center gap-3">
          {lesson.isCompleted ? (
            <p className="text-sm text-accent">✓ مكتمل</p>
          ) : (
            <button
              type="button"
              onClick={markComplete}
              disabled={pending}
              className="border border-accent bg-accent px-4 py-2 text-sm text-background disabled:opacity-60"
            >
              {pending ? "جاري الحفظ..." : "تحديد كمكتمل"}
            </button>
          )}
          {progressError ? <p className="text-sm text-red-400">{progressError}</p> : null}
        </div>
        <div className="mt-8 flex flex-wrap gap-3 text-sm">
          {lesson.previousLesson ? (
            <Link href={lessonHref(lesson.courseSlug, lesson.previousLesson.id)} className="border border-border px-4 py-2">
              الدرس السابق
            </Link>
          ) : null}
          {allComplete ? (
            <p className="border border-accent px-4 py-2 text-accent">أكملت الدورة</p>
          ) : lesson.nextLesson ? (
            <Link href={nextHref} className="border border-accent px-4 py-2 text-accent">
              الدرس التالي
            </Link>
          ) : (
            <Link href={`/dashboard/courses/${lesson.courseSlug}`} className="border border-accent px-4 py-2 text-accent">
              العودة إلى الدورة
            </Link>
          )}
        </div>
        <div className="mt-10 lg:hidden">
          <CourseCurriculumNav
            courseTitle={lesson.courseTitle}
            courseSlug={lesson.courseSlug}
            sections={lesson.sections}
            progressPercent={lesson.progressPercent}
            currentLessonId={lesson.id}
            collapsible
          />
        </div>
      </div>
      <aside className="hidden lg:block">
        <CourseCurriculumNav
          courseTitle={lesson.courseTitle}
          courseSlug={lesson.courseSlug}
          sections={lesson.sections}
          progressPercent={lesson.progressPercent}
          currentLessonId={lesson.id}
        />
      </aside>
    </div>
  );
}
