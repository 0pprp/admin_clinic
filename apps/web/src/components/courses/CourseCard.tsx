import Image from "next/image";
import Link from "next/link";
import type { CourseSummary } from "@/lib/api/public-types";
import { BrandAccentLabel } from "@/components/brand/BrandAccentLabel";
import { courseLevelLabel, formatIqd } from "@/lib/format";

export function CourseCard({ course }: { course: CourseSummary }) {
  return (
    <article className="flex h-full flex-col border border-border bg-surface">
      <div className="relative aspect-[16/10] overflow-hidden bg-surface-warm">
        {course.thumbnailUrl ? (
          <Image
            src={course.thumbnailUrl}
            alt={`غلاف دورة ${course.title}`}
            fill
            className="object-cover"
            sizes="(max-width: 768px) 100vw, 33vw"
            unoptimized={!course.thumbnailUrl.startsWith("/")}
          />
        ) : (
          <div className="flex h-full items-end p-5">
            <BrandAccentLabel as="span" className="text-sm font-bold tracking-wide">
              كورس
            </BrandAccentLabel>
          </div>
        )}
      </div>
      <div className="flex flex-1 flex-col p-6">
        <p className="text-xs text-muted">{courseLevelLabel(course.level)}</p>
        <h3 className="mt-2 text-xl font-semibold leading-7">
          <Link href={`/courses/${course.slug}`} className="hover:text-accent">
            {course.title}
          </Link>
        </h3>
        <p className="mt-3 flex-1 text-sm leading-7 text-muted">{course.shortDescription}</p>
        <div className="mt-6 flex items-center justify-between gap-3">
          <p className="text-sm font-semibold">{formatIqd(course.priceIQD)}</p>
          <Link href={`/courses/${course.slug}`} className="text-sm text-accent hover:underline">
            تفاصيل الدورة
          </Link>
        </div>
      </div>
    </article>
  );
}
