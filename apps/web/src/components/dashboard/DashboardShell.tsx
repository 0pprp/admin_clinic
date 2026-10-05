import { PageIntro } from "@/components/shared/PageIntro";

/** غلاف صفحات مساحة المتعلم — eyebrow برتقالي + عنوان عريض وفق Figma Clinic */
export function DashboardShell({
  eyebrow = "مساحة المتعلم",
  title,
  description,
  children
}: {
  eyebrow?: string;
  title: string;
  description?: string;
  children: React.ReactNode;
}) {
  return (
    <div>
      <PageIntro embedded eyebrow={eyebrow} title={title} description={description} />
      <div className="mt-8">{children}</div>
    </div>
  );
}
