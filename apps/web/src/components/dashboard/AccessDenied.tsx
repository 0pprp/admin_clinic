import Link from "next/link";

export function AccessDenied({
  title = "لا تملك صلاحية الوصول إلى هذا المحتوى.",
  description = "إذا كنت تعتقد أن هذا خطأ، تواصل مع الدعم أو عد إلى دوراتك."
}: {
  title?: string;
  description?: string;
}) {
  return (
    <section className="border border-border bg-surface px-5 py-10 sm:px-8">
      <h1 className="text-2xl font-semibold">{title}</h1>
      <p className="mt-3 max-w-xl text-sm leading-8 text-muted">{description}</p>
      <div className="mt-8 flex flex-wrap gap-3">
        <Link href="/dashboard/courses" className="border border-accent px-4 py-2 text-sm text-accent">
          دوراتي
        </Link>
        <Link href="/contact" className="border border-border px-4 py-2 text-sm">
          تواصل مع الدعم
        </Link>
      </div>
    </section>
  );
}
