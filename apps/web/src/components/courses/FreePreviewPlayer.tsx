"use client";

import { useEffect, useState } from "react";
import { HlsLessonPlayer } from "@/components/learning/HlsLessonPlayer";
import { apiFetch } from "@/lib/api/client";
import type { LessonPlayback } from "@/lib/learning";

export function FreePreviewPlayer({ lessonId, title }: { lessonId: string; title: string }) {
  const [playback, setPlayback] = useState<LessonPlayback | null>(null);
  const [error, setError] = useState("");

  useEffect(() => {
    let cancelled = false;

    async function load() {
      try {
        const response = await apiFetch(`/api/lessons/${lessonId}/playback`);
        if (!response.ok) {
          throw new Error("تعذر تحميل المعاينة.");
        }
        const data = (await response.json()) as LessonPlayback;
        if (!cancelled) {
          setPlayback(data);
        }
      } catch (caught) {
        if (!cancelled) {
          setError(caught instanceof Error ? caught.message : "تعذر تحميل المعاينة.");
        }
      }
    }

    void load();
    return () => {
      cancelled = true;
    };
  }, [lessonId]);

  if (error) {
    return (
      <div className="flex aspect-video items-center justify-center border border-border bg-surface px-6 text-center">
        <p className="max-w-md text-sm leading-8 text-muted">{error}</p>
      </div>
    );
  }

  if (!playback) {
    return (
      <div className="flex aspect-video items-center justify-center border border-border bg-surface px-6 text-center">
        <p className="text-sm text-muted">جاري تحميل المعاينة...</p>
      </div>
    );
  }

  if (playback.playbackUnavailable || playback.kind !== "hls" || !playback.playbackUrl) {
    return (
      <div className="flex aspect-video items-center justify-center border border-border bg-surface px-6 text-center">
        <p className="max-w-md text-sm leading-8 text-muted">
          {playback.message || "الفيديو غير جاهز للمعاينة بعد."}
        </p>
      </div>
    );
  }

  return (
    <div className="aspect-video overflow-hidden border border-border bg-black">
      <HlsLessonPlayer src={playback.playbackUrl} title={title} />
    </div>
  );
}
