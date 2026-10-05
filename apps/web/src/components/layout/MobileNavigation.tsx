"use client";

import Link from "next/link";
import { useEffect, useId, useRef, useState } from "react";
import { createPortal } from "react-dom";
import { navItems } from "@/lib/content/brand";
import { BrandWordmark } from "@/components/ui/clinic";

export function MobileNavigation({ hasSession }: { hasSession: boolean }) {
  const [open, setOpen] = useState(false);
  const [mounted, setMounted] = useState(false);
  const dialogId = useId();
  const openButtonRef = useRef<HTMLButtonElement>(null);
  const closeButtonRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    setMounted(true);
  }, []);

  useEffect(() => {
    if (!open) {
      return;
    }

    closeButtonRef.current?.focus();

    function onKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") {
        setOpen(false);
        openButtonRef.current?.focus();
      }
    }

    document.addEventListener("keydown", onKeyDown);
    const previousOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    return () => {
      document.removeEventListener("keydown", onKeyDown);
      document.body.style.overflow = previousOverflow;
    };
  }, [open]);

  function closeMenu() {
    setOpen(false);
    openButtonRef.current?.focus();
  }

  const drawer =
    open && mounted
      ? createPortal(
          <div className="fixed inset-0 z-[100] lg:hidden">
            <button type="button" className="absolute inset-0 bg-black/45" aria-label="إغلاق القائمة" onClick={closeMenu} />
            <div
              id={dialogId}
              role="dialog"
              aria-modal="true"
              aria-label="قائمة التنقل"
              className="absolute inset-y-0 start-0 flex w-[min(22rem,92vw)] flex-col rounded-e-2xl border-e border-border bg-surface shadow-[0_0_40px_rgba(15,23,42,0.18)]"
            >
              <div className="flex items-center justify-between border-b border-border px-5 py-4">
                <BrandWordmark compact />
                <button
                  ref={closeButtonRef}
                  type="button"
                  className="rounded-xl border border-border px-3 py-1.5 text-sm text-foreground"
                  onClick={closeMenu}
                >
                  إغلاق
                </button>
              </div>
              <nav className="flex flex-1 flex-col gap-1 overflow-y-auto px-3 py-4 text-base text-foreground">
                {navItems.map((item) => (
                  <Link
                    key={item.href}
                    href={item.href}
                    className="rounded-xl px-3 py-3 transition hover:bg-surface-warm"
                    onClick={closeMenu}
                  >
                    {item.label}
                  </Link>
                ))}
              </nav>
              <div className="space-y-3 border-t border-border px-5 py-5">
                <Link
                  href={hasSession ? "/dashboard" : "/login"}
                  className="block text-sm font-medium text-foreground"
                  onClick={closeMenu}
                >
                  {hasSession ? "لوحة التحكم" : "تسجيل الدخول"}
                </Link>
                <Link
                  href="/courses"
                  className="inline-flex w-full items-center justify-center rounded-xl bg-accent px-4 py-3 text-sm font-semibold text-primary-foreground"
                  onClick={closeMenu}
                >
                  استكشف الكورسات
                </Link>
              </div>
            </div>
          </div>,
          document.body
        )
      : null;

  return (
    <div className="lg:hidden">
      <button
        ref={openButtonRef}
        type="button"
        className="inline-flex h-11 w-11 items-center justify-center rounded-xl border border-border bg-surface text-foreground"
        aria-expanded={open}
        aria-controls={dialogId}
        aria-label={open ? "إغلاق القائمة" : "فتح القائمة"}
        onClick={() => setOpen((value) => !value)}
      >
        <span aria-hidden="true" className="relative block h-4 w-5">
          <span className={`absolute start-0 top-0 block h-0.5 w-5 rounded-full bg-current transition ${open ? "top-1.5 rotate-45" : ""}`} />
          <span className={`absolute start-0 top-1.5 block h-0.5 w-5 rounded-full bg-current transition ${open ? "opacity-0" : ""}`} />
          <span className={`absolute start-0 top-3 block h-0.5 w-5 rounded-full bg-current transition ${open ? "top-1.5 -rotate-45" : ""}`} />
        </span>
      </button>
      {drawer}
    </div>
  );
}
