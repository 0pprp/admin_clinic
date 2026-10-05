import { positioningAreas } from "@/lib/content/brand";
import type { ExpertiseItem } from "@/lib/api/public-types";
import { Container } from "@/components/shared/Container";

export function TrustStrip({ expertise }: { expertise: ExpertiseItem[] }) {
  const items = expertise.length > 0 ? expertise.map((item) => item.title) : [...positioningAreas];

  return (
    <section className="border-y border-border bg-surface" aria-label="مجالات المنصة">
      <Container className="py-6 sm:py-8">
        <div className="clinic-panel flex flex-wrap items-center justify-center gap-x-6 gap-y-3 px-5 py-4 text-sm text-muted">
          {items.map((item, index) => (
            <span key={item} className="inline-flex items-center gap-6">
              <span className="font-medium text-foreground">{item}</span>
              {index < items.length - 1 ? (
                <span className="hidden text-accent sm:inline" aria-hidden="true">
                  •
                </span>
              ) : null}
            </span>
          ))}
        </div>
      </Container>
    </section>
  );
}
