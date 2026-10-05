import { ButtonLink } from "@/components/ui/clinic";

export function ForbiddenState({
  title = "ليست لديك صلاحية لعرض هذه الصفحة.",
  description = "هذا القسم متاح لأدوار أخرى في لوحة الإدارة. يمكنك العودة إلى النظرة العامة أو التواصل مع مدير المنصة."
}: {
  title?: string;
  description?: string;
}) {
  return (
    <section className="clinic-card px-5 py-10 sm:px-8">
      <p className="text-sm font-bold text-accent">غير مصرح</p>
      <h1 className="mt-2 text-2xl font-extrabold tracking-tight">{title}</h1>
      <p className="mt-3 max-w-xl text-sm leading-8 text-muted">{description}</p>
      <ButtonLink href="/admin" variant="accent" className="mt-8">
        العودة إلى لوحة الإدارة
      </ButtonLink>
    </section>
  );
}
