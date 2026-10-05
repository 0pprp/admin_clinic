import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import { cookies } from "next/headers";
import { notFound, redirect } from "next/navigation";
import { BrandAccentLabel } from "@/components/brand/BrandAccentLabel";
import { CourseCurriculum } from "@/components/courses/CourseCurriculum";
import { CourseEnrollCta } from "@/components/courses/CourseEnrollCta";
import { Container } from "@/components/shared/Container";
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
      <section className="border-b border-border bg-surface-dark text-primary-foreground">
        <Container className="grid gap-10 py-14 lg:grid-cols-[1.1fr_0.9fr] lg:items-center">
          <div>
            <nav className="text-xs text-primary-foreground/60" aria-label="مسار التنقل">
              <Link href="/" className="hover:text-accent-soft">
                الرئيسية
              </Link>
              <span aria-hidden="true"> / </span>
              <Link href="/courses" className="hover:text-accent-soft">
                الكورسات
              </Link>
              <span aria-hidden="true"> / </span>
              <span className="text-primary-foreground">{course.title}</span>
            </nav>
            <BrandAccentLabel className="mt-6 text-base font-bold tracking-wide">
              {courseLevelLabel(course.level)}
            </BrandAccentLabel>
            <h1 className="mt-4 text-4xl font-semibold leading-tight sm:text-5xl">{course.title}</h1>
            <p className="mt-5 max-w-2xl text-base leading-8 text-primary-foreground/70">{course.shortDescription}</p>
            <dl className="mt-8 grid gap-4 text-sm sm:grid-cols-2">
              <div>
                <dt className="text-primary-foreground/50">السعر</dt>
                <dd className="mt-1 text-lg font-semibold">{formatIqd(course.priceIQD)}</dd>
              </div>
              <div>
                <dt className="text-primary-foreground/50">المدة</dt>
                <dd className="mt-1">{formatDuration(course.totalDurationSeconds)}</dd>
              </div>
              <div>
                <dt className="text-primary-foreground/50">الدروس</dt>
                <dd className="mt-1">{course.lessonCount} درساً</dd>
              </div>
              <div>
                <dt className="text-primary-foreground/50">الوصول</dt>
                <dd className="mt-1">{courseAccessLabel(course.accessType, course.accessDurationDays)}</dd>
              </div>
            </dl>
            <div className="mt-8">
              <CourseEnrollCta slug={course.slug} hasSession={hasSession} />
            </div>
          </div>
          <div className="relative aspect-[16/10] overflow-hidden bg-[#151c24]">
            {course.thumbnailUrl ? (
              <Image
                src={course.thumbnailUrl}
                alt={`غلاف دورة ${course.title}`}
                fill
                className="object-cover"
                sizes="(max-width: 1024px) 100vw, 45vw"
                unoptimized={!course.thumbnailUrl.startsWith("/")}
              />
            ) : (
              <div className="flex h-full items-end p-6">
                <BrandAccentLabel as="span" className="text-sm font-bold tracking-wide">
                  كورس
                </BrandAccentLabel>
              </div>
            )}
          </div>
        </Container>
      </section>
      <Container className="space-y-16 py-16">
        <section>
          <h2 className="text-3xl font-semibold">عن الدورة</h2>
          <div className="mt-5 max-w-3xl whitespace-pre-line text-base leading-8 text-muted">{course.description}</div>
        </section>
        <section>
          <h2 className="text-3xl font-semibold">المنهج</h2>
          <div className="mt-8">
            <CourseCurriculum slug={course.slug} sections={course.sections} />
          </div>
        </section>
        <section className="grid gap-10 lg:grid-cols-2">
          <div>
            <h2 className="text-2xl font-semibold">ماذا ستخرج به؟</h2>
            <p className="mt-4 text-sm leading-8 text-muted">
              TODO: تُضاف هنا مخرجات التعلّم المعتمدة لهذه الدورة، دون اختراع وعود غير موثقة.
            </p>
          </div>
          <div>
            <h2 className="text-2xl font-semibold">لمن هذه الدورة؟</h2>
            <p className="mt-4 text-sm leading-8 text-muted">
              TODO: يُكتب هنا توصيف الجمهور المستهدف بعد اعتماد المحتوى الحقيقي.
            </p>
          </div>
        </section>
        <section>
          <h2 className="text-2xl font-semibold">مقدّم البرنامج</h2>
          <p className="mt-4 max-w-2xl text-sm leading-8 text-muted">
            تقدّم العيادة الإدارية هذه الدورة ضمن كورساتها. التفاصيل الكاملة عن المنهج متاحة في صفحة «عن العيادة».
          </p>
        </section>
        <section className="bg-surface-dark px-6 py-12 text-primary-foreground sm:px-10">
          <h2 className="text-3xl font-semibold">ابدأ عندما تكون جاهزاً.</h2>
          <p className="mt-4 max-w-xl text-sm leading-7 text-primary-foreground/70">
            الاشتراك يتم عبر طلب يدوي. بعد إرسال الطلب يتواصل معك الفريق لتأكيد التحويل ثم التفعيل.
          </p>
          <div className="mt-8">
            <CourseEnrollCta slug={course.slug} hasSession={hasSession} />
          </div>
        </section>
      </Container>
    </>
  );
}
