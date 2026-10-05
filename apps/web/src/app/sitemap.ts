import type { MetadataRoute } from "next";
import { fetchPublic } from "@/lib/api/public";
import type { ArticleSummary, CourseSummary, Paged } from "@/lib/api/public-types";
import { absoluteUrl } from "@/lib/seo";

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const staticRoutes = ["/", "/about", "/courses", "/articles", "/consultation", "/faq", "/contact", "/privacy", "/terms"];
  const courses = await fetchPublic<Paged<CourseSummary>>("/api/public/courses?page=1&pageSize=24");
  const articles = await fetchPublic<Paged<ArticleSummary>>("/api/public/articles?page=1&pageSize=24");

  const dynamicRoutes = [
    ...(courses?.items ?? []).map((course) => `/courses/${course.slug}`),
    ...(articles?.items ?? []).map((article) => `/articles/${article.slug}`)
  ];

  return [...staticRoutes, ...dynamicRoutes].map((path) => ({
    url: absoluteUrl(path),
    lastModified: new Date()
  }));
}
