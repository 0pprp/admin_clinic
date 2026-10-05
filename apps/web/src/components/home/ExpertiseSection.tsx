import type { ExpertiseItem } from "@/lib/api/public-types";
import { BrandAccentLabel } from "@/components/brand/BrandAccentLabel";
import { Container } from "@/components/shared/Container";
import { EmptyState } from "@/components/shared/EmptyState";
import { SectionHeading } from "@/components/shared/SectionHeading";

export function ExpertiseSection({ items }: { items: ExpertiseItem[] }) {
  return (
    <section className="bg-surface">
      <Container className="py-20">
        <SectionHeading eyebrow="الخبرة" title="مجالات نعمل عليها بوضوح." />
        {items.length === 0 ? (
          <div className="mt-10">
            <EmptyState
              title="مجالات الخبرة ستُعرض هنا"
              description="عند اعتماد عناصر Expertise النشطة، تظهر في هذا القسم تلقائياً. النص الحالي Placeholder للتصميم."
            />
          </div>
        ) : (
          <ul className="mt-12 grid gap-10 sm:grid-cols-2">
            {items.map((item, index) => (
              <li key={item.id} className="border-t border-border pt-6">
                <BrandAccentLabel className="text-sm font-bold tracking-wide">
                  {String(index + 1).padStart(2, "0")}
                </BrandAccentLabel>
                <h3 className="mt-3 text-2xl font-semibold">{item.title}</h3>
                <p className="mt-3 text-sm leading-8 text-muted">{item.description}</p>
              </li>
            ))}
          </ul>
        )}
      </Container>
    </section>
  );
}
