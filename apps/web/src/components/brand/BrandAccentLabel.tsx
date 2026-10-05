import type { ReactNode } from "react";

type BrandAccentLabelProps = {
  children: ReactNode;
  as?: "p" | "span";
  className?: string;
};

/**
 * تسميات الهوية بلون الشعار + خط Amiri المتصل.
 * باقي الواجهة (روابط/أزرار/نصوص) تبقى بخط IBM Plex.
 */
export function BrandAccentLabel({
  children,
  as = "p",
  className = ""
}: BrandAccentLabelProps) {
  const Tag = as;
  return <Tag className={`brand-accent-label ${className}`.trim()}>{children}</Tag>;
}
