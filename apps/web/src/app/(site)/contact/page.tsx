import { BrandAccentLabel } from "@/components/brand/BrandAccentLabel";
import { Container } from "@/components/shared/Container";
import { PageIntro } from "@/components/shared/PageIntro";
import { ContactForm } from "@/components/public/ContactForm";
import { fetchPublic } from "@/lib/api/public";
import type { PublicSiteSettings } from "@/lib/api/public-types";
import { parseSocialLinks } from "@/lib/social-links";
import { createPageMetadata } from "@/lib/seo";

export const metadata = createPageMetadata({
  title: "تواصل معنا",
  description: "بيانات التواصل العامة ونموذج المراسلة المعتمد للعيادة الإدارية.",
  path: "/contact"
});

export default async function ContactPage() {
  const settings = await fetchPublic<PublicSiteSettings>("/api/public/site-settings");
  const social = parseSocialLinks(settings?.socialLinks);
  const hasAny =
    Boolean(settings?.publicPhone || settings?.publicWhatsApp || settings?.publicEmail) || social.length > 0;

  return (
    <>
      <PageIntro
        eyebrow="تواصل معنا"
        title="يمكن التواصل عبر القنوات المعتمدة أو من خلال النموذج."
        description="أرسل رسالتك عبر النموذج، أو استخدم بيانات التواصل العامة المعتمدة."
      />
      <Container className="space-y-16 py-16">
        {hasAny ? (
          <ul className="max-w-xl space-y-6 text-lg">
            {settings?.publicPhone ? (
              <li>
                <BrandAccentLabel className="text-sm font-bold tracking-wide">الهاتف</BrandAccentLabel>
                <a href={`tel:${settings.publicPhone}`} className="mt-2 block hover:text-accent">
                  {settings.publicPhone}
                </a>
              </li>
            ) : null}
            {settings?.publicWhatsApp ? (
              <li>
                <BrandAccentLabel className="text-sm font-bold tracking-wide">واتساب</BrandAccentLabel>
                <a
                  href={`https://wa.me/${settings.publicWhatsApp.replace(/\D/g, "")}`}
                  className="mt-2 block hover:text-accent"
                  rel="noreferrer"
                  target="_blank"
                >
                  {settings.publicWhatsApp}
                </a>
              </li>
            ) : null}
            {settings?.publicEmail ? (
              <li>
                <BrandAccentLabel className="text-sm font-bold tracking-wide">البريد</BrandAccentLabel>
                <a href={`mailto:${settings.publicEmail}`} className="mt-2 block hover:text-accent">
                  {settings.publicEmail}
                </a>
              </li>
            ) : null}
            {social.length > 0 ? (
              <li>
                <BrandAccentLabel className="text-sm font-bold tracking-wide">حسابات عامة</BrandAccentLabel>
                <ul className="mt-3 space-y-2">
                  {social.map((item) => (
                    <li key={item.href}>
                      <a href={item.href} className="hover:text-accent" rel="noreferrer" target="_blank">
                        {item.label}
                      </a>
                    </li>
                  ))}
                </ul>
              </li>
            ) : null}
          </ul>
        ) : (
          <p className="max-w-xl text-base leading-8 text-muted">
            بيانات التواصل العامة ستظهر هنا بعد اعتمادها في إعدادات الموقع. يمكنك استخدام النموذج أدناه لإرسال رسالة.
          </p>
        )}
        <section className="border-t border-border pt-16">
          <h2 className="text-3xl font-semibold">أرسل رسالة</h2>
          <div className="mt-8">
            <ContactForm />
          </div>
        </section>
      </Container>
    </>
  );
}
