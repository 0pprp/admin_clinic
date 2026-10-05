import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { Container } from "@/components/shared/Container";
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
    <Container className="py-16 sm:py-20">
      {published ? <p className="text-sm text-muted">{published}</p> : null}
      <h1 className="mt-4 max-w-3xl text-4xl font-semibold leading-tight sm:text-5xl">{article.title}</h1>
      <p className="mt-5 max-w-2xl text-lg leading-8 text-muted">{article.excerpt}</p>
      <article className="mt-12 max-w-3xl whitespace-pre-line text-base leading-9">{article.content}</article>
    </Container>
  );
}
