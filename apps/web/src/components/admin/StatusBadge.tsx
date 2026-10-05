import { type BadgeTone } from "@/lib/admin/labels";
import { cn } from "@/components/ui/clinic/cn";

const toneClass: Record<BadgeTone, string> = {
  neutral: "border-border bg-surface text-foreground",
  warning: "border-[#8a5a12]/20 bg-[#fff3df] text-[#8a5a12]",
  success: "border-accent/30 bg-[#ffe8de] text-accent",
  danger: "border-red-200 bg-red-50 text-red-800"
};

export function StatusBadge({
  label,
  tone = "neutral"
}: {
  label: string;
  tone?: BadgeTone;
}) {
  return (
    <span className={cn("inline-flex rounded-md border px-2.5 py-1 text-xs font-medium", toneClass[tone])}>
      {label}
    </span>
  );
}
