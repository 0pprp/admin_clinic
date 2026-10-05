import { legalCopy } from "@/lib/content/pages";
import { createPageMetadata } from "@/lib/seo";
import { Container } from "@/components/shared/Container";
import { PageIntro } from "@/components/shared/PageIntro";

export const metadata = createPageMetadata({
  title: legalCopy.privacyTitle,
  description: "سياسة الخصوصية للعيادة الإدارية. النص النهائي سيُعتمد لاحقاً.",
  path: "/privacy"
});

export default function PrivacyPage() {
  return (
    <>
      <PageIntro eyebrow="قانوني" title={legalCopy.privacyTitle} />
      <Container className="py-12 sm:py-16">
        <div className="clinic-card max-w-2xl px-6 py-8 sm:px-8">
          <p className="text-base leading-8 text-muted">{legalCopy.privacyBody}</p>
        </div>
      </Container>
    </>
  );
}
