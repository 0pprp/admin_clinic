import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
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

/** P15 · قراءة المقال */
export default async function ArticleDetailPage({ params }: PageProps<"/articles/[slug]">) {
  const { slug } = await params;
  const article = await fetchPublic<ArticleDetail>(`/api/public/articles/${slug}`);
  if (!article) {
    notFound();
  }

  const published = formatDate(article.publishedAt);

  return (
    <div className="clinic-shell py-10 sm:py-14">
      <nav className="text-xs text-muted" aria-label="مسار التنقل">
        <Link href="/" className="hover:text-accent">
          الرئيسية
        </Link>
        <span aria-hidden="true"> / </span>
        <Link href="/articles" className="hover:text-accent">
          المقالات
        </Link>
        <span aria-hidden="true"> / </span>
        <span className="text-foreground">{article.title}</span>
      </nav>

      <article className="mx-auto mt-8 max-w-3xl">
        <header>
          {published ? <p className="text-sm font-bold text-accent">نُشر {published}</p> : <p className="text-sm font-bold text-accent">المقالات</p>}
          <h1 className="mt-3 text-3xl font-extrabold leading-tight tracking-tight sm:text-4xl">{article.title}</h1>
          {article.excerpt ? <p className="mt-4 text-base leading-8 text-muted">{article.excerpt}</p> : null}
        </header>

        <div className="clinic-card mt-8 whitespace-pre-line px-6 py-8 text-base leading-9 text-foreground sm:px-10 sm:py-10">
          {article.content}
        </div>

        <p className="mt-8">
          <Link href="/articles" className="text-sm font-semibold text-accent hover:underline">
            العودة إلى المقالات
          </Link>
        </p>
      </article>
    </div>
  );
}
