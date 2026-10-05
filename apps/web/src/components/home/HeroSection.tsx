import Image from "next/image";
import { brand, heroCopy } from "@/lib/content/brand";
import { BrandSlogan } from "@/components/brand/BrandSlogan";
import { BrandMark, ButtonLink } from "@/components/ui/clinic";
import { Container } from "@/components/shared/Container";

export function HeroSection({ imageSrc }: { imageSrc: string | null }) {
  return (
    <section className="relative overflow-hidden bg-surface-dark text-primary-foreground">
      <div
        aria-hidden="true"
        className="pointer-events-none absolute inset-0 opacity-[0.07]"
        style={{
          backgroundImage:
            "radial-gradient(circle at 20% 20%, rgba(241,90,36,0.35), transparent 42%), linear-gradient(120deg, rgba(255,255,255,0.04), transparent 55%)"
        }}
      />
      <Container className="relative grid items-center gap-8 py-12 sm:gap-12 sm:py-16 lg:grid-cols-[minmax(0,0.58fr)_minmax(0,0.42fr)] lg:gap-14 lg:py-22 lg:py-24">
        <div className="min-w-0">
          <BrandSlogan className="text-xl font-bold leading-relaxed tracking-wide sm:text-2xl md:text-[1.75rem]">
            {heroCopy.eyebrow}
          </BrandSlogan>
          <h1 className="mt-4 max-w-xl text-[1.85rem] font-semibold leading-[1.3] tracking-tight sm:mt-5 sm:text-4xl lg:text-[3rem]">
            {heroCopy.title}
          </h1>
          <p className="mt-4 max-w-lg text-sm leading-8 text-primary-foreground/70 sm:mt-6 sm:text-base">
            {heroCopy.description}
          </p>
          <div className="mt-7 flex flex-wrap gap-3 sm:mt-9">
            <ButtonLink href="/courses" variant="accent" size="lg">
              استكشف الكورسات
            </ButtonLink>
            <ButtonLink
              href="/about"
              variant="outline"
              size="lg"
              className="border-primary-foreground/30 text-primary-foreground hover:border-accent hover:bg-transparent hover:text-accent-soft"
            >
              عن العيادة
            </ButtonLink>
          </div>
        </div>
        <div className="relative mx-auto w-full max-w-xs sm:max-w-sm lg:mx-0 lg:max-w-none">
          <div className="absolute -inset-2 rounded-2xl border border-accent/30 sm:-inset-3" aria-hidden="true" />
          <div className="relative aspect-[4/5] overflow-hidden rounded-2xl bg-[#0a2744]">
            {imageSrc ? (
              <Image
                src={imageSrc}
                alt={`الهوية البصرية لـ ${brand.nameAr}`}
                fill
                className="object-cover"
                sizes="(max-width: 1024px) 80vw, 40vw"
                priority
              />
            ) : (
              <div className="flex h-full flex-col items-center justify-center gap-6 bg-[#071b33] p-6 text-center sm:gap-8 sm:p-8">
                <BrandMark className="h-28 w-28 sm:h-36 sm:w-36" />
                <div>
                  <p className="text-xl font-semibold sm:text-2xl">{brand.nameAr}</p>
                  <p className="mt-3 text-sm text-primary-foreground/70">{brand.taglineAr}</p>
                  <BrandSlogan className="mt-5 text-base font-bold sm:text-lg">{brand.sloganAr}</BrandSlogan>
                </div>
              </div>
            )}
          </div>
        </div>
      </Container>
    </section>
  );
}
