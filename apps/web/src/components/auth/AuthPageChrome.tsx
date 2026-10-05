import type { ReactNode } from "react";
import { SiteFooter } from "@/components/layout/SiteFooter";
import { SiteHeader } from "@/components/layout/SiteHeader";
import { fetchPublic } from "@/lib/api/public";
import type { PublicSiteSettings } from "@/lib/api/public-types";

/** غلاف صفحات المصادقة — يطابق P09/P10/P11 في Figma (هيدر + فوتر الموقع) */
export async function AuthPageChrome({ children }: { children: ReactNode }) {
  const settings = await fetchPublic<PublicSiteSettings>("/api/public/site-settings");

  return (
    <>
      <SiteHeader />
      <main id="main" className="flex-1 bg-background">
        {children}
      </main>
      <SiteFooter settings={settings} />
    </>
  );
}
