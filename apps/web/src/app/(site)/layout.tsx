import type { ReactNode } from "react";
import { SiteFooter } from "@/components/layout/SiteFooter";
import { SiteHeader } from "@/components/layout/SiteHeader";
import { fetchPublic } from "@/lib/api/public";
import type { PublicSiteSettings } from "@/lib/api/public-types";
import { brand } from "@/lib/content/brand";
import { absoluteUrl } from "@/lib/seo";

function JsonLd() {
  const data = {
    "@context": "https://schema.org",
    "@graph": [
      {
        "@type": "WebSite",
        name: brand.nameAr,
        alternateName: brand.nameEn,
        url: absoluteUrl("/"),
        inLanguage: "ar"
      },
      {
        "@type": "Organization",
        name: brand.nameAr,
        alternateName: brand.nameEn,
        description: brand.taglineAr,
        url: absoluteUrl("/"),
        sameAs: []
      }
    ]
  };

  return (
    <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: JSON.stringify(data) }} />
  );
}

export default async function SiteLayout({ children }: { children: ReactNode }) {
  const settings = await fetchPublic<PublicSiteSettings>("/api/public/site-settings");

  return (
    <>
      <JsonLd />
      <SiteHeader />
      <main id="main" className="flex-1">
        {children}
      </main>
      <SiteFooter settings={settings} />
    </>
  );
}
