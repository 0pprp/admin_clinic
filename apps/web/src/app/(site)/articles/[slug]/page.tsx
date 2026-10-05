import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { Container } from "@/components/shared/Container";
import { PageIntro } from "@/components/shared/PageIntro";
import { fetchPublic } from "@/lib/api/public";
import type { ArticleDetail } from "@/lib/api/public-types";
import { formatDate } from "@/lib/format";
import { createPageMetadata } from "@/lib/seo";

export async function generateMetadata({ params }: PageProps<"/articles/[slug]">): Promise<Metadata> {
  const { slug } = await params;
  const article = await fetchPublic<ArticleDetail>(`/api/public/articles/${slug}`);
  if (!article) {
    return createPageMetadata({
      title: "المقال غير متاح",
      description: "هذا المقال غير متاح للعرض العام.",
      path: `/articles/${slug}`
    });
  }

  return createPageMetadata({
    title: article.title,
    description: article.excerpt,
    path: `/articles/${slug}`,
    type: "article"
  });
}

export default async function ArticleDetailPage({ params }: PageProps<"/articles/[slug]">) {
  const { slug } = await params;
  const article = await fetchPublic<ArticleDetail>(`/api/public/articles/${slug}`);
  if (!article) {
    notFound();
  }

  const published = formatDate(article.publishedAt);

  return (
    <>
      <PageIntro
        eyebrow={published ? `نُشر ${published}` : "المقالات"}
        title={article.title}
        description={article.excerpt}
      />
      <Container className="py-12 sm:py-16">
        <article className="clinic-card max-w-3xl whitespace-pre-line px-6 py-8 text-base leading-9 sm:px-8">
          {article.content}
        </article>
      </Container>
    </>
  );
}
