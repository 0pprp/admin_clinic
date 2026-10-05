import Link from "next/link";
import { brand, heroCopy } from "@/lib/content/brand";

export function HeroSection() {
  return (
    <section className="clinic-shell pt-4 sm:pt-6 md:pt-8">
      <div className="overflow-hidden rounded-[1.25rem] bg-surface-dark px-4 py-8 text-primary-foreground sm:rounded-[1.75rem] sm:px-8 sm:py-12 md:px-10 md:py-14 lg:px-14 lg:py-16">
        <p className="text-sm font-bold text-accent sm:text-base">{heroCopy.eyebrow}</p>
        <h1 className="mt-3 max-w-3xl text-[1.65rem] font-extrabold leading-[1.35] tracking-tight sm:mt-4 sm:text-4xl lg:text-[3rem]">
          {heroCopy.title}
        </h1>
        <p className="mt-3 max-w-2xl text-sm leading-7 text-primary-foreground/75 sm:mt-5 sm:text-base sm:leading-8">
          {heroCopy.description}
        </p>
        <div className="mt-7 flex flex-col gap-3 sm:mt-8 sm:flex-row sm:flex-wrap">
          <Link
            href="/courses"
            className="inline-flex w-full items-center justify-center rounded-xl bg-accent px-5 py-3 text-sm font-semibold text-primary-foreground transition hover:bg-accent-soft sm:w-auto"
          >
            استكشف الكورسات
          </Link>
          <Link
            href="/consultation"
            className="inline-flex w-full items-center justify-center rounded-xl bg-surface px-5 py-3 text-sm font-semibold text-primary transition hover:bg-surface-warm sm:w-auto"
          >
            اطلب استشارة
          </Link>
        </div>
        <p className="sr-only">{brand.nameAr}</p>
      </div>
    </section>
  );
}
