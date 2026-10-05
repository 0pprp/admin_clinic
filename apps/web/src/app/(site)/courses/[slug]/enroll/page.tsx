import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { BrandAccentLabel } from "@/components/brand/BrandAccentLabel";
import { EnrollRequestForm } from "@/components/courses/EnrollRequestForm";
import { Container } from "@/components/shared/Container";
import { fetchPublic } from "@/lib/api/public";
import type { CourseDetail } from "@/lib/api/public-types";

export const metadata: Metadata = {
  title: "طلب الاشتراك",
  robots: { index: false, follow: false }
};

export default async function EnrollPage({ params }: PageProps<"/courses/[slug]/enroll">) {
  const { slug } = await params;
  const course = await fetchPublic<CourseDetail>(`/api/public/courses/${slug}`);
  if (!course) {
    notFound();
  }

  return (
    <Container className="py-14 sm:py-20">
      <Link href={`/courses/${slug}`} className="text-sm text-accent hover:underline">
        العودة إلى الدورة
      </Link>
      <BrandAccentLabel className="mt-8 text-base font-bold tracking-wide">طلب الاشتراك</BrandAccentLabel>
      <h1 className="mt-4 max-w-3xl text-4xl font-semibold leading-tight">تأكيد بيانات طلب الاشتراك</h1>
      <p className="mt-4 max-w-2xl text-sm leading-8 text-muted">
        هذا ليس دفعاً داخل الموقع. بعد الإرسال يتواصل معك الفريق لتفاصيل التحويل، ثم يتم التفعيل لاحقاً.
      </p>
      <div className="mt-10 max-w-3xl border border-border bg-surface px-5 py-8 sm:px-8">
        <EnrollRequestForm
          courseId={course.id}
          courseTitle={course.title}
          slug={course.slug}
          priceIQD={course.priceIQD}
          accessType={course.accessType}
          accessDurationDays={course.accessDurationDays}
        />
      </div>
    </Container>
  );
}
