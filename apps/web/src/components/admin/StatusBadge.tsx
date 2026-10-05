import { type BadgeTone } from "@/lib/admin/labels";

const toneClass: Record<BadgeTone, string> = {
  neutral: "border-border bg-surface text-foreground",
  warning: "border-border bg-surface-warm text-foreground",
  success: "border-accent bg-surface text-foreground",
  danger: "border-foreground bg-surface text-foreground"
};

export function StatusBadge({
  label,
  tone = "neutral"
}: {
  label: string;
  tone?: BadgeTone;
}) {
  return (
    <span className={`inline-flex border px-2 py-0.5 text-xs ${toneClass[tone]}`}>
      {label}
    </span>
  );
}
