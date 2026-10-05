import Image from "next/image";
import Link from "next/link";
import type { ArticleSummary } from "@/lib/api/public-types";
import { formatDate } from "@/lib/format";

export function ArticleCard({ article }: { article: ArticleSummary }) {
  const published = formatDate(article.publishedAt);

  return (
    <article className="flex h-full flex-col border-t border-border pt-6">
      {article.coverImage ? (
        <div className="relative mb-5 aspect-[16/9] overflow-hidden bg-surface-warm">
          <Image
            src={article.coverImage}
            alt={`غلاف مقال ${article.title}`}
            fill
            className="object-cover"
            sizes="(max-width: 768px) 100vw, 33vw"
            unoptimized={!article.coverImage.startsWith("/")}
          />
        </div>
      ) : null}
      {published ? <p className="text-xs text-muted">{published}</p> : null}
      <h3 className="mt-2 text-2xl font-semibold leading-8">
        <Link href={`/articles/${article.slug}`} className="hover:text-accent">
          {article.title}
        </Link>
      </h3>
      <p className="mt-3 text-sm leading-7 text-muted">{article.excerpt}</p>
      <Link href={`/articles/${article.slug}`} className="mt-4 text-sm text-accent hover:underline">
        اقرأ المقال
      </Link>
    </article>
  );
}
