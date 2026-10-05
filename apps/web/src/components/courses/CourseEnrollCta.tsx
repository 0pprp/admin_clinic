import { ButtonLink } from "@/components/ui/clinic";

export function CourseEnrollCta({
  slug,
  hasSession,
  className = "w-full"
}: {
  slug: string;
  hasSession: boolean;
  className?: string;
}) {
  if (hasSession) {
    return (
      <ButtonLink href={`/courses/${slug}/enroll`} variant="accent" size="lg" className={className}>
        اشترك الآن
      </ButtonLink>
    );
  }

  const from = encodeURIComponent(`/courses/${slug}`);
  return (
    <ButtonLink href={`/login?from=${from}&intent=enroll`} variant="accent" size="lg" className={className}>
      اشترك الآن
    </ButtonLink>
  );
}
