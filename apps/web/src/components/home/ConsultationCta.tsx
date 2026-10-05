import { consultationCopy } from "@/lib/content/brand";
import { BrandAccentLabel } from "@/components/brand/BrandAccentLabel";
import { ButtonLink } from "@/components/ui/clinic";
import { Container } from "@/components/shared/Container";

export function ConsultationCta() {
  return (
    <section className="bg-background">
      <Container className="py-16 sm:py-20">
        <div className="clinic-card overflow-hidden bg-[linear-gradient(135deg,#ffffff_0%,#e8eef5_100%)] px-6 py-12 sm:px-12 sm:py-14">
          <BrandAccentLabel className="text-sm font-bold tracking-wide sm:text-base">الاستشارات</BrandAccentLabel>
          <h2 className="mt-4 max-w-2xl text-3xl font-semibold leading-tight sm:text-4xl">{consultationCopy.title}</h2>
          <p className="mt-5 max-w-xl text-base leading-8 text-muted">{consultationCopy.body}</p>
          <ButtonLink href="/consultation" variant="primary" size="lg" className="mt-8">
            {consultationCopy.cta}
          </ButtonLink>
        </div>
      </Container>
    </section>
  );
}
