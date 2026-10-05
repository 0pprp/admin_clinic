import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { EnrollRequestForm } from "@/components/courses/EnrollRequestForm";
import { Container } from "@/components/shared/Container";
import { PageIntro } from "@/components/shared/PageIntro";
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
    <>
      <PageIntro
        eyebrow="طلب الاشتراك"
        title="تأكيد بيانات طلب الاشتراك"
        description="هذا ليس دفعاً داخل الموقع. بعد الإرسال يتواصل معك الفريق لتفاصيل التحويل، ثم يتم التفعيل لاحقاً."
      />
      <Container className="py-12 sm:py-16">
        <Link href={`/courses/${slug}`} className="text-sm font-medium text-accent hover:underline">
          العودة إلى الدورة
        </Link>
        <div className="mt-8 max-w-3xl">
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
    </>
  );
}
