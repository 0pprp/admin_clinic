import type { Metadata } from "next";
import { brand } from "@/lib/content/brand";

export const siteUrl = (process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3000").replace(/\/$/, "");

export function absoluteUrl(path: string): string {
  if (path.startsWith("http")) {
    return path;
  }

  return `${siteUrl}${path.startsWith("/") ? path : `/${path}`}`;
}

export function createPageMetadata({
  title,
  description,
  path,
  type = "website",
  image
}: {
  title: string;
  description: string;
  path: string;
  type?: "website" | "article";
  image?: string | null;
}): Metadata {
  const url = absoluteUrl(path);
  const ogImage = image ? [{ url: image }] : undefined;

  return {
    title,
    description,
    alternates: { canonical: url },
    openGraph: {
      title,
      description,
      url,
      locale: "ar_IQ",
      siteName: brand.nameAr,
      type,
      images: ogImage
    },
    twitter: {
      card: "summary_large_image",
      title,
      description
    }
  };
}
