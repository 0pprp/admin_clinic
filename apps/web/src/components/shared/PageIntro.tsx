import { BrandAccentLabel } from "@/components/brand/BrandAccentLabel";
import { Container } from "@/components/shared/Container";

export function PageIntro({
  eyebrow,
  title,
  description
}: {
  eyebrow?: string;
  title: string;
  description?: string;
}) {
  return (
    <section className="border-b border-border bg-surface-dark text-primary-foreground">
      <Container className="py-16 sm:py-20">
        {eyebrow ? (
          <BrandAccentLabel className="text-base font-bold tracking-wide sm:text-lg">
            {eyebrow}
          </BrandAccentLabel>
        ) : null}
        <h1 className="mt-4 max-w-3xl text-4xl font-semibold leading-tight tracking-tight sm:text-5xl">{title}</h1>
        {description ? (
          <p className="mt-5 max-w-2xl text-base leading-8 text-primary-foreground/70">{description}</p>
        ) : null}
      </Container>
    </section>
  );
}
