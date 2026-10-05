import Link from "next/link";
import { CourseCard } from "@/components/courses/CourseCard";
import { EmptyState } from "@/components/shared/EmptyState";
import { Pagination } from "@/components/shared/Pagination";
import { fetchPublic } from "@/lib/api/public";
import type { CourseSummary, Paged } from "@/lib/api/public-types";
import { createPageMetadata } from "@/lib/seo";

export const dynamic = "force-dynamic";

export const metadata = createPageMetadata({
  title: "الكورسات",
  description: "تصفّح كورسات العيادة الإدارية العملية للإدارة والقيادة.",
  path: "/courses"
});

/** P02 · الكورسات */
export default async function CoursesPage({ searchParams }: PageProps<"/courses">) {
  const params = await searchParams;
  const page = Math.max(1, Number(params.page ?? "1") || 1);
  const q = typeof params.q === "string" ? params.q.trim() : "";
  const data = await fetchPublic<Paged<CourseSummary>>(`/api/public/courses?page=${page}&pageSize=12`);
  const items = (data?.items ?? []).filter((course) => {
    if (!q) return true;
    const hay = `${course.title} ${course.shortDescription}`.toLowerCase();
    return hay.includes(q.toLowerCase());
  });

  return (
    <div className="clinic-shell py-10 sm:py-14">
      <div className="max-w-3xl">
        <p className="text-sm font-bold text-accent">الكورسات</p>
        <h1 className="mt-3 text-3xl font-extrabold tracking-tight sm:text-4xl">ابدأ من التحدي الذي تواجهه</h1>
        <p className="mt-3 text-sm leading-8 text-muted sm:text-base">كورسات مصممة لواقع العمل والإدارة اليومية</p>
      </div>

      <form action="/courses" method="get" className="mt-8 flex flex-col gap-3 sm:flex-row sm:items-center">
        <input
          type="search"
          name="q"
          defaultValue={q}
          placeholder="ابحث عن كورس..."
          className="h-12 w-full flex-1 rounded-xl border border-border bg-surface px-4 text-sm outline-none transition focus:border-accent"
        />
        <button
          type="submit"
          className="inline-flex h-12 items-center justify-center rounded-xl bg-accent px-6 text-sm font-semibold text-primary-foreground transition hover:bg-accent-soft"
        >
          بحث
        </button>
      </form>

      {items.length === 0 ? (
        <div className="mt-10">
          <EmptyState title="لا توجد نتائج" description="جرّب كلمات بحث أخرى أو تصفّح كل الكورسات." />
          {q ? (
            <p className="mt-4 text-sm">
              <Link href="/courses" className="font-semibold text-accent hover:underline">
                عرض كل الكورسات
              </Link>
            </p>
          ) : null}
        </div>
      ) : (
        <div className="mt-10 grid gap-5 md:grid-cols-2 xl:grid-cols-3">
          {items.map((course) => (
            <CourseCard key={course.id} course={course} />
          ))}
        </div>
      )}

      {!q && data ? (
        <Pagination page={data.page} pageSize={data.pageSize} totalCount={data.totalCount} basePath="/courses" />
      ) : null}
    </div>
  );
}
