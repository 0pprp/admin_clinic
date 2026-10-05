import Image from "next/image";
import Link from "next/link";
import type { ArticleSummary } from "@/lib/api/public-types";
import { ButtonLink } from "@/components/ui/clinic";
import { formatDate } from "@/lib/format";

export function ArticleCard({ article }: { article: ArticleSummary }) {
  const published = formatDate(article.publishedAt);

  return (
    <article className="clinic-card flex h-full flex-col overflow-hidden transition hover:border-accent/40 hover:shadow-[0_12px_40px_rgba(7,27,51,0.08)]">
      {article.coverImage ? (
        <div className="relative aspect-[16/9] overflow-hidden bg-surface-warm">
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
      <div className="flex flex-1 flex-col p-5 sm:p-6">
        {published ? <p className="text-xs text-muted">{published}</p> : null}
        <h3 className="mt-2 text-xl font-semibold leading-8 sm:text-2xl">
          <Link href={`/articles/${article.slug}`} className="hover:text-accent">
            {article.title}
          </Link>
        </h3>
        <p className="mt-3 flex-1 text-sm leading-7 text-muted">{article.excerpt}</p>
        <ButtonLink href={`/articles/${article.slug}`} variant="ghost" size="sm" className="mt-4 w-fit px-0 text-accent hover:bg-transparent">
          اقرأ المقال
        </ButtonLink>
      </div>
    </article>
  );
}
