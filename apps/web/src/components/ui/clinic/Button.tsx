import Link from "next/link";
import type { ButtonHTMLAttributes, ReactNode } from "react";
import { cn } from "./cn";

const variants = {
  primary: "border-transparent bg-primary text-primary-foreground hover:bg-[#0a2744]",
  accent: "border-transparent bg-accent text-primary-foreground hover:bg-accent-soft",
  outline: "border-primary bg-transparent text-primary hover:bg-primary hover:text-primary-foreground",
  ghost: "border-transparent bg-transparent text-primary hover:bg-surface-warm",
  soft: "border-border bg-surface text-foreground hover:border-accent hover:text-accent"
} as const;

const sizes = {
  sm: "px-3 py-1.5 text-xs",
  md: "px-4 py-2.5 text-sm",
  lg: "px-5 py-3 text-sm"
} as const;

type Variant = keyof typeof variants;
type Size = keyof typeof sizes;

function buttonClasses(variant: Variant, size: Size, className?: string) {
  return cn(
    "inline-flex items-center justify-center gap-2 rounded-md border font-medium transition disabled:opacity-60",
    variants[variant],
    sizes[size],
    className
  );
}

export function Button({
  variant = "primary",
  size = "md",
  className,
  children,
  ...props
}: ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: Variant;
  size?: Size;
  children: ReactNode;
}) {
  return (
    <button type="button" className={buttonClasses(variant, size, className)} {...props}>
      {children}
    </button>
  );
}

export function ButtonLink({
  href,
  variant = "primary",
  size = "md",
  className,
  children
}: {
  href: string;
  variant?: Variant;
  size?: Size;
  className?: string;
  children: ReactNode;
}) {
  return (
    <Link href={href} className={buttonClasses(variant, size, className)}>
      {children}
    </Link>
  );
}
