import { aboutPreviewCopy } from "@/lib/content/brand";
import { ButtonLink } from "@/components/ui/clinic";
import { Container } from "@/components/shared/Container";
import { SectionHeading } from "@/components/shared/SectionHeading";

export function AboutPreview() {
  return (
    <section className="bg-background">
      <Container className="grid gap-10 py-16 sm:py-20 lg:grid-cols-[0.9fr_1.1fr] lg:items-end">
        <SectionHeading eyebrow={aboutPreviewCopy.eyebrow} title={aboutPreviewCopy.title} />
        <div>
          <p className="max-w-xl text-base leading-8 text-muted">{aboutPreviewCopy.body}</p>
          <p className="mt-4 max-w-xl text-xs leading-6 text-muted/80">{aboutPreviewCopy.note}</p>
          <ButtonLink href="/about" variant="outline" size="lg" className="mt-8">
            اعرف المزيد
          </ButtonLink>
        </div>
      </Container>
    </section>
  );
}
