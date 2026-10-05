import { clinicInputClassName } from "@/components/ui/clinic/Field";

/** Clinic form controls — used across admin list/detail pages */
export const inputClassName = clinicInputClassName;

export const selectClassName = inputClassName;

export const textareaClassName = `${inputClassName} min-h-24 resize-y`;

/** Orange primary CTA (Figma admin actions) */
export const primaryButtonClassName =
  "inline-flex items-center justify-center gap-2 rounded-xl border border-transparent bg-accent px-4 py-2.5 text-sm font-semibold text-primary-foreground transition hover:bg-accent-soft disabled:opacity-60";

export const secondaryButtonClassName =
  "inline-flex items-center justify-center gap-2 rounded-xl border border-border bg-surface px-4 py-2.5 text-sm font-semibold text-foreground transition hover:border-accent hover:text-accent disabled:opacity-60";

export const dangerButtonClassName =
  "inline-flex items-center justify-center gap-2 rounded-xl border border-red-200 bg-red-50 px-4 py-2.5 text-sm font-semibold text-red-800 transition hover:border-red-300 hover:bg-red-100 disabled:opacity-60";

export const linkButtonClassName = "text-sm font-semibold text-accent hover:underline";

export const adminFormPanelClassName = "clinic-card mb-8 grid gap-4 p-5 sm:grid-cols-2";

export const adminTableHeadClassName = "bg-surface-warm/80 text-start text-xs font-bold text-muted";

export const adminTableCellClassName = "px-4 py-3.5";
