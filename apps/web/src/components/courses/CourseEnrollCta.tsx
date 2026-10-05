import { ButtonLink } from "@/components/ui/clinic";

export function CourseEnrollCta({ slug, hasSession }: { slug: string; hasSession: boolean }) {
  if (hasSession) {
    return (
      <ButtonLink href={`/courses/${slug}/enroll`} variant="accent" size="lg">
        اشترك الآن
      </ButtonLink>
    );
  }

  const from = encodeURIComponent(`/courses/${slug}`);
  return (
    <ButtonLink href={`/login?from=${from}&intent=enroll`} variant="accent" size="lg">
      اشترك الآن
    </ButtonLink>
  );
}
