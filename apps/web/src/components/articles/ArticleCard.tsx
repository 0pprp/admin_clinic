import Image from "next/image";
import Link from "next/link";
import type { ArticleSummary } from "@/lib/api/public-types";
import { formatDate } from "@/lib/format";

export function ArticleCard({ article }: { article: ArticleSummary }) {
  const published = formatDate(article.publishedAt);

  return (
    <article className="clinic-card flex h-full flex-col overflow-hidden transition hover:border-accent/40">
      <div className="relative aspect-[16/10] overflow-hidden bg-surface-warm">
        {article.coverImage ? (
          <Image
            src={article.coverImage}
            alt={`غلاف مقال ${article.title}`}
            fill
            className="object-cover"
            sizes="(max-width: 768px) 100vw, 33vw"
            unoptimized={!article.coverImage.startsWith("/")}
          />
        ) : (
          <div className="flex h-full items-end bg-surface-dark px-5 py-5">
            <p className="text-sm font-bold text-accent">مقال</p>
          </div>
        )}
      </div>
      <div className="flex flex-1 flex-col p-5">
        {published ? <p className="text-xs font-semibold text-muted">{published}</p> : null}
        <h3 className="mt-2 text-lg font-bold leading-7">
          <Link href={`/articles/${article.slug}`} className="hover:text-accent">
            {article.title}
          </Link>
        </h3>
        <p className="mt-3 flex-1 text-sm leading-7 text-muted">{article.excerpt}</p>
        <Link
          href={`/articles/${article.slug}`}
          className="mt-5 inline-flex text-sm font-semibold text-accent transition hover:text-accent-soft"
        >
          اقرأ المقال
        </Link>
      </div>
    </article>
  );
}
