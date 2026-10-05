import Image from "next/image";
import Link from "next/link";
import { brand, heroCopy } from "@/lib/content/brand";
import { BrandSlogan } from "@/components/brand/BrandSlogan";
import { Container } from "@/components/shared/Container";

export function HeroSection({ imageSrc }: { imageSrc: string | null }) {
  return (
    <section className="relative overflow-hidden bg-surface-dark text-primary-foreground">
      <div
        aria-hidden="true"
        className="pointer-events-none absolute inset-0 opacity-[0.08]"
        style={{
          backgroundImage:
            "linear-gradient(to left, rgba(241,90,36,0.45) 1px, transparent 1px), linear-gradient(to bottom, rgba(241,90,36,0.45) 1px, transparent 1px)",
          backgroundSize: "72px 72px"
        }}
      />
      <p
        aria-hidden="true"
        className="pointer-events-none absolute -start-6 top-16 hidden select-none text-[9rem] font-semibold leading-none text-white/5 md:block"
      >
        عيادة
      </p>
      <Container className="relative grid items-center gap-8 py-12 sm:gap-12 sm:py-16 lg:grid-cols-[minmax(0,0.55fr)_minmax(0,0.45fr)] lg:gap-16 lg:py-24">
        <div className="min-w-0">
          <BrandSlogan className="text-xl font-bold leading-relaxed tracking-wide sm:text-2xl md:text-[1.75rem]">
            {heroCopy.eyebrow}
          </BrandSlogan>
          <h1 className="mt-4 max-w-xl text-[1.85rem] font-semibold leading-[1.3] tracking-tight sm:mt-5 sm:text-4xl lg:text-[3.1rem]">
            {heroCopy.title}
          </h1>
          <p className="mt-4 max-w-lg text-sm leading-8 text-primary-foreground/70 sm:mt-6 sm:text-base">
            {heroCopy.description}
          </p>
          <div className="mt-7 flex flex-wrap gap-3 sm:mt-9">
            <Link
              href="/courses"
              className="border border-accent bg-accent px-5 py-3 text-sm text-primary-foreground transition hover:bg-accent-soft"
            >
              استكشف الكورسات
            </Link>
            <Link
              href="/about"
              className="border border-primary-foreground/25 px-5 py-3 text-sm text-primary-foreground transition hover:border-accent hover:text-accent-soft"
            >
              عن العيادة
            </Link>
          </div>
        </div>
        <div className="relative mx-auto w-full max-w-xs sm:max-w-sm lg:mx-0 lg:max-w-none">
          <div className="absolute -inset-2 border border-accent/30 sm:-inset-3" aria-hidden="true" />
          <div className="relative aspect-[4/5] overflow-hidden bg-[#0a2744]">
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
                <Image
                  src="/brand/logo.png"
                  alt=""
                  width={160}
                  height={160}
                  className="h-28 w-28 object-contain sm:h-40 sm:w-40"
                  priority
                />
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
