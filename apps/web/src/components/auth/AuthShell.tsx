import type { ReactNode } from "react";
import { BrandMark } from "@/components/ui/clinic";

export function AuthShell({
  title,
  description,
  children
}: {
  title: string;
  description?: string;
  children: ReactNode;
}) {
  return (
    <div className="clinic-shell flex flex-col items-center px-5 py-12 sm:py-16">
      <div className="w-full max-w-md text-center">
        <h1 className="text-3xl font-extrabold tracking-tight text-foreground sm:text-4xl">{title}</h1>
        {description ? <p className="mt-3 text-sm leading-7 text-muted sm:text-base">{description}</p> : null}
      </div>
      <div className="mt-8 w-full max-w-md rounded-[1.25rem] border border-border bg-surface p-7 shadow-[0_12px_40px_rgba(15,23,42,0.06)] sm:p-8">
        <div className="mb-6 flex justify-center">
          <BrandMark className="h-11 w-11" />
        </div>
        {children}
      </div>
    </div>
  );
}

export function Field({ label, children }: { label: string; children: ReactNode }) {
  return (
    <label className="mb-4 block text-start">
      <span className="mb-1.5 block text-sm font-bold text-foreground">{label}</span>
      {children}
    </label>
  );
}

export { clinicInputClassName as inputClassName } from "@/components/ui/clinic/Field";

export const buttonClassName =
  "w-full rounded-xl bg-accent px-4 py-3 text-sm font-semibold text-primary-foreground transition hover:bg-accent-soft disabled:opacity-60";
