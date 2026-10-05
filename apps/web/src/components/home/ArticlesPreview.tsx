import type { ArticleSummary } from "@/lib/api/public-types";
import { ArticleCard } from "@/components/articles/ArticleCard";
import { ButtonLink } from "@/components/ui/clinic";
import { Container } from "@/components/shared/Container";
import { EmptyState } from "@/components/shared/EmptyState";
import { SectionHeading } from "@/components/shared/SectionHeading";

export function ArticlesPreview({ articles }: { articles: ArticleSummary[] }) {
  return (
    <section className="bg-surface">
      <Container className="py-16 sm:py-20">
        <div className="flex flex-wrap items-end justify-between gap-6">
          <SectionHeading eyebrow="المقالات" title="أفكار للقراءة بهدوء." />
          <ButtonLink href="/articles" variant="ghost" size="sm" className="text-accent hover:text-accent">
            كل المقالات
          </ButtonLink>
        </div>
        {articles.length === 0 ? (
          <div className="mt-10">
            <EmptyState title="المقالات ستُنشر قريباً" description="عند اعتماد المقالات المنشورة ستظهر أحدث ثلاث مقالات هنا." />
          </div>
        ) : (
          <div className="mt-12 grid gap-6 lg:grid-cols-3 lg:gap-8">
            {articles.map((article) => (
              <ArticleCard key={article.id} article={article} />
            ))}
          </div>
        )}
      </Container>
    </section>
  );
}
