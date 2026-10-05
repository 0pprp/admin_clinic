import { finalCtaCopy } from "@/lib/content/brand";
import { ButtonLink } from "@/components/ui/clinic";
import { Container } from "@/components/shared/Container";

export function FinalCta() {
  return (
    <section className="relative overflow-hidden bg-surface-dark text-primary-foreground">
      <div
        aria-hidden="true"
        className="pointer-events-none absolute inset-0 opacity-30"
        style={{
          backgroundImage: "radial-gradient(circle at 80% 20%, rgba(241,90,36,0.45), transparent 40%)"
        }}
      />
      <Container className="relative py-16 text-center sm:py-20">
        <h2 className="text-3xl font-semibold tracking-tight sm:text-5xl">{finalCtaCopy.title}</h2>
        <p className="mx-auto mt-5 max-w-xl text-sm leading-8 text-primary-foreground/70">{finalCtaCopy.body}</p>
        <div className="mt-9 flex flex-wrap justify-center gap-3">
          <ButtonLink href="/courses" variant="accent" size="lg">
            استكشف الكورسات
          </ButtonLink>
          <ButtonLink
            href="/consultation"
            variant="outline"
            size="lg"
            className="border-primary-foreground/30 text-primary-foreground hover:border-accent hover:bg-transparent hover:text-accent-soft"
          >
            احجز استشارة
          </ButtonLink>
        </div>
      </Container>
    </section>
  );
}
