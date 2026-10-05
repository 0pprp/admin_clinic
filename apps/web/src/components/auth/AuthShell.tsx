import type { ReactNode } from "react";
import { BrandWordmark } from "@/components/ui/clinic";
import { clinicInputClassName } from "@/components/ui/clinic/Field";

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
    <main id="main" className="flex min-h-full flex-1 items-center justify-center bg-background px-5 py-12 text-foreground">
      <div className="w-full max-w-md rounded-[1.5rem] border border-border bg-surface p-7 shadow-[0_20px_60px_rgba(15,23,42,0.08)] sm:p-8">
        <BrandWordmark />
        <h1 className="mt-8 text-2xl font-extrabold tracking-tight">{title}</h1>
        {description ? <p className="mt-2 text-sm leading-7 text-muted">{description}</p> : null}
        <div className="mt-6">{children}</div>
      </div>
    </main>
  );
}

export function Field({ label, children }: { label: string; children: ReactNode }) {
  return (
    <label className="mb-4 block">
      <span className="mb-1.5 block text-sm font-medium">{label}</span>
      {children}
    </label>
  );
}

export const inputClassName = clinicInputClassName;

export const buttonClassName =
  "w-full rounded-xl bg-accent px-4 py-2.5 text-sm font-semibold text-primary-foreground transition hover:bg-accent-soft disabled:opacity-60";
