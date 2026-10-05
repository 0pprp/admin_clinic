import Link from "next/link";
import { secondaryButtonClassName } from "@/lib/admin/ui";

export function ForbiddenState({
  title = "ليست لديك صلاحية لعرض هذه الصفحة.",
  description = "هذا القسم متاح لأدوار أخرى في لوحة الإدارة. يمكنك العودة إلى النظرة العامة أو التواصل مع مدير المنصة."
}: {
  title?: string;
  description?: string;
}) {
  return (
    <section className="border border-border bg-surface px-5 py-10">
      <h1 className="text-2xl font-semibold">{title}</h1>
      <p className="mt-3 max-w-xl text-sm leading-8 text-muted">{description}</p>
      <Link href="/admin" className={`mt-8 ${secondaryButtonClassName}`}>
        العودة إلى لوحة الإدارة
      </Link>
    </section>
  );
}
