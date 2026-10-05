import type { TestimonialItem } from "@/lib/api/public-types";
import { Container } from "@/components/shared/Container";
import { SectionHeading } from "@/components/shared/SectionHeading";

export function TestimonialsSection({ items }: { items: TestimonialItem[] }) {
  if (items.length === 0) {
    return null;
  }

  return (
    <section className="bg-background">
      <Container className="py-20">
        <SectionHeading eyebrow="آراء" title="كلمات ممن جربوا العمل معنا." />
        <ul className="mt-12 grid gap-8 lg:grid-cols-3">
          {items.map((item) => (
            <li key={item.id} className="bg-surface p-8">
              <p className="text-base leading-8">{item.body}</p>
              <p className="mt-6 text-sm font-semibold">{item.authorDisplayName}</p>
              {item.authorTitle ? <p className="mt-1 text-xs text-muted">{item.authorTitle}</p> : null}
            </li>
          ))}
        </ul>
      </Container>
    </section>
  );
}
