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

/** P14 · تواصل — أسلوب FAQ/Contact في Figma */
export default async function ContactPage() {
  const settings = await fetchPublic<PublicSiteSettings>("/api/public/site-settings");
  const social = parseSocialLinks(settings?.socialLinks);
  const hasAny =
    Boolean(settings?.publicPhone || settings?.publicWhatsApp || settings?.publicEmail) || social.length > 0;

  return (
    <div className="clinic-shell py-10 sm:py-14">
      <PageIntro
        embedded
        eyebrow="تواصل معنا"
        title="يمكن التواصل عبر القنوات المعتمدة أو من خلال النموذج."
        description="أرسل رسالتك عبر النموذج، أو استخدم بيانات التواصل العامة المعتمدة."
      />

      <div className="mt-10 grid gap-8 lg:grid-cols-[minmax(0,18rem)_minmax(0,1fr)] lg:items-start lg:gap-10">
        <aside className="clinic-card space-y-6 p-6 sm:p-7">
          {hasAny ? (
            <ul className="space-y-5 text-sm sm:text-base">
              {settings?.publicPhone ? (
                <li>
                  <p className="text-sm font-bold text-accent">الهاتف</p>
                  <a href={`tel:${settings.publicPhone}`} className="mt-1.5 block font-semibold hover:text-accent">
                    {settings.publicPhone}
                  </a>
                </li>
              ) : null}
              {settings?.publicWhatsApp ? (
                <li>
                  <p className="text-sm font-bold text-accent">واتساب</p>
                  <a
                    href={`https://wa.me/${settings.publicWhatsApp.replace(/\D/g, "")}`}
                    className="mt-1.5 block font-semibold hover:text-accent"
                    rel="noreferrer"
                    target="_blank"
                  >
                    {settings.publicWhatsApp}
                  </a>
                </li>
              ) : null}
              {settings?.publicEmail ? (
                <li>
                  <p className="text-sm font-bold text-accent">البريد</p>
                  <a href={`mailto:${settings.publicEmail}`} className="mt-1.5 block break-all font-semibold hover:text-accent">
                    {settings.publicEmail}
                  </a>
                </li>
              ) : null}
              {social.length > 0 ? (
                <li>
                  <p className="text-sm font-bold text-accent">حسابات عامة</p>
                  <ul className="mt-2 space-y-2">
                    {social.map((item) => (
                      <li key={item.href}>
                        <a href={item.href} className="font-semibold hover:text-accent" rel="noreferrer" target="_blank">
                          {item.label}
                        </a>
                      </li>
                    ))}
                  </ul>
                </li>
              ) : null}
            </ul>
          ) : (
            <p className="text-sm leading-8 text-muted">
              بيانات التواصل العامة ستظهر هنا بعد اعتمادها. يمكنك استخدام النموذج لإرسال رسالة.
            </p>
          )}
        </aside>

        <section className="clinic-card rounded-[1.25rem] p-6 sm:p-8">
          <h2 className="text-xl font-extrabold tracking-tight">أرسل رسالة</h2>
          <div className="mt-6">
            <ContactForm />
          </div>
        </section>
      </div>
    </div>
  );
}
