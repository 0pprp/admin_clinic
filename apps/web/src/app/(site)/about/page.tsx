import { positioningAreas } from "@/lib/content/brand";
import { aboutCopy } from "@/lib/content/pages";
import { fetchPublic } from "@/lib/api/public";
import type { ExpertiseItem } from "@/lib/api/public-types";
import { createPageMetadata } from "@/lib/seo";
import { BrandSlogan } from "@/components/brand/BrandSlogan";
import { brand } from "@/lib/content/brand";
import { ButtonLink } from "@/components/ui/clinic";
import { Container } from "@/components/shared/Container";
import { PageIntro } from "@/components/shared/PageIntro";

export const metadata = createPageMetadata({
  title: "عن العيادة",
  description: "نبذة عن العيادة الإدارية ومنهج التشخيص والعلاج والتطوير الإداري.",
  path: "/about"
});

export default async function AboutPage() {
  const expertise = await fetchPublic<ExpertiseItem[]>("/api/public/expertise");
  const interests = expertise && expertise.length > 0 ? expertise.map((item) => item.title) : [...positioningAreas];

  return (
    <>
      <PageIntro eyebrow={aboutCopy.heroEyebrow} title={aboutCopy.heroTitle} description={aboutCopy.heroBody} />
      <Container className="space-y-16 py-16 sm:py-20">
        <section>
          <h2 className="text-3xl font-semibold">{aboutCopy.bioTitle}</h2>
          <p className="mt-5 max-w-2xl text-base leading-8 text-muted">{aboutCopy.bioBody}</p>
        </section>
        <section className="border-t border-border pt-16">
          <h2 className="text-3xl font-semibold">{aboutCopy.philosophyTitle}</h2>
          <p className="mt-5 max-w-2xl text-base leading-8 text-muted">{aboutCopy.philosophyBody}</p>
        </section>
        <section className="border-t border-border pt-16">
          <h2 className="text-3xl font-semibold">{aboutCopy.interestsTitle}</h2>
          <ul className="mt-8 grid gap-4 sm:grid-cols-2">
            {interests.map((item) => (
              <li key={item} className="clinic-panel border-s-4 border-s-accent px-5 py-4 text-lg">
                {item}
              </li>
            ))}
          </ul>
        </section>
        <section className="border-t border-border pt-16">
          <h2 className="text-3xl font-semibold">{aboutCopy.storyTitle}</h2>
          <p className="mt-5 max-w-2xl text-base leading-8 text-muted">{aboutCopy.storyBody}</p>
          <BrandSlogan className="mt-5 text-xl font-bold sm:text-2xl">{brand.sloganAr}</BrandSlogan>
        </section>
        <section className="clinic-card overflow-hidden bg-surface-dark px-6 py-12 text-primary-foreground sm:px-10">
          <h2 className="text-3xl font-semibold">ابدأ من المحتوى أو من نقاش أعمق.</h2>
          <div className="mt-8 flex flex-wrap gap-3">
            <ButtonLink href="/courses" variant="accent" size="lg">
              استكشف الكورسات
            </ButtonLink>
            <ButtonLink
              href="/consultation"
              variant="outline"
              size="lg"
              className="border-primary-foreground/25 text-primary-foreground hover:bg-primary-foreground hover:text-primary"
            >
              احجز استشارة
            </ButtonLink>
          </div>
        </section>
      </Container>
    </>
  );
}
