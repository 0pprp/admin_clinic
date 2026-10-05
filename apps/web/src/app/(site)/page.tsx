import { AboutPreview } from "@/components/home/AboutPreview";
import { ArticlesPreview } from "@/components/home/ArticlesPreview";
import { ConsultationCta } from "@/components/home/ConsultationCta";
import { ExpertiseSection } from "@/components/home/ExpertiseSection";
import { FaqSection } from "@/components/home/FaqSection";
import { FeaturedCourses } from "@/components/home/FeaturedCourses";
import { FinalCta } from "@/components/home/FinalCta";
import { HeroSection } from "@/components/home/HeroSection";
import { StatisticsSection } from "@/components/home/StatisticsSection";
import { TestimonialsSection } from "@/components/home/TestimonialsSection";
import { TrustStrip } from "@/components/home/TrustStrip";
import { WhySection } from "@/components/home/WhySection";
import { AdCarousel } from "@/components/ui/clinic";
import { Container } from "@/components/shared/Container";
import { fetchPublic } from "@/lib/api/public";
import type {
  ArticleSummary,
  CourseSummary,
  ExpertiseItem,
  FaqItem,
  StatisticItem,
  TestimonialItem
} from "@/lib/api/public-types";
import { homeAdSlides } from "@/lib/content/ads";
import { brand } from "@/lib/content/brand";
import { resolveHeroImage } from "@/lib/media/hero-image";
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

export default async function HomePage() {
  const [expertise, featured, statistics, testimonials, articles, faq] = await Promise.all([
    fetchPublic<ExpertiseItem[]>("/api/public/expertise"),
    fetchPublic<CourseSummary[]>("/api/public/courses/featured"),
    fetchPublic<StatisticItem[]>("/api/public/statistics"),
    fetchPublic<TestimonialItem[]>("/api/public/testimonials"),
    fetchPublic<ArticleSummary[]>("/api/public/articles/latest"),
    fetchPublic<FaqItem[]>("/api/public/faq?home=true")
  ]);

  return (
    <>
      <HeroSection imageSrc={resolveHeroImage()} />
      <Container className="py-8 sm:py-10">
        <AdCarousel slides={homeAdSlides} />
      </Container>
      <TrustStrip expertise={expertise ?? []} />
      <AboutPreview />
      <ExpertiseSection items={expertise ?? []} />
      <FeaturedCourses courses={featured ?? []} />
      <WhySection />
      <StatisticsSection items={statistics ?? []} />
      <TestimonialsSection items={testimonials ?? []} />
      <ArticlesPreview articles={articles ?? []} />
      <ConsultationCta />
      <FaqSection items={faq ?? []} />
      <FinalCta />
    </>
  );
}
