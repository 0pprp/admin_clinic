import { legalCopy } from "@/lib/content/pages";
import { createPageMetadata } from "@/lib/seo";
import { Container } from "@/components/shared/Container";
import { PageIntro } from "@/components/shared/PageIntro";

export const metadata = createPageMetadata({
  title: legalCopy.termsTitle,
  description: "الشروط والأحكام للعيادة الإدارية. النص النهائي سيُعتمد لاحقاً.",
  path: "/terms"
});

export default function TermsPage() {
  return (
    <>
      <PageIntro eyebrow="قانوني" title={legalCopy.termsTitle} />
      <Container className="py-12 sm:py-16">
        <div className="clinic-card max-w-2xl px-6 py-8 sm:px-8">
          <p className="text-base leading-8 text-muted">{legalCopy.termsBody}</p>
        </div>
      </Container>
    </>
  );
}
