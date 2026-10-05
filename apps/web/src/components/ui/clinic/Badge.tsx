import type { ReactNode } from "react";
import { cn } from "./cn";

const tones = {
  navy: "bg-[#e8eef5] text-primary",
  orange: "bg-[#ffe8de] text-accent",
  mint: "bg-[#e6f6ef] text-[#0f6b4c]",
  sand: "bg-[#fff3df] text-[#8a5a12]",
  rose: "bg-[#fde8ef] text-[#9b1d4a]",
  sky: "bg-[#e5f2ff] text-[#145a9c]"
} as const;

export function Badge({
  children,
  tone = "navy",
  className
}: {
  children: ReactNode;
  tone?: keyof typeof tones;
  className?: string;
}) {
  return (
    <span className={cn("inline-flex rounded-full px-2.5 py-1 text-[11px] font-medium", tones[tone], className)}>
      {children}
    </span>
  );
}
