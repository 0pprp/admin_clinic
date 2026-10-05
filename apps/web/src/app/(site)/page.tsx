import { FeaturedCourses } from "@/components/home/FeaturedCourses";
import { HeroSection } from "@/components/home/HeroSection";
import { HomeSteps } from "@/components/home/HomeSteps";
import { AdCarousel } from "@/components/ui/clinic";
import { fetchPublic } from "@/lib/api/public";
import type { CourseSummary } from "@/lib/api/public-types";
import { homeAdSlides } from "@/lib/content/ads";
import { brand } from "@/lib/content/brand";
import { createPageMetadata } from "@/lib/seo";

export const dynamic = "force-dynamic";

const homeMetadata = createPageMetadata({
  title: brand.siteTitle,
  description: brand.siteDescription,
  path: "/"
});

export const metadata = {
  ...homeMetadata,
  title: { absolute: brand.siteTitle }
};

/** P01 · الرئيسية — مطابق لتخطيط Figma حرفياً */
export default async function HomePage() {
  const featured = await fetchPublic<CourseSummary[]>("/api/public/courses/featured");

  return (
    <>
      <HeroSection />
      <HomeSteps />
      <AdCarousel slides={homeAdSlides} />
      <FeaturedCourses courses={featured ?? []} />
    </>
  );
}
