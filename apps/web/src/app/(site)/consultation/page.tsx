import { consultationPageCopy } from "@/lib/content/pages";
import { createPageMetadata } from "@/lib/seo";
import { BrandAccentLabel } from "@/components/brand/BrandAccentLabel";
import { Container } from "@/components/shared/Container";
import { PageIntro } from "@/components/shared/PageIntro";
import { ConsultationForm } from "@/components/public/ConsultationForm";
import { fetchPublic } from "@/lib/api/public";
import type { PublicSiteSettings } from "@/lib/api/public-types";

export const metadata = createPageMetadata({
  title: "الاستشارات",
  description: "اطلب استشارة مع العيادة الإدارية. يراجع الفريق الطلب ويتواصل لتأكيد الموعد والتفاصيل.",
  path: "/consultation"
});

export default async function ConsultationPage() {
  const settings = await fetchPublic<PublicSiteSettings>("/api/public/site-settings");

  return (
    <>
      <PageIntro
        eyebrow={consultationPageCopy.eyebrow}
        title={consultationPageCopy.title}
        description={consultationPageCopy.intro}
      />
      <Container className="space-y-16 py-16">
        <section>
          <h2 className="text-3xl font-semibold">{consultationPageCopy.forWhomTitle}</h2>
          <ul className="mt-6 max-w-2xl space-y-4 text-base leading-8 text-muted">
            {consultationPageCopy.forWhom.map((item) => (
              <li key={item} className="border-s border-accent/50 ps-4">
                {item}
              </li>
            ))}
          </ul>
        </section>
        <section className="border-t border-border pt-16">
          <h2 className="text-3xl font-semibold">{consultationPageCopy.processTitle}</h2>
          <ol className="mt-6 max-w-2xl space-y-4 text-base leading-8 text-muted">
            {consultationPageCopy.process.map((item, index) => (
              <li key={item}>
                <BrandAccentLabel as="span" className="text-sm font-bold tracking-wide">
                  {String(index + 1).padStart(2, "0")}
                </BrandAccentLabel>
                <span className="ms-3">{item}</span>
              </li>
            ))}
          </ol>
        </section>
        {settings?.consultationInfo ? (
          <p className="max-w-2xl text-base leading-8 text-muted whitespace-pre-wrap">{settings.consultationInfo}</p>
        ) : null}
        <section className="border-t border-border pt-16">
          <h2 className="text-3xl font-semibold">طلب استشارة</h2>
          <p className="mt-3 max-w-2xl text-sm leading-8 text-muted">{consultationPageCopy.note}</p>
          <div className="mt-8">
            <ConsultationForm />
          </div>
        </section>
      </Container>
    </>
  );
}
