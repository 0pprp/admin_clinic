import Link from "next/link";

export function CourseEnrollCta({ slug, hasSession }: { slug: string; hasSession: boolean }) {
  if (hasSession) {
    return (
      <Link
        href={`/courses/${slug}/enroll`}
        className="inline-flex border border-accent bg-accent px-5 py-3 text-sm text-surface-dark"
      >
        اشترك الآن
      </Link>
    );
  }

  const from = encodeURIComponent(`/courses/${slug}`);
  return (
    <Link
      href={`/login?from=${from}&intent=enroll`}
      className="inline-flex border border-accent bg-accent px-5 py-3 text-sm text-surface-dark"
    >
      اشترك الآن
    </Link>
  );
}
