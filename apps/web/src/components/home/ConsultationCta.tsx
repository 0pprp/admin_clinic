import Link from "next/link";
import { consultationCopy } from "@/lib/content/brand";
import { BrandAccentLabel } from "@/components/brand/BrandAccentLabel";
import { Container } from "@/components/shared/Container";

export function ConsultationCta() {
  return (
    <section className="bg-background">
      <Container className="py-20">
        <div className="border border-border bg-surface px-6 py-14 sm:px-12">
          <BrandAccentLabel className="text-sm font-bold tracking-wide sm:text-base">الاستشارات</BrandAccentLabel>
          <h2 className="mt-4 max-w-2xl text-3xl font-semibold leading-tight sm:text-4xl">{consultationCopy.title}</h2>
          <p className="mt-5 max-w-xl text-base leading-8 text-muted">{consultationCopy.body}</p>
          <Link
            href="/consultation"
            className="mt-8 inline-flex border border-foreground bg-foreground px-5 py-3 text-sm text-primary-foreground transition hover:bg-accent hover:text-surface-dark"
          >
            {consultationCopy.cta}
          </Link>
        </div>
      </Container>
    </section>
  );
}
