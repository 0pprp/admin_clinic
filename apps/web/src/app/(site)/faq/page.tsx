import { EmptyState } from "@/components/shared/EmptyState";
import { FaqAccordion } from "@/components/shared/FaqAccordion";
import { PageIntro } from "@/components/shared/PageIntro";
import { fetchPublic } from "@/lib/api/public";
import type { FaqItem } from "@/lib/api/public-types";
import { createPageMetadata } from "@/lib/seo";

export const metadata = createPageMetadata({
  title: "الأسئلة الشائعة",
  description: "إجابات على الأسئلة المتكررة حول العيادة الإدارية.",
  path: "/faq"
});

/** P14 · الأسئلة الشائعة */
export default async function FaqPage() {
  const items = (await fetchPublic<FaqItem[]>("/api/public/faq")) ?? [];

  return (
    <div className="clinic-shell py-10 sm:py-14">
      <PageIntro
        embedded
        eyebrow="الأسئلة الشائعة"
        title="إجابات مختصرة قبل أن تبدأ."
        description="تظهر هنا الأسئلة النشطة فقط."
      />

      <div className="mt-10 max-w-3xl">
        {items.length === 0 ? (
          <EmptyState title="لا توجد أسئلة معتمدة بعد" description="عند اعتماد الأسئلة الشائعة ستظهر في هذه الصفحة." />
        ) : (
          <FaqAccordion items={items} />
        )}
      </div>
    </div>
  );
}
