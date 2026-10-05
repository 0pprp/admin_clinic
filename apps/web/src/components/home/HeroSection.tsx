import Link from "next/link";
import { brand, heroCopy } from "@/lib/content/brand";

export function HeroSection() {
  return (
    <section className="clinic-shell pt-6 sm:pt-8">
      <div className="overflow-hidden rounded-[1.75rem] bg-surface-dark px-6 py-10 text-primary-foreground sm:px-10 sm:py-14 lg:px-14 lg:py-16">
        <p className="text-sm font-bold text-accent sm:text-base">{heroCopy.eyebrow}</p>
        <h1 className="mt-4 max-w-3xl text-[1.85rem] font-extrabold leading-[1.35] tracking-tight sm:text-4xl lg:text-[3rem]">
          {heroCopy.title}
        </h1>
        <p className="mt-4 max-w-2xl text-sm leading-8 text-primary-foreground/75 sm:mt-5 sm:text-base">
          {heroCopy.description}
        </p>
        <div className="mt-8 flex flex-wrap gap-3">
          <Link
            href="/courses"
            className="inline-flex rounded-xl bg-accent px-5 py-3 text-sm font-semibold text-primary-foreground transition hover:bg-accent-soft"
          >
            استكشف الكورسات
          </Link>
          <Link
            href="/consultation"
            className="inline-flex rounded-xl bg-surface px-5 py-3 text-sm font-semibold text-primary transition hover:bg-surface-warm"
          >
            اطلب استشارة
          </Link>
        </div>
        <p className="sr-only">{brand.nameAr}</p>
      </div>
    </section>
  );
}
