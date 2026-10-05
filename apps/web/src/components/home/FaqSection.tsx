import type { FaqItem } from "@/lib/api/public-types";
import { ButtonLink } from "@/components/ui/clinic";
import { Container } from "@/components/shared/Container";
import { FaqAccordion } from "@/components/shared/FaqAccordion";
import { SectionHeading } from "@/components/shared/SectionHeading";

export function FaqSection({ items }: { items: FaqItem[] }) {
  if (items.length === 0) {
    return null;
  }

  return (
    <section className="bg-surface">
      <Container className="py-16 sm:py-20">
        <div className="flex flex-wrap items-end justify-between gap-6">
          <SectionHeading eyebrow="الأسئلة" title="أسئلة متكررة." />
          <ButtonLink href="/faq" variant="ghost" size="sm" className="text-accent hover:text-accent">
            كل الأسئلة
          </ButtonLink>
        </div>
        <div className="mt-10">
          <FaqAccordion items={items} />
        </div>
      </Container>
    </section>
  );
}
