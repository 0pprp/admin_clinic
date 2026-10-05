"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { cn } from "./cn";
import { clinicInputClassName } from "./Field";

const WEEKDAYS = ["أحد", "إثنين", "ثلاثاء", "أربعاء", "خميس", "جمعة", "سبت"] as const;
const MONTHS = [
  "يناير",
  "فبراير",
  "مارس",
  "أبريل",
  "مايو",
  "يونيو",
  "يوليو",
  "أغسطس",
  "سبتمبر",
  "أكتوبر",
  "نوفمبر",
  "ديسمبر"
] as const;

function pad(n: number) {
  return String(n).padStart(2, "0");
}

function toIso(year: number, month: number, day: number) {
  return `${year}-${pad(month + 1)}-${pad(day)}`;
}

function parseIso(value: string) {
  if (!/^\d{4}-\d{2}-\d{2}$/.test(value)) return null;
  const [y, m, d] = value.split("-").map(Number);
  const date = new Date(y, m - 1, d);
  if (date.getFullYear() !== y || date.getMonth() !== m - 1 || date.getDate() !== d) return null;
  return date;
}

function formatDisplay(value: string) {
  const date = parseIso(value);
  if (!date) return "";
  return `${date.getDate()} ${MONTHS[date.getMonth()]} ${date.getFullYear()}`;
}

type ClinicDatePickerProps = {
  value?: string;
  defaultValue?: string;
  onChange?: (value: string) => void;
  name?: string;
  placeholder?: string;
  disabled?: boolean;
  className?: string;
  min?: string;
  max?: string;
};

