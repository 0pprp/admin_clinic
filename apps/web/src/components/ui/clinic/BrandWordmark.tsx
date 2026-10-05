import Link from "next/link";
import { brand } from "@/lib/content/brand";
import { BrandMark } from "./BrandMark";
import { cn } from "./cn";

export function BrandWordmark({
  inverted = false,
  compact = false,
  href = "/"
}: {
  inverted?: boolean;
  compact?: boolean;
  href?: string;
}) {
  return (
    <Link
      href={href}
      className="group inline-flex max-w-full items-center gap-2.5 sm:gap-3"
      aria-label={`${brand.nameAr} — الصفحة الرئيسية`}
    >
      <BrandMark className={cn(compact ? "h-8 w-8" : "h-9 w-9 sm:h-10 sm:w-10")} />
      <span className="inline-flex min-w-0 flex-col items-start">
        <span
          className={cn(
            "truncate font-semibold leading-none tracking-tight",
            compact ? "text-sm sm:text-base" : "text-[0.95rem] sm:text-lg",
            inverted ? "text-primary-foreground" : "text-foreground"
          )}
        >
          {brand.nameAr}
        </span>
        {!compact ? (
          <span
            className={cn(
              "mt-1.5 hidden max-w-[15rem] truncate text-[10px] leading-none sm:block sm:text-[11px]",
              inverted ? "text-primary-foreground/70" : "text-muted"
            )}
          >
            {brand.taglineAr}
          </span>
        ) : null}
      </span>
    </Link>
  );
}
