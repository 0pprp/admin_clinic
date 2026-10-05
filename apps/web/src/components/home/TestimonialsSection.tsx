import type { TestimonialItem } from "@/lib/api/public-types";
import { FeedbackCard } from "@/components/ui/clinic";
import { Container } from "@/components/shared/Container";
import { SectionHeading } from "@/components/shared/SectionHeading";

export function TestimonialsSection({ items }: { items: TestimonialItem[] }) {
  if (items.length === 0) {
    return null;
  }

  return (
    <section className="bg-background">
      <Container className="py-16 sm:py-20">
        <SectionHeading eyebrow="آراء" title="كلمات ممن جربوا العمل معنا." />
        <ul className="mt-12 grid gap-5 sm:gap-6 lg:grid-cols-3">
          {items.map((item, index) => (
            <li key={item.id}>
              <FeedbackCard
                quote={item.body}
                name={item.authorDisplayName}
                role={item.authorTitle ?? undefined}
                index={index}
              />
            </li>
          ))}
        </ul>
      </Container>
    </section>
  );
}
