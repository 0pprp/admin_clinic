import type { StatisticItem } from "@/lib/api/public-types";
import { Metric } from "@/components/ui/clinic";
import { Container } from "@/components/shared/Container";

export function StatisticsSection({ items }: { items: StatisticItem[] }) {
  if (items.length === 0) {
    return null;
  }

  return (
    <section className="bg-surface-warm" aria-label="مؤشرات المنصة">
      <Container className="grid gap-4 py-14 sm:grid-cols-2 sm:gap-5 lg:grid-cols-4 lg:py-16">
        {items.map((item) => (
          <Metric key={item.id} value={item.displayValue} label={item.label} />
        ))}
      </Container>
    </section>
  );
}
