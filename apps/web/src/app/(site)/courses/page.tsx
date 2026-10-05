import { CourseCard } from "@/components/courses/CourseCard";
import { Container } from "@/components/shared/Container";
import { EmptyState } from "@/components/shared/EmptyState";
import { PageIntro } from "@/components/shared/PageIntro";
import { Pagination } from "@/components/shared/Pagination";
import { fetchPublic } from "@/lib/api/public";
import type { CourseSummary, Paged } from "@/lib/api/public-types";
import { featuredCoursesCopy } from "@/lib/content/brand";
import { createPageMetadata } from "@/lib/seo";

export const dynamic = "force-dynamic";

export const metadata = createPageMetadata({
  title: "الكورسات",
  description: featuredCoursesCopy.description,
  path: "/courses"
});

export default async function CoursesPage({ searchParams }: PageProps<"/courses">) {
  const params = await searchParams;
  const page = Math.max(1, Number(params.page ?? "1") || 1);
  const data = await fetchPublic<Paged<CourseSummary>>(`/api/public/courses?page=${page}&pageSize=12`);
  const items = data?.items ?? [];

  return (
    <>
      <PageIntro eyebrow="الكورسات" title={featuredCoursesCopy.title} description={featuredCoursesCopy.description} />
      <Container className="py-12 sm:py-16">
        {items.length === 0 ? (
          <EmptyState title="الكورسات ستتوفر قريباً" description="لا توجد كورسات منشورة للعرض في الوقت الحالي." />
        ) : (
          <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-3">
            {items.map((course) => (
              <CourseCard key={course.id} course={course} />
            ))}
          </div>
        )}
        {data ? (
          <Pagination page={data.page} pageSize={data.pageSize} totalCount={data.totalCount} basePath="/courses" />
        ) : null}
      </Container>
    </>
  );
}
