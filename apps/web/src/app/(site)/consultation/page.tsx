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
          <ul className="mt-6 grid max-w-2xl gap-4">
            {consultationPageCopy.forWhom.map((item) => (
              <li key={item} className="clinic-panel border-s-4 border-s-accent/60 px-5 py-4 text-base leading-8 text-muted">
                {item}
              </li>
            ))}
          </ul>
        </section>
        <section className="border-t border-border pt-16">
          <h2 className="text-3xl font-semibold">{consultationPageCopy.processTitle}</h2>
          <ol className="clinic-panel mt-6 max-w-2xl divide-y divide-border p-5 sm:p-6">
            {consultationPageCopy.process.map((item, index) => (
              <li key={item} className="py-4 text-base leading-8 text-muted first:pt-0 last:pb-0">
                <BrandAccentLabel as="span" className="text-sm font-bold tracking-wide">
                  {String(index + 1).padStart(2, "0")}
                </BrandAccentLabel>
                <span className="ms-3">{item}</span>
              </li>
            ))}
          </ol>
        </section>
        {settings?.consultationInfo ? (
          <p className="clinic-panel max-w-2xl whitespace-pre-wrap p-6 text-base leading-8 text-muted sm:p-8">{settings.consultationInfo}</p>
        ) : null}
        <section className="border-t border-border pt-16">
          <h2 className="text-3xl font-semibold">طلب استشارة</h2>
          <p className="mt-3 max-w-2xl text-sm leading-8 text-muted">{consultationPageCopy.note}</p>
          <div className="clinic-panel mt-8 max-w-2xl p-6 sm:p-8">
            <ConsultationForm />
          </div>
        </section>
      </Container>
    </>
  );
}
