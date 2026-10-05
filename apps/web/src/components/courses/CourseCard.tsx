import Link from "next/link";
import type { CourseSummary } from "@/lib/api/public-types";
import { brand } from "@/lib/content/brand";
import { courseLevelLabel, formatIqd } from "@/lib/format";

export function CourseCard({ course }: { course: CourseSummary }) {
  return (
    <article className="clinic-card flex h-full flex-col overflow-hidden">
      <div className="relative flex aspect-[16/11] items-center justify-center bg-surface-dark px-6 text-center text-primary-foreground">
        <div>
          <p className="text-lg font-extrabold leading-8 sm:text-xl">{course.title}</p>
          <p className="mt-3 text-sm font-bold text-accent">{brand.nameAr}</p>
        </div>
      </div>
      <div className="flex flex-1 flex-col p-5">
        <h3 className="text-lg font-bold leading-7">
          <Link href={`/courses/${course.slug}`} className="hover:text-accent">
            {course.title}
          </Link>
        </h3>
        <p className="mt-2 text-xs text-muted">
          مسجّلة • مستوى {courseLevelLabel(course.level)}
        </p>
        <p className="mt-3 flex-1 text-sm leading-7 text-muted">{course.shortDescription}</p>
        <p className="mt-5 text-base font-extrabold text-foreground">{formatIqd(course.priceIQD)}</p>
      </div>
    </article>
  );
}
