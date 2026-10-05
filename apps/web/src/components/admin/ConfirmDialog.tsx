"use client";

import { useEffect } from "react";
import { dangerButtonClassName, secondaryButtonClassName } from "@/lib/admin/ui";

export function ConfirmDialog({
  open,
  title,
  description,
  confirmLabel = "تأكيد",
  cancelLabel = "إلغاء",
  pending = false,
  tone = "default",
  children,
  onConfirm,
  onClose
}: {
  open: boolean;
  title: string;
  description?: string;
  confirmLabel?: string;
  cancelLabel?: string;
  pending?: boolean;
  tone?: "default" | "danger";
  children?: React.ReactNode;
  onConfirm: () => void;
  onClose: () => void;
}) {
  useEffect(() => {
    if (!open) {
      return;
    }

    function onKey(event: KeyboardEvent) {
      if (event.key === "Escape" && !pending) {
        onClose();
      }
    }

    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [open, pending, onClose]);

  if (!open) {
    return null;
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-surface-dark/50 p-4" role="presentation">
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby="confirm-dialog-title"
        className="w-full max-w-md border border-border bg-surface p-6 text-foreground shadow-lg"
      >
        <h2 id="confirm-dialog-title" className="text-lg font-semibold">
          {title}
        </h2>
        {description ? <p className="mt-3 text-sm leading-7 text-muted">{description}</p> : null}
        {children ? <div className="mt-4">{children}</div> : null}
        <div className="mt-6 flex flex-wrap justify-end gap-3">
          <button type="button" className={secondaryButtonClassName} disabled={pending} onClick={onClose}>
            {cancelLabel}
          </button>
          <button
            type="button"
            className={tone === "danger" ? dangerButtonClassName : "inline-flex items-center justify-center border border-foreground bg-foreground px-4 py-2 text-sm text-primary-foreground disabled:opacity-50"}
            disabled={pending}
            onClick={onConfirm}
          >
            {pending ? "جاري التنفيذ..." : confirmLabel}
          </button>
        </div>
      </div>
    </div>
  );
}
