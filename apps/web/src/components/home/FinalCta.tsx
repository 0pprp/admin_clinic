import Link from "next/link";
import { finalCtaCopy } from "@/lib/content/brand";
import { Container } from "@/components/shared/Container";

export function FinalCta() {
  return (
    <section className="bg-surface-dark text-primary-foreground">
      <Container className="py-20 text-center">
        <h2 className="text-3xl font-semibold sm:text-5xl">{finalCtaCopy.title}</h2>
        <p className="mx-auto mt-5 max-w-xl text-sm leading-8 text-primary-foreground/70">{finalCtaCopy.body}</p>
        <div className="mt-9 flex flex-wrap justify-center gap-3">
          <Link href="/courses" className="border border-accent bg-accent px-5 py-3 text-sm text-primary-foreground">
            استكشف الكورسات
          </Link>
          <Link href="/consultation" className="border border-primary-foreground/25 px-5 py-3 text-sm">
            احجز استشارة
          </Link>
        </div>
      </Container>
    </section>
  );
}
