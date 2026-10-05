import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import { cookies } from "next/headers";
import { notFound, redirect } from "next/navigation";
import { CourseCurriculum } from "@/components/courses/CourseCurriculum";
import { CourseEnrollCta } from "@/components/courses/CourseEnrollCta";
import { fetchPublic } from "@/lib/api/public";
import type { CourseDetail } from "@/lib/api/public-types";
import { brand } from "@/lib/content/brand";
import { courseAccessLabel, courseLevelLabel, formatDuration, formatIqd } from "@/lib/format";
import { createPageMetadata } from "@/lib/seo";

export const dynamic = "force-dynamic";

export async function generateMetadata({ params }: PageProps<"/courses/[slug]">): Promise<Metadata> {
  const { slug } = await params;
  const course = await fetchPublic<CourseDetail>(`/api/public/courses/${slug}`);
  if (!course) {
    notFound();
  }

  return createPageMetadata({
    title: course.title,
    description: course.shortDescription,
    path: `/courses/${slug}`,
    image: course.thumbnailUrl
  });
}

/** P03 · تفاصيل الكورس — تخطيط منقسم: محتوى/منهج + لوحة سعر واشتراك لاصقة */
export default async function CourseDetailPage({
  params,
  searchParams
}: PageProps<"/courses/[slug]">) {
  const { slug } = await params;
  const query = await searchParams;
  const course = await fetchPublic<CourseDetail>(`/api/public/courses/${slug}`);
  if (!course) {
    notFound();
  }

  const jar = await cookies();
  const hasSession = jar.has("mr_access") || jar.has("mr_refresh");
  if (hasSession && query.intent === "enroll") {
    redirect(`/courses/${slug}/enroll`);
  }
  const jsonLd = {
    "@context": "https://schema.org",
    "@type": "Course",
    name: course.title,
    description: course.shortDescription,
    inLanguage: "ar",
    provider: {
      "@type": "Person",
      name: brand.nameAr
    },
    offers: {
      "@type": "Offer",
      price: course.priceIQD,
      priceCurrency: "IQD",
      availability: "https://schema.org/InStock"
    }
  };

  return (
    <>
      <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: JSON.stringify(jsonLd) }} />
      <div className="clinic-shell py-8 sm:py-10 md:py-14">
        <nav className="text-xs text-muted" aria-label="مسار التنقل">
          <Link href="/" className="hover:text-accent">
            الرئيسية
          </Link>
          <span aria-hidden="true"> / </span>
          <Link href="/courses" className="hover:text-accent">
            الكورسات
          </Link>
          <span aria-hidden="true"> / </span>
          <span className="text-foreground">{course.title}</span>
        </nav>

        <div className="mt-6 grid gap-6 sm:mt-8 sm:gap-8 lg:grid-cols-[minmax(0,1fr)_20rem] lg:items-start lg:gap-10">
          {/* المحتوى على اليمين في RTL، اللوحة على اليسار */}
          <div className="min-w-0 space-y-6 sm:space-y-8">
            <div className="relative aspect-[16/9] overflow-hidden rounded-[1.25rem] bg-surface-dark sm:aspect-[2/1]">
              {course.thumbnailUrl ? (
                <Image
                  src={course.thumbnailUrl}
                  alt={`غلاف دورة ${course.title}`}
                  fill
                  className="object-cover"
                  sizes="(max-width: 1024px) 100vw, 60vw"
                  unoptimized={!course.thumbnailUrl.startsWith("/")}
                  priority
                />
              ) : (
                <div className="flex h-full items-end p-5 text-primary-foreground sm:p-8">
                  <p className="text-sm font-bold text-accent">كورس</p>
                </div>
              )}
            </div>

            <header>
              <p className="text-sm font-bold text-accent">{courseLevelLabel(course.level)}</p>
              <h1 className="mt-3 text-2xl font-extrabold leading-tight tracking-tight sm:text-3xl md:text-4xl">{course.title}</h1>
              <p className="mt-3 max-w-2xl text-sm leading-7 text-muted sm:mt-4 sm:text-base sm:leading-8">{course.shortDescription}</p>
            </header>

            <section className="clinic-card px-4 py-5 sm:px-7 sm:py-8">
              <h2 className="text-lg font-extrabold tracking-tight sm:text-xl md:text-2xl">عن الدورة</h2>
              <div className="mt-4 whitespace-pre-line text-sm leading-7 text-muted sm:text-base sm:leading-8">{course.description}</div>
            </section>

            <section>
              <h2 className="text-lg font-extrabold tracking-tight sm:text-xl md:text-2xl">المنهج</h2>
              <div className="mt-4 sm:mt-5">
                <CourseCurriculum slug={course.slug} sections={course.sections} />
              </div>
            </section>
          </div>

          <aside className="lg:sticky lg:top-24">
            <div className="clinic-card p-4 sm:p-6">
              <p className="text-sm font-bold text-muted">سعر الاشتراك</p>
              <p className="mt-2 text-3xl font-extrabold tracking-tight text-foreground">{formatIqd(course.priceIQD)}</p>

              <dl className="mt-6 space-y-3 border-t border-border pt-5 text-sm">
                <div className="flex items-center justify-between gap-3">
                  <dt className="text-muted">المدة</dt>
                  <dd className="font-semibold">{formatDuration(course.totalDurationSeconds)}</dd>
                </div>
                <div className="flex items-center justify-between gap-3">
                  <dt className="text-muted">الدروس</dt>
                  <dd className="font-semibold">{course.lessonCount} درساً</dd>
                </div>
                <div className="flex items-center justify-between gap-3">
                  <dt className="text-muted">الوصول</dt>
                  <dd className="font-semibold text-end">{courseAccessLabel(course.accessType, course.accessDurationDays)}</dd>
                </div>
              </dl>

              <div className="mt-6">
                <CourseEnrollCta slug={course.slug} hasSession={hasSession} />
              </div>
              <p className="mt-4 text-xs leading-6 text-muted">
                الاشتراك عبر طلب يدوي. بعد الإرسال يتواصل معك الفريق لتأكيد التحويل ثم التفعيل.
              </p>
            </div>
          </aside>
        </div>
      </div>
    </>
  );
}
