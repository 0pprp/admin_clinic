import type { Metadata } from "next";
import Link from "next/link";
import { notFound, redirect } from "next/navigation";
import { cache } from "react";
import { BrandAccentLabel } from "@/components/brand/BrandAccentLabel";
import { FreePreviewPlayer } from "@/components/courses/FreePreviewPlayer";
import { Container } from "@/components/shared/Container";
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
  const { slug, lessonId } = await params;
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

  return (
    <Container className="py-16 sm:py-20">
      <Link href={`/courses/${encodeURIComponent(preview.courseSlug)}`} className="text-sm text-accent hover:underline">
        العودة إلى الكورس
      </Link>
      <BrandAccentLabel className="mt-8 text-base font-bold tracking-wide">معاينة مجانية</BrandAccentLabel>
      <h1 className="mt-4 max-w-3xl text-4xl font-semibold leading-tight">{preview.title}</h1>
      <p className="mt-3 text-sm text-muted">{formatDuration(preview.durationSeconds)}</p>
      {preview.description ? (
        <p className="mt-6 max-w-2xl text-base leading-8 text-muted">{preview.description}</p>
      ) : null}
      <div className="mt-10">
        <FreePreviewPlayer lessonId={preview.lessonId} title={preview.title} />
      </div>
    </Container>
  );
}
