import type { ReactNode } from "react";
import { cn } from "./cn";

export function Metric({
  value,
  label,
  className
}: {
  value: ReactNode;
  label: string;
  className?: string;
}) {
  return (
    <div className={cn("rounded-md border border-border bg-surface px-5 py-4", className)}>
      <p className="text-2xl font-semibold tracking-tight text-primary sm:text-3xl">{value}</p>
      <p className="mt-1 text-sm text-muted">{label}</p>
    </div>
  );
}
