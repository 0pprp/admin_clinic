import type { StatisticItem } from "@/lib/api/public-types";
import { Container } from "@/components/shared/Container";

export function StatisticsSection({ items }: { items: StatisticItem[] }) {
  if (items.length === 0) {
    return null;
  }

  return (
    <section className="bg-surface-warm" aria-label="مؤشرات المنصة">
      <Container className="grid gap-10 py-16 sm:grid-cols-2 lg:grid-cols-4">
        {items.map((item) => (
          <div key={item.id} className="text-center sm:text-start">
            <p className="text-3xl font-semibold tracking-tight">{item.displayValue}</p>
            <p className="mt-2 text-sm text-muted">{item.label}</p>
          </div>
        ))}
      </Container>
    </section>
  );
}
