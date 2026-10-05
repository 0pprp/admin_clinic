import type { ReactNode } from "react";
import { Wordmark } from "@/components/layout/Wordmark";

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
    <main id="main" className="flex min-h-full flex-1 items-center justify-center bg-background px-6 py-12 text-foreground">
      <div className="w-full max-w-md border border-border bg-surface p-8">
        <Wordmark />
        <h1 className="mt-8 text-2xl font-semibold">{title}</h1>
        {description ? <p className="mt-2 text-sm text-muted">{description}</p> : null}
        <div className="mt-6">{children}</div>
      </div>
    </main>
  );
}

export function Field({
  label,
  children
}: {
  label: string;
  children: ReactNode;
}) {
  return (
    <label className="mb-4 block">
      <span className="mb-1.5 block text-sm font-medium">{label}</span>
      {children}
    </label>
  );
}

export const inputClassName =
  "w-full border border-border bg-surface px-3 py-2.5 text-sm outline-none focus:border-foreground";

export const buttonClassName =
  "w-full bg-foreground px-4 py-2.5 text-sm font-semibold text-primary-foreground disabled:opacity-60";
