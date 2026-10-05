import Link from "next/link";
import { positioningAreas } from "@/lib/content/brand";
import { aboutCopy } from "@/lib/content/pages";
import { fetchPublic } from "@/lib/api/public";
import type { ExpertiseItem } from "@/lib/api/public-types";
import { createPageMetadata } from "@/lib/seo";
import { BrandSlogan } from "@/components/brand/BrandSlogan";
import { brand } from "@/lib/content/brand";
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
              <li key={item} className="border-s-2 border-accent ps-4 text-lg">
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
        <section className="bg-surface-dark px-6 py-12 text-primary-foreground sm:px-10">
          <h2 className="text-3xl font-semibold">ابدأ من المحتوى أو من نقاش أعمق.</h2>
          <div className="mt-8 flex flex-wrap gap-3">
            <Link href="/courses" className="border border-accent bg-accent px-5 py-3 text-sm text-primary-foreground">
              استكشف الكورسات
            </Link>
            <Link href="/consultation" className="border border-primary-foreground/25 px-5 py-3 text-sm">
              احجز استشارة
            </Link>
          </div>
        </section>
      </Container>
    </>
  );
}
