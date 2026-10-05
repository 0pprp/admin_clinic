"use client";

import { useEffect, useId, useRef, useState } from "react";
import { cn } from "./cn";
import { clinicInputClassName } from "./Field";

export type ClinicSelectOption = { value: string; label: string };

type ClinicSelectProps = {
  options: ClinicSelectOption[];
  value?: string;
  defaultValue?: string;
  onChange?: (value: string) => void;
  name?: string;
  placeholder?: string;
  disabled?: boolean;
  className?: string;
  id?: string;
};

export function ClinicSelect({
  options,
  value,
  defaultValue = "",
  onChange,
  name,
  placeholder = "اختر…",
  disabled = false,
  className,
  id
}: ClinicSelectProps) {
  const autoId = useId();
  const listId = `${autoId}-list`;
  const rootRef = useRef<HTMLDivElement>(null);
  const isControlled = value !== undefined;
  const [internal, setInternal] = useState(defaultValue);
  const selected = isControlled ? value : internal;
  const [open, setOpen] = useState(false);

  useEffect(() => {
    if (!open) return;
    function onDoc(event: MouseEvent) {
      if (!rootRef.current?.contains(event.target as Node)) {
        setOpen(false);
      }
    }
    function onKey(event: KeyboardEvent) {
      if (event.key === "Escape") setOpen(false);
    }
    document.addEventListener("mousedown", onDoc);
    document.addEventListener("keydown", onKey);
    return () => {
      document.removeEventListener("mousedown", onDoc);
      document.removeEventListener("keydown", onKey);
    };
  }, [open]);

  const selectedOption = options.find((item) => item.value === selected);

  function choose(next: string) {
    if (!isControlled) setInternal(next);
    onChange?.(next);
    setOpen(false);
  }

  return (
    <div ref={rootRef} className={cn("relative", className)}>
      {name ? <input type="hidden" name={name} value={selected} /> : null}
      <button
        type="button"
        id={id}
        disabled={disabled}
        aria-haspopup="listbox"
        aria-expanded={open}
        aria-controls={listId}
        className={cn(
          clinicInputClassName,
          "flex items-center justify-between gap-3 text-start",
          open && "border-accent ring-2 ring-accent/20",
          disabled && "cursor-not-allowed opacity-60"
        )}
        onClick={() => setOpen((current) => !current)}
      >
        <span className={cn("truncate", !selectedOption && "text-muted/70")}>
          {selectedOption?.label ?? placeholder}
        </span>
        <svg
          viewBox="0 0 20 20"
          className={cn("h-4 w-4 shrink-0 text-muted transition", open && "rotate-180 text-accent")}
          aria-hidden="true"
        >
          <path
            fill="currentColor"
            d="M5.3 7.3a1 1 0 0 1 1.4 0L10 10.6l3.3-3.3a1 1 0 1 1 1.4 1.4l-4 4a1 1 0 0 1-1.4 0l-4-4a1 1 0 0 1 0-1.4Z"
          />
        </svg>
      </button>
      {open ? (
        <ul
          id={listId}
          role="listbox"
          className="absolute inset-x-0 z-50 mt-2 max-h-60 overflow-auto rounded-2xl border border-border bg-surface p-1.5 shadow-[0_18px_50px_rgba(15,23,42,0.14)]"
        >
          {options.map((option) => {
            const active = option.value === selected;
            return (
              <li key={option.value || "__empty"}>
                <button
                  type="button"
                  role="option"
                  aria-selected={active}
                  className={cn(
                    "flex w-full items-center rounded-xl px-3 py-2.5 text-start text-sm transition",
                    active
                      ? "bg-accent font-semibold text-primary-foreground"
                      : "text-foreground hover:bg-surface-warm hover:text-foreground"
                  )}
                  onClick={() => choose(option.value)}
                >
                  {option.label}
                </button>
              </li>
            );
          })}
        </ul>
      ) : null}
    </div>
  );
}
