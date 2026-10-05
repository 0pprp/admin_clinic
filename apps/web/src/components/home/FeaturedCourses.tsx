import Link from "next/link";
import type { CourseSummary } from "@/lib/api/public-types";
import { CourseCard } from "@/components/courses/CourseCard";
import { Container } from "@/components/shared/Container";
import { EmptyState } from "@/components/shared/EmptyState";
import { SectionHeading } from "@/components/shared/SectionHeading";

export function FeaturedCourses({ courses }: { courses: CourseSummary[] }) {
  return (
    <section className="bg-background">
      <Container className="py-20">
        <div className="flex flex-wrap items-end justify-between gap-6">
          <SectionHeading eyebrow="الكورسات" title="محتوى مرتّب للتطبيق." />
          <Link href="/courses" className="text-sm text-accent hover:underline">
            كل الكورسات
          </Link>
        </div>
        {courses.length === 0 ? (
          <div className="mt-10">
            <EmptyState title="الكورسات ستتوفر قريباً" description="عندما تُنشر الكورسات المميزة ستظهر هنا مباشرة." />
          </div>
        ) : (
          <div className="mt-12 grid gap-6 md:grid-cols-2 xl:grid-cols-3">
            {courses.map((course) => (
              <CourseCard key={course.id} course={course} />
            ))}
          </div>
        )}
      </Container>
    </section>
  );
}
