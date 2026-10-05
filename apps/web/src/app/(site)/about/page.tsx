import { positioningAreas } from "@/lib/content/brand";
import { aboutCopy } from "@/lib/content/pages";
import { fetchPublic } from "@/lib/api/public";
import type { ExpertiseItem } from "@/lib/api/public-types";
import { createPageMetadata } from "@/lib/seo";
import { BrandSlogan } from "@/components/brand/BrandSlogan";
import { brand } from "@/lib/content/brand";
import { ButtonLink } from "@/components/ui/clinic";
import { PageIntro } from "@/components/shared/PageIntro";

export const metadata = createPageMetadata({
  title: "عن العيادة",
  description: "نبذة عن العيادة الإدارية ومنهج التشخيص والعلاج والتطوير الإداري.",
  path: "/about"
});

/** P13 · عن العيادة */
export default async function AboutPage() {
  const expertise = await fetchPublic<ExpertiseItem[]>("/api/public/expertise");
  const interests = expertise && expertise.length > 0 ? expertise.map((item) => item.title) : [...positioningAreas];

  return (
    <div className="clinic-shell space-y-12 py-10 sm:space-y-14 sm:py-14">
      <PageIntro embedded eyebrow={aboutCopy.heroEyebrow} title={aboutCopy.heroTitle} description={aboutCopy.heroBody} />

      <section className="clinic-card px-5 py-7 sm:px-8 sm:py-9">
        <h2 className="text-xl font-extrabold tracking-tight sm:text-2xl">{aboutCopy.bioTitle}</h2>
        <p className="mt-4 max-w-2xl text-sm leading-8 text-muted sm:text-base">{aboutCopy.bioBody}</p>
      </section>

      <section>
        <h2 className="text-xl font-extrabold tracking-tight sm:text-2xl">{aboutCopy.philosophyTitle}</h2>
        <p className="mt-4 max-w-2xl text-sm leading-8 text-muted sm:text-base">{aboutCopy.philosophyBody}</p>
      </section>

      <section>
        <h2 className="text-xl font-extrabold tracking-tight sm:text-2xl">{aboutCopy.interestsTitle}</h2>
        <ul className="mt-6 grid gap-3 sm:grid-cols-2">
          {interests.map((item) => (
            <li key={item} className="clinic-card border-s-4 border-s-accent px-5 py-4 text-base font-semibold leading-8">
              {item}
            </li>
          ))}
        </ul>
      </section>

      <section>
        <h2 className="text-xl font-extrabold tracking-tight sm:text-2xl">{aboutCopy.storyTitle}</h2>
        <p className="mt-4 max-w-2xl text-sm leading-8 text-muted sm:text-base">{aboutCopy.storyBody}</p>
        <BrandSlogan className="mt-5 text-lg font-bold text-accent sm:text-xl">{brand.sloganAr}</BrandSlogan>
      </section>

      <section className="clinic-card overflow-hidden bg-surface-dark px-6 py-10 text-primary-foreground sm:px-10 sm:py-12">
        <h2 className="text-2xl font-extrabold tracking-tight sm:text-3xl">ابدأ من المحتوى أو من نقاش أعمق.</h2>
        <div className="mt-7 flex flex-wrap gap-3">
          <ButtonLink href="/courses" variant="accent" size="lg">
            استكشف الكورسات
          </ButtonLink>
          <ButtonLink
            href="/consultation"
            variant="outline"
            size="lg"
            className="border-primary-foreground/30 text-primary-foreground hover:bg-primary-foreground hover:text-primary"
          >
            احجز استشارة
          </ButtonLink>
        </div>
      </section>
    </div>
  );
}
