export function BrandSlogan({ children, className = "" }: { children: React.ReactNode; className?: string }) {
  return <p className={`brand-accent-label ${className}`}>{children}</p>;
}
