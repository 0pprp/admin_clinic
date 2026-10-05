import Image from "next/image";
import Link from "next/link";
import { brand } from "@/lib/content/brand";

export function Wordmark({ inverted = false }: { inverted?: boolean }) {
  return (
    <Link
      href="/"
      className="group inline-flex max-w-full items-center gap-2 sm:gap-3"
      aria-label={`${brand.nameAr} — الصفحة الرئيسية`}
    >
      <Image
        src="/brand/logo.png"
        alt=""
        width={40}
        height={40}
        className="h-8 w-8 shrink-0 rounded-sm object-contain sm:h-10 sm:w-10"
        priority
      />
      <span className="inline-flex min-w-0 flex-col items-start">
        <span
          className={`truncate text-[0.95rem] font-semibold leading-none tracking-tight sm:text-lg ${
            inverted ? "text-primary-foreground" : "text-foreground"
          }`}
        >
          {brand.nameAr}
        </span>
        <span
          className={`mt-1.5 hidden max-w-[14rem] truncate text-[10px] leading-none sm:block sm:text-[11px] ${
            inverted ? "text-primary-foreground/70" : "text-muted"
          }`}
        >
          {brand.taglineAr}
        </span>
      </span>
    </Link>
  );
}
