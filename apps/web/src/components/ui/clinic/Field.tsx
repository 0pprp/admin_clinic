import type { InputHTMLAttributes, ReactNode, TextareaHTMLAttributes } from "react";
import { cn } from "./cn";

export const clinicInputClassName =
  "w-full rounded-md border border-border bg-surface px-3.5 py-2.5 text-sm text-foreground outline-none transition placeholder:text-muted/70 focus:border-accent focus:ring-2 focus:ring-accent/20";

export function ClinicField({
  label,
  hint,
  children,
  className
}: {
  label: string;
  hint?: string;
  children: ReactNode;
  className?: string;
}) {
  return (
    <label className={cn("mb-4 block", className)}>
      <span className="mb-1.5 block text-sm font-medium text-foreground">{label}</span>
      {children}
      {hint ? <span className="mt-1.5 block text-xs text-muted">{hint}</span> : null}
    </label>
  );
}

export function ClinicInput(props: InputHTMLAttributes<HTMLInputElement>) {
  return <input {...props} className={cn(clinicInputClassName, props.className)} />;
}

export function ClinicTextarea(props: TextareaHTMLAttributes<HTMLTextAreaElement>) {
  return <textarea {...props} className={cn(clinicInputClassName, "min-h-28 resize-y", props.className)} />;
}
