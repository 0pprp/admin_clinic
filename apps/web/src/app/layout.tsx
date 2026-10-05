import type { Metadata } from "next";
import { Cairo } from "next/font/google";
import { brand } from "@/lib/content/brand";
import { siteUrl } from "@/lib/seo";
import "./globals.css";

const cairo = Cairo({
  subsets: ["arabic", "latin"],
  weight: ["400", "500", "600", "700", "800"],
  variable: "--font-cairo",
  display: "swap"
});

export const metadata: Metadata = {
  metadataBase: new URL(siteUrl),
  title: {
    default: brand.siteTitle,
    template: `%s | ${brand.siteTitle}`
  },
  description: brand.siteDescription,
  openGraph: {
    siteName: brand.nameAr,
    locale: "ar_IQ",
    type: "website"
  }
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="ar" dir="rtl" className={`${cairo.variable} h-full antialiased`}>
      <body className={`${cairo.className} flex min-h-full flex-col`}>
        <a href="#main" className="skip-link">
          تخطي إلى المحتوى
        </a>
        {children}
      </body>
    </html>
  );
}
