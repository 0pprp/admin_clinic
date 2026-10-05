import { consultationPageCopy } from "@/lib/content/pages";
import { createPageMetadata } from "@/lib/seo";
import { PageIntro } from "@/components/shared/PageIntro";
import { ConsultationForm } from "@/components/public/ConsultationForm";
import { fetchPublic } from "@/lib/api/public";
import type { PublicSiteSettings } from "@/lib/api/public-types";

export const metadata = createPageMetadata({
  title: "الاستشارات",
  description: "اطلب استشارة مع العيادة الإدارية. يراجع الفريق الطلب ويتواصل لتأكيد الموعد والتفاصيل.",
  path: "/consultation"
});

/** P07 · الاستشارات — نموذج طويل بحقول العيادة */
export default async function ConsultationPage() {
  const settings = await fetchPublic<PublicSiteSettings>("/api/public/site-settings");

  return (
    <div className="clinic-shell py-10 sm:py-14">
      <PageIntro
        embedded
        eyebrow={consultationPageCopy.eyebrow}
        title={consultationPageCopy.title}
        description={consultationPageCopy.intro}
      />

      <div className="mt-10 grid gap-10 lg:grid-cols-[minmax(0,1fr)_minmax(0,26rem)] lg:items-start lg:gap-12">
        <div className="space-y-10">
          <section>
            <h2 className="text-xl font-extrabold tracking-tight sm:text-2xl">{consultationPageCopy.forWhomTitle}</h2>
            <ul className="mt-5 space-y-3">
              {consultationPageCopy.forWhom.map((item) => (
                <li
                  key={item}
                  className="clinic-card border-s-4 border-s-accent px-5 py-4 text-sm leading-8 text-muted sm:text-base"
                >
                  {item}
                </li>
              ))}
            </ul>
          </section>

          <section>
            <h2 className="text-xl font-extrabold tracking-tight sm:text-2xl">{consultationPageCopy.processTitle}</h2>
            <ol className="clinic-card mt-5 divide-y divide-border overflow-hidden px-5 sm:px-6">
              {consultationPageCopy.process.map((item, index) => (
                <li key={item} className="flex gap-4 py-4 text-sm leading-8 text-muted first:pt-5 last:pb-5 sm:text-base">
                  <span className="shrink-0 text-sm font-bold text-accent">{String(index + 1).padStart(2, "0")}</span>
                  <span>{item}</span>
                </li>
              ))}
            </ol>
          </section>

          {settings?.consultationInfo ? (
            <p className="clinic-card whitespace-pre-wrap px-5 py-6 text-sm leading-8 text-muted sm:px-7 sm:text-base">
              {settings.consultationInfo}
            </p>
          ) : null}
        </div>

        <aside>
          <div className="clinic-card rounded-[1.25rem] p-6 sm:p-8 lg:sticky lg:top-24">
            <h2 className="text-xl font-extrabold tracking-tight">طلب استشارة</h2>
            <p className="mt-2 text-sm leading-7 text-muted">{consultationPageCopy.note}</p>
            <div className="mt-6">
              <ConsultationForm />
            </div>
          </div>
        </aside>
      </div>
    </div>
  );
}
