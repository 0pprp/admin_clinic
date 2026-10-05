export function BrandAccentLabel({
  children,
  className = "",
  as: Tag = "p"
}: {
  children: React.ReactNode;
  className?: string;
  as?: "p" | "span" | "div";
}) {
  return <Tag className={`brand-accent-label ${className}`}>{children}</Tag>;
}
