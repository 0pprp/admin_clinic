import Link from "next/link";
import type { CourseSummary } from "@/lib/api/public-types";
import { CourseCard } from "@/components/courses/CourseCard";
import { featuredCoursesCopy } from "@/lib/content/brand";
import { EmptyState } from "@/components/shared/EmptyState";

export function FeaturedCourses({ courses }: { courses: CourseSummary[] }) {
  return (
    <section className="clinic-shell py-10 sm:py-14">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div className="max-w-2xl">
          <h2 className="text-2xl font-extrabold tracking-tight sm:text-3xl">{featuredCoursesCopy.title}</h2>
          <p className="mt-2 text-sm leading-7 text-muted sm:text-base">{featuredCoursesCopy.description}</p>
        </div>
        <Link href="/courses" className="text-sm font-semibold text-accent hover:underline">
          كل الكورسات
        </Link>
      </div>
      {courses.length === 0 ? (
        <div className="mt-8">
          <EmptyState title="الكورسات ستتوفر قريباً" description="عندما تُنشر الكورسات المميزة ستظهر هنا مباشرة." />
        </div>
      ) : (
        <div className="mt-8 grid gap-5 md:grid-cols-2 xl:grid-cols-3">
          {courses.map((course) => (
            <CourseCard key={course.id} course={course} />
          ))}
        </div>
      )}
    </section>
  );
}
