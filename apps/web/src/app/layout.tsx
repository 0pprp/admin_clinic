import type { Metadata } from "next";
import { Amiri } from "next/font/google";
import localFont from "next/font/local";
import { brand } from "@/lib/content/brand";
import { siteUrl } from "@/lib/seo";
import "./globals.css";

const arabicSans = localFont({
  src: [
    { path: "../fonts/ibm-plex-sans-arabic-400.woff2", weight: "400", style: "normal" },
    { path: "../fonts/ibm-plex-sans-arabic-600.woff2", weight: "600", style: "normal" },
    { path: "../fonts/ibm-plex-sans-arabic-700.woff2", weight: "700", style: "normal" }
  ],
  variable: "--font-arabic-sans",
  display: "swap"
});

const latinSans = localFont({
  src: [
    { path: "../fonts/ibm-plex-sans-400.woff2", weight: "400", style: "normal" },
    { path: "../fonts/ibm-plex-sans-600.woff2", weight: "600", style: "normal" }
  ],
  variable: "--font-latin-sans",
  display: "swap"
});

/** خط ناسخ متصل لشعار: شخّص . عالج . طوّر */
const arabicNaskh = Amiri({
  subsets: ["arabic", "latin"],
  weight: ["400", "700"],
  variable: "--font-arabic-naskh",
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
    <html
      lang="ar"
      dir="rtl"
      className={`${arabicSans.variable} ${latinSans.variable} ${arabicNaskh.variable} h-full antialiased`}
    >
      <body className={`${arabicSans.className} flex min-h-full flex-col`}>
        <a href="#main" className="skip-link">
          تخطي إلى المحتوى
        </a>
        {children}
      </body>
    </html>
  );
}
