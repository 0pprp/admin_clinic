"use client";

import { useEffect } from "react";
import { Button } from "@/components/ui/clinic";

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
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-surface-dark/55 p-4 backdrop-blur-sm" role="presentation">
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby="confirm-dialog-title"
        className="clinic-card w-full max-w-md p-6 text-foreground shadow-[0_24px_64px_rgba(15,23,42,0.22)]"
      >
        <p className="text-xs font-bold text-accent">تأكيد الإجراء</p>
        <h2 id="confirm-dialog-title" className="mt-2 text-lg font-extrabold tracking-tight">
          {title}
        </h2>
        {description ? <p className="mt-3 text-sm leading-7 text-muted">{description}</p> : null}
        {children ? <div className="mt-4">{children}</div> : null}
        <div className="mt-6 flex flex-wrap justify-end gap-3">
          <Button type="button" variant="soft" size="md" disabled={pending} onClick={onClose}>
            {cancelLabel}
          </Button>
          <Button
            type="button"
            variant={tone === "danger" ? "outline" : "accent"}
            size="md"
            disabled={pending}
            onClick={onConfirm}
            className={tone === "danger" ? "border-red-300 text-red-800 hover:border-red-400 hover:bg-red-50 hover:text-red-900" : undefined}
          >
            {pending ? "جاري التنفيذ..." : confirmLabel}
          </Button>
        </div>
      </div>
    </div>
  );
}
