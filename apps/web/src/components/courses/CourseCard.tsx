import Image from "next/image";
import Link from "next/link";
import type { CourseSummary } from "@/lib/api/public-types";
import { Badge } from "@/components/ui/clinic";
import { courseLevelLabel, formatIqd } from "@/lib/format";

export function CourseCard({ course }: { course: CourseSummary }) {
  return (
    <article className="clinic-card flex h-full flex-col overflow-hidden transition hover:border-accent/40 hover:shadow-[0_12px_40px_rgba(7,27,51,0.08)]">
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
            <Badge tone="orange">كورس</Badge>
          </div>
        )}
      </div>
      <div className="flex flex-1 flex-col p-5 sm:p-6">
        <Badge tone="navy">{courseLevelLabel(course.level)}</Badge>
        <h3 className="mt-3 text-xl font-semibold leading-7">
          <Link href={`/courses/${course.slug}`} className="hover:text-accent">
            {course.title}
          </Link>
        </h3>
        <p className="mt-3 flex-1 text-sm leading-7 text-muted">{course.shortDescription}</p>
        <div className="mt-6 flex items-center justify-between gap-3 border-t border-border pt-4">
          <p className="text-sm font-semibold text-primary">{formatIqd(course.priceIQD)}</p>
          <Link href={`/courses/${course.slug}`} className="text-sm font-medium text-accent hover:underline">
            تفاصيل الدورة
          </Link>
        </div>
      </div>
    </article>
  );
}
