import type { Metadata } from "next";
import Link from "next/link";
import { notFound, redirect } from "next/navigation";
import { cache } from "react";
import { FreePreviewPlayer } from "@/components/courses/FreePreviewPlayer";
import { Container } from "@/components/shared/Container";
import { PageIntro } from "@/components/shared/PageIntro";
import { fetchPublic, normalizeRouteSlug } from "@/lib/api/public";
import type { LessonPreview } from "@/lib/api/public-types";
import { formatDuration } from "@/lib/format";
import { createPageMetadata } from "@/lib/seo";

export const dynamic = "force-dynamic";

const loadPreview = cache(async (lessonId: string) =>
  fetchPublic<LessonPreview>(`/api/public/lessons/${lessonId}/preview`, { fresh: true })
);

async function resolvePreview(slug: string, lessonId: string) {
  const preview = await loadPreview(lessonId);
  if (!preview) {
    return null;
  }

  if (normalizeRouteSlug(preview.courseSlug) !== normalizeRouteSlug(slug)) {
    redirect(`/courses/${encodeURIComponent(preview.courseSlug)}/preview/${lessonId}`);
  }

  return preview;
}

export async function generateMetadata({
  params
}: PageProps<"/courses/[slug]/preview/[lessonId]">): Promise<Metadata> {
  const { lessonId } = await params;
  const preview = await loadPreview(lessonId);
  if (!preview) {
    return { title: "معاينة غير متاحة" };
  }

  return createPageMetadata({
    title: preview.title,
    description: preview.description ?? "معاينة مجانية من منهج الدورة.",
    path: `/courses/${preview.courseSlug}/preview/${lessonId}`
  });
}

export default async function LessonPreviewPage({
  params
}: PageProps<"/courses/[slug]/preview/[lessonId]">) {
  const { slug, lessonId } = await params;
  const preview = await resolvePreview(slug, lessonId);
  if (!preview) {
    notFound();
  }

  const introDescription = [
    formatDuration(preview.durationSeconds),
    preview.description
  ]
    .filter(Boolean)
    .join(" · ");

  return (
    <>
      <PageIntro eyebrow="معاينة مجانية" title={preview.title} description={introDescription || undefined} />
      <Container className="py-12 sm:py-16">
        <Link
          href={`/courses/${encodeURIComponent(preview.courseSlug)}`}
          className="text-sm font-medium text-accent hover:underline"
        >
          العودة إلى الكورس
        </Link>
        <div className="clinic-card mt-8 overflow-hidden p-2 sm:p-3">
          <FreePreviewPlayer lessonId={preview.lessonId} title={preview.title} />
        </div>
      </Container>
    </>
  );
}
