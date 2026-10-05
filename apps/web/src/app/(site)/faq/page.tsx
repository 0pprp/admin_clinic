import { Container } from "@/components/shared/Container";
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

export default async function FaqPage() {
  const items = (await fetchPublic<FaqItem[]>("/api/public/faq")) ?? [];

  return (
    <>
      <PageIntro
        eyebrow="الأسئلة الشائعة"
        title="إجابات مختصرة قبل أن تبدأ."
        description="تظهر هنا الأسئلة النشطة فقط."
      />
      <Container className="py-16">
        {items.length === 0 ? (
          <EmptyState title="لا توجد أسئلة معتمدة بعد" description="عند اعتماد الأسئلة الشائعة ستظهر في هذه الصفحة." />
        ) : (
          <FaqAccordion items={items} />
        )}
      </Container>
    </>
  );
}
