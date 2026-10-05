import Link from "next/link";
import type { FaqItem } from "@/lib/api/public-types";
import { Container } from "@/components/shared/Container";
import { FaqAccordion } from "@/components/shared/FaqAccordion";
import { SectionHeading } from "@/components/shared/SectionHeading";

export function FaqSection({ items }: { items: FaqItem[] }) {
  if (items.length === 0) {
    return null;
  }

  return (
    <section className="bg-surface">
      <Container className="py-20">
        <div className="flex flex-wrap items-end justify-between gap-6">
          <SectionHeading eyebrow="الأسئلة" title="أسئلة متكررة." />
          <Link href="/faq" className="text-sm text-accent hover:underline">
            كل الأسئلة
          </Link>
        </div>
        <div className="mt-10">
          <FaqAccordion items={items} />
        </div>
      </Container>
    </section>
  );
}
