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
      <Container className="py-16">
        <p className="max-w-2xl text-base leading-8 text-muted">{legalCopy.termsBody}</p>
      </Container>
    </>
  );
}
