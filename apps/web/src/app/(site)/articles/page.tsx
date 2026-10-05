import { ArticleCard } from "@/components/articles/ArticleCard";
import { EmptyState } from "@/components/shared/EmptyState";
import { PageIntro } from "@/components/shared/PageIntro";
import { Pagination } from "@/components/shared/Pagination";
import { fetchPublic } from "@/lib/api/public";
import type { ArticleSummary, Paged } from "@/lib/api/public-types";
import { createPageMetadata } from "@/lib/seo";

export const metadata = createPageMetadata({
  title: "المقالات",
  description: "مقالات العيادة الإدارية المنشورة للقراءة العامة.",
  path: "/articles"
});

/** P12 · شبكة المقالات */
export default async function ArticlesPage({ searchParams }: PageProps<"/articles">) {
  const params = await searchParams;
  const page = Math.max(1, Number(params.page ?? "1") || 1);
  const data = await fetchPublic<Paged<ArticleSummary>>(`/api/public/articles?page=${page}&pageSize=12`);
  const items = data?.items ?? [];

  return (
    <div className="clinic-shell py-10 sm:py-14">
      <PageIntro
        embedded
        eyebrow="المقالات"
        title="قراءة لترتيب التفكير."
        description="تظهر المقالات المنشورة فقط، بعد حلول موعد نشرها."
      />

      {items.length === 0 ? (
        <div className="mt-10">
          <EmptyState title="لا توجد مقالات منشورة بعد" description="عند نشر المقالات المعتمدة ستظهر في هذا القسم." />
        </div>
      ) : (
        <div className="mt-10 grid gap-5 md:grid-cols-2 xl:grid-cols-3">
          {items.map((article) => (
            <ArticleCard key={article.id} article={article} />
          ))}
        </div>
      )}

      {data ? (
        <Pagination page={data.page} pageSize={data.pageSize} totalCount={data.totalCount} basePath="/articles" />
      ) : null}
    </div>
  );
}
