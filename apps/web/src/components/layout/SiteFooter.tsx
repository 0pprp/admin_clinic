import Link from "next/link";
import { brand } from "@/lib/content/brand";
import type { PublicSiteSettings } from "@/lib/api/public-types";
import { BrandWordmark } from "@/components/ui/clinic";

const footerLinks = [
  { href: "/contact", label: "تواصل معنا" },
  { href: "/faq", label: "الأسئلة الشائعة" },
  { href: "/terms", label: "الشروط والأحكام" },
  { href: "/privacy", label: "الخصوصية" }
] as const;

export function SiteFooter({ settings }: { settings: PublicSiteSettings | null }) {
  const year = new Date().getFullYear();

  return (
    <footer className="mt-auto bg-surface-dark text-primary-foreground">
      <div className="clinic-shell flex flex-col gap-8 py-10 sm:flex-row sm:items-center sm:justify-between sm:py-12">
        <div>
          <BrandWordmark inverted />
          <p className="mt-4 max-w-md text-sm leading-7 text-primary-foreground/65">
            {settings?.footerText ?? brand.siteDescription}
          </p>
        </div>
        <nav aria-label="روابط التذييل" className="flex flex-wrap gap-x-6 gap-y-3 text-sm text-primary-foreground/80">
          {footerLinks.map((item) => (
            <Link key={item.href} href={item.href} className="transition hover:text-accent-soft">
              {item.label}
            </Link>
          ))}
        </nav>
      </div>
      <div className="border-t border-white/10">
        <div className="clinic-shell py-4 text-xs text-primary-foreground/45">
          © {year} {settings?.brandName ?? brand.nameAr}. جميع الحقوق محفوظة.
        </div>
      </div>
    </footer>
  );
}
