import { ButtonLink } from "@/components/ui/clinic";

export function AccessDenied({
  title = "لا تملك صلاحية الوصول إلى هذا المحتوى.",
  description = "إذا كنت تعتقد أن هذا خطأ، تواصل مع الدعم أو عد إلى دوراتك."
}: {
  title?: string;
  description?: string;
}) {
  return (
    <section className="clinic-panel px-5 py-10 sm:px-8 sm:py-12">
      <h1 className="text-2xl font-semibold">{title}</h1>
      <p className="mt-3 max-w-xl text-sm leading-8 text-muted">{description}</p>
      <div className="mt-8 flex flex-wrap gap-3">
        <ButtonLink href="/dashboard/courses" variant="accent" size="md">
          دوراتي
        </ButtonLink>
        <ButtonLink href="/contact" variant="soft" size="md">
          تواصل مع الدعم
        </ButtonLink>
      </div>
    </section>
  );
}
