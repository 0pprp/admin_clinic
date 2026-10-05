import Link from "next/link";
import type { CourseSummary } from "@/lib/api/public-types";
import { brand } from "@/lib/content/brand";
import { courseLevelLabel, formatIqd } from "@/lib/format";

export function CourseCard({ course }: { course: CourseSummary }) {
  return (
    <article className="clinic-card flex h-full flex-col overflow-hidden">
      <div className="relative flex aspect-[16/10] flex-col justify-end bg-surface-dark px-5 py-5 text-primary-foreground sm:px-6 sm:py-6">
        <p className="text-base font-extrabold leading-8 sm:text-lg">من مدير مشغول إلى قائد مؤثر</p>
        <p className="mt-2 text-sm font-bold text-accent">{brand.nameAr}</p>
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