export function ClinicDatePicker({
  value,
  defaultValue = "",
  onChange,
  name,
  placeholder = "اختر التاريخ",
  disabled = false,
  className,
  min,
  max
}: ClinicDatePickerProps) {
  const rootRef = useRef<HTMLDivElement>(null);
  const isControlled = value !== undefined;
  const [internal, setInternal] = useState(defaultValue);
  const selected = isControlled ? value : internal;
  const [open, setOpen] = useState(false);

  const initialCursor = parseIso(selected) ?? new Date();
  const [cursor, setCursor] = useState(() => new Date(initialCursor.getFullYear(), initialCursor.getMonth(), 1));

  useEffect(() => {
    const parsed = parseIso(selected);
    if (parsed) {
      setCursor(new Date(parsed.getFullYear(), parsed.getMonth(), 1));
    }
  }, [selected]);

  useEffect(() => {
    if (!open) return;
    function onDoc(event: MouseEvent) {
      if (!rootRef.current?.contains(event.target as Node)) setOpen(false);
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

  const days = useMemo(() => {
    const year = cursor.getFullYear();
    const month = cursor.getMonth();
    const firstDow = new Date(year, month, 1).getDay();
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    const cells: Array<{ day: number; iso: string; inMonth: boolean } | null> = [];
    for (let i = 0; i < firstDow; i += 1) cells.push(null);
    for (let day = 1; day <= daysInMonth; day += 1) {
      cells.push({ day, iso: toIso(year, month, day), inMonth: true });
    }
    while (cells.length % 7 !== 0) cells.push(null);
    return cells;
  }, [cursor]);

  function choose(iso: string) {
    if (min && iso < min) return;
    if (max && iso > max) return;
    if (!isControlled) setInternal(iso);
    onChange?.(iso);
    setOpen(false);
  }

  function shiftMonth(delta: number) {
    setCursor((current) => new Date(current.getFullYear(), current.getMonth() + delta, 1));
  }

  const todayIso = toIso(new Date().getFullYear(), new Date().getMonth(), new Date().getDate());

  return (
    <div ref={rootRef} className={cn("relative", className)}>
      {name ? <input type="hidden" name={name} value={selected} /> : null}
      <button
        type="button"
        disabled={disabled}
        aria-haspopup="dialog"
        aria-expanded={open}
        className={cn(
          clinicInputClassName,
          "flex items-center justify-between gap-3 text-start",
          open && "border-accent ring-2 ring-accent/20",
          disabled && "cursor-not-allowed opacity-60"
        )}
        onClick={() => setOpen((current) => !current)}
      >
        <span className={cn("truncate", !selected && "text-muted/70")}>
          {selected ? formatDisplay(selected) : placeholder}
        </span>
        <svg viewBox="0 0 24 24" className="h-4 w-4 shrink-0 text-accent" aria-hidden="true">
          <path
            fill="currentColor"
            d="M7 2a1 1 0 0 1 1 1v1h8V3a1 1 0 1 1 2 0v1h1a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h1V3a1 1 0 0 1 1-1Zm12 8H5v10h14V10Zm-2-4H7v1a1 1 0 1 1-2 0V6H5v2h14V6h-1v1a1 1 0 1 1-2 0V6Z"
          />
        </svg>
      </button>

      {open ? (
        <div
          role="dialog"
          aria-label="اختيار التاريخ"
          className="absolute inset-x-0 z-50 mt-2 w-full min-w-[18.5rem] overflow-hidden rounded-2xl border border-border bg-surface shadow-[0_22px_60px_rgba(15,23,42,0.16)] sm:inset-inline-start-0 sm:w-[20.5rem]"
        >
          <div className="flex items-center justify-between gap-2 bg-surface-dark px-4 py-3 text-primary-foreground">
            <button
              type="button"
              className="inline-flex h-8 w-8 items-center justify-center rounded-lg bg-white/10 transition hover:bg-white/20"
              aria-label="الشهر السابق"
              onClick={() => shiftMonth(-1)}
            >
              ‹
            </button>
            <p className="text-sm font-extrabold tracking-tight">
              {MONTHS[cursor.getMonth()]} {cursor.getFullYear()}
            </p>
            <button
              type="button"
              className="inline-flex h-8 w-8 items-center justify-center rounded-lg bg-white/10 transition hover:bg-white/20"
              aria-label="الشهر التالي"
              onClick={() => shiftMonth(1)}
            >
              ›
            </button>
          </div>

          <div className="grid grid-cols-7 gap-1 px-3 pt-3 text-center text-[11px] font-bold text-muted">
            {WEEKDAYS.map((day) => (
              <span key={day} className="py-1">
                {day}
              </span>
            ))}
          </div>

          <div className="grid grid-cols-7 gap-1 px-3 pb-3 pt-1">
            {days.map((cell, index) => {
              if (!cell) {
                return <span key={`empty-${index}`} className="h-9" />;
              }
              const isSelected = cell.iso === selected;
              const isToday = cell.iso === todayIso;
              const outOfRange = Boolean((min && cell.iso < min) || (max && cell.iso > max));
              return (
                <button
                  key={cell.iso}
                  type="button"
                  disabled={outOfRange}
                  className={cn(
                    "inline-flex h-9 items-center justify-center rounded-xl text-sm font-semibold transition",
                    isSelected && "bg-accent text-primary-foreground shadow-sm",
                    !isSelected && isToday && "border border-accent/50 text-accent",
                    !isSelected && !isToday && "text-foreground hover:bg-surface-warm",
                    outOfRange && "cursor-not-allowed opacity-30"
                  )}
                  onClick={() => choose(cell.iso)}
                >
                  {cell.day}
                </button>
              );
            })}
          </div>

          <div className="flex items-center justify-between border-t border-border px-3 py-2.5">
            <button
              type="button"
              className="rounded-lg px-2.5 py-1.5 text-xs font-semibold text-muted transition hover:bg-surface-warm hover:text-foreground"
              onClick={() => {
                if (!isControlled) setInternal("");
                onChange?.("");
                setOpen(false);
              }}
            >
              مسح
            </button>
            <button
              type="button"
              className="rounded-lg px-2.5 py-1.5 text-xs font-bold text-accent transition hover:bg-accent/10"
              onClick={() => choose(todayIso)}
            >
              اليوم
            </button>
          </div>
        </div>
      ) : null}
    </div>
  );
}
