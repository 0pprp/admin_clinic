import { cn } from "./cn";

/** الشعار الهندسي البرتقالي — مطابق لعلامة Figma */
export function BrandMark({ className = "", title }: { className?: string; title?: string }) {
  return (
    <svg
      viewBox="0 0 48 48"
      className={cn("h-10 w-10 shrink-0", className)}
      aria-hidden={title ? undefined : true}
      role={title ? "img" : undefined}
    >
      {title ? <title>{title}</title> : null}
      <path fill="#F97316" d="M24 2.5 29.8 18.2 45.5 24 29.8 29.8 24 45.5 18.2 29.8 2.5 24l15.7-5.8z" />
      <path fill="#FB923C" d="M24 10.5 27.4 20.6 37.5 24 27.4 27.4 24 37.5 20.6 27.4 10.5 24l10.1-3.4z" />
      <circle cx="24" cy="24" r="3.2" fill="#0F172A" />
    </svg>
  );
}
