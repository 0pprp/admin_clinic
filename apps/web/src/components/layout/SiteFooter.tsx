import Link from "next/link";
import { brand, navItems } from "@/lib/content/brand";
import type { PublicSiteSettings } from "@/lib/api/public-types";
import { BrandAccentLabel } from "@/components/brand/BrandAccentLabel";
import { Container } from "@/components/shared/Container";
import { Wordmark } from "./Wordmark";

export function SiteFooter({ settings }: { settings: PublicSiteSettings | null }) {
  const year = new Date().getFullYear();
  const footerLinks = navItems.filter((item) => item.href !== "/faq");

  return (
    <footer className="bg-surface-dark text-primary-foreground">
      <Container className="grid gap-12 py-14 sm:py-16 md:grid-cols-[1.2fr_1fr_1fr]">
        <div>
          <Wordmark inverted />
          <p className="mt-5 max-w-sm text-sm leading-7 text-primary-foreground/70">
            {settings?.footerText ?? brand.siteDescription}
          </p>
        </div>
        <div>
          <BrandAccentLabel className="text-sm font-bold tracking-wide">استكشف</BrandAccentLabel>
          <ul className="mt-4 space-y-3 text-sm text-primary-foreground/75">
            {footerLinks.map((item) => (
              <li key={item.href}>
                <Link href={item.href} className="transition hover:text-accent-soft">
                  {item.label}
                </Link>
              </li>
            ))}
          </ul>
        </div>
        <div>
          <BrandAccentLabel className="text-sm font-bold tracking-wide">قانوني</BrandAccentLabel>
          <ul className="mt-4 space-y-3 text-sm text-primary-foreground/75">
            <li>
              <Link href="/privacy" className="transition hover:text-accent-soft">
                سياسة الخصوصية
              </Link>
            </li>
            <li>
              <Link href="/terms" className="transition hover:text-accent-soft">
                الشروط والأحكام
              </Link>
            </li>
          </ul>
        </div>
      </Container>
      <div className="border-t border-white/10">
        <Container className="py-5 text-xs text-primary-foreground/50">
          © {year} {settings?.brandName ?? brand.nameAr}. جميع الحقوق محفوظة.
        </Container>
      </div>
    </footer>
  );
}
