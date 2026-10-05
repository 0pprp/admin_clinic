import { type BadgeTone } from "@/lib/admin/labels";
import { cn } from "@/components/ui/clinic/cn";

const toneClass: Record<BadgeTone, string> = {
  neutral: "bg-[#e8eef5] text-primary",
  warning: "bg-[#fff3df] text-[#8a5a12]",
  success: "bg-[#ffe8de] text-accent",
  danger: "bg-[#fde8ef] text-[#9b1d4a]"
};

export function StatusBadge({
  label,
  tone = "neutral"
}: {
  label: string;
  tone?: BadgeTone;
}) {
  return (
    <span className={cn("inline-flex rounded-full px-2.5 py-1 text-[11px] font-semibold", toneClass[tone])}>
      {label}
    </span>
  );
}
