"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { cn } from "./cn";
import { clinicInputClassName } from "./Field";

type Period = "am" | "pm";

function pad(n: number) {
  return String(n).padStart(2, "0");
}

/** Parse "HH:mm" or "HH:mm:ss" (24h) into 12h parts */
function parseTime24(value: string): { hour12: number; minute: number; period: Period } | null {
  const match = value.trim().match(/^(\d{1,2}):(\d{2})(?::\d{2})?$/);
  if (!match) return null;
  const hour24 = Number(match[1]);
  const minute = Number(match[2]);
  if (hour24 < 0 || hour24 > 23 || minute < 0 || minute > 59) return null;
  const period: Period = hour24 >= 12 ? "pm" : "am";
  let hour12 = hour24 % 12;
  if (hour12 === 0) hour12 = 12;
  return { hour12, minute, period };
}

function toTime24(hour12: number, minute: number, period: Period) {
  let hour24 = hour12 % 12;
  if (period === "pm") hour24 += 12;
  return `${pad(hour24)}:${pad(minute)}`;
}

function formatDisplay(value: string) {
  const parts = parseTime24(value);
  if (!parts) return "";
  return `${parts.hour12}:${pad(parts.minute)} ${parts.period === "am" ? "صباحاً" : "مساءً"}`;
}

const HOURS = Array.from({ length: 12 }, (_, i) => i + 1);
const MINUTES = Array.from({ length: 60 }, (_, i) => i);

type ClinicTimePickerProps = {
  value?: string;
  defaultValue?: string;
  onChange?: (value: string) => void;
  name?: string;
  placeholder?: string;
  disabled?: boolean;
  className?: string;
};

export function ClinicTimePicker({
  value,
  defaultValue = "",
  onChange,
  name,
  placeholder = "اختر الوقت",
  disabled = false,
  className
}: ClinicTimePickerProps) {
  const rootRef = useRef<HTMLDivElement>(null);
  const isControlled = value !== undefined;
  const [internal, setInternal] = useState(defaultValue);
  const selected = isControlled ? value : internal;
  const [open, setOpen] = useState(false);

  const parsed = useMemo(() => parseTime24(selected) ?? { hour12: 9, minute: 0, period: "am" as Period }, [selected]);
  const [hour12, setHour12] = useState(parsed.hour12);
  const [minute, setMinute] = useState(parsed.minute);
  const [period, setPeriod] = useState<Period>(parsed.period);

  useEffect(() => {
    const next = parseTime24(selected);
    if (next) {
      setHour12(next.hour12);
      setMinute(next.minute);
      setPeriod(next.period);
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

  function commit(nextHour: number, nextMinute: number, nextPeriod: Period) {
    const next = toTime24(nextHour, nextMinute, nextPeriod);
    if (!isControlled) setInternal(next);
    onChange?.(next);
  }

  function applyAndClose() {
    commit(hour12, minute, period);
    setOpen(false);
  }

  function clear() {
    if (!isControlled) setInternal("");
    onChange?.("");
    setOpen(false);
  }

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
            d="M12 2a10 10 0 1 1 0 20 10 10 0 0 1 0-20Zm0 2a8 8 0 1 0 0 16 8 8 0 0 0 0-16Zm.75 2.5a.75.75 0 0 0-1.5 0V12c0 .2.08.39.22.53l3 3a.75.75 0 1 0 1.06-1.06l-2.78-2.78V6.5Z"
          />
        </svg>
      </button>

      {open ? (
        <div
          role="dialog"
          aria-label="اختيار الوقت"
          className="absolute inset-x-0 z-50 mt-2 w-full min-w-[18rem] overflow-hidden rounded-2xl border border-border bg-surface shadow-[0_22px_60px_rgba(15,23,42,0.16)] sm:w-[21rem]"
        >
          <div className="bg-surface-dark px-4 py-3 text-primary-foreground">
            <p className="text-xs font-medium text-primary-foreground/65">الوقت المفضل</p>
            <p className="mt-1 text-lg font-extrabold tracking-tight">
              {hour12}:{pad(minute)} {period === "am" ? "صباحاً" : "مساءً"}
            </p>
          </div>

          <div className="grid grid-cols-3 gap-2 p-3">
            <WheelColumn
              label="الساعة"
              values={HOURS}
              selected={hour12}
              onSelect={(next) => {
                setHour12(next);
                commit(next, minute, period);
              }}
            />
            <WheelColumn
              label="الدقيقة"
              values={MINUTES}
              selected={minute}
              format={(n) => pad(n)}
              onSelect={(next) => {
                setMinute(next);
                commit(hour12, next, period);
              }}
            />
            <div className="flex flex-col">
              <p className="mb-2 text-center text-[11px] font-bold text-muted">الفترة</p>
              <div className="flex flex-1 flex-col gap-2">
                {(
                  [
                    { value: "am" as const, label: "صباحاً" },
                    { value: "pm" as const, label: "مساءً" }
                  ] as const
                ).map((item) => (
                  <button
                    key={item.value}
                    type="button"
                    className={cn(
                      "flex flex-1 items-center justify-center rounded-xl text-sm font-bold transition",
                      period === item.value
                        ? "bg-accent text-primary-foreground shadow-sm"
                        : "bg-surface-warm text-foreground hover:bg-border/60"
                    )}
                    onClick={() => {
                      setPeriod(item.value);
                      commit(hour12, minute, item.value);
                    }}
                  >
                    {item.label}
                  </button>
                ))}
              </div>
            </div>
          </div>

          <div className="flex items-center justify-between border-t border-border px-3 py-2.5">
            <button
              type="button"
              className="rounded-lg px-2.5 py-1.5 text-xs font-semibold text-muted transition hover:bg-surface-warm hover:text-foreground"
              onClick={clear}
            >
              مسح
            </button>
            <button
              type="button"
              className="rounded-xl bg-accent px-3.5 py-1.5 text-xs font-bold text-primary-foreground transition hover:bg-accent-soft"
              onClick={applyAndClose}
            >
              تأكيد
            </button>
          </div>
        </div>
      ) : null}
    </div>
  );
}

function WheelColumn({
  label,
  values,
  selected,
  onSelect,
  format = String
}: {
  label: string;
  values: number[];
  selected: number;
  onSelect: (value: number) => void;
  format?: (value: number) => string;
}) {
  const listRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const node = listRef.current?.querySelector<HTMLButtonElement>(`[data-value="${selected}"]`);
    node?.scrollIntoView({ block: "center" });
  }, [selected, values]);

  return (
    <div className="flex min-h-0 flex-col">
      <p className="mb-2 text-center text-[11px] font-bold text-muted">{label}</p>
      <div
        ref={listRef}
        className="h-40 overflow-y-auto rounded-xl border border-border bg-surface-warm/50 p-1 [scrollbar-width:thin]"
      >
        {values.map((value) => {
          const active = value === selected;
          return (
            <button
              key={value}
              type="button"
              data-value={value}
              className={cn(
                "flex w-full items-center justify-center rounded-lg py-1.5 text-sm font-semibold transition",
                active ? "bg-surface-dark text-primary-foreground" : "text-foreground hover:bg-surface"
              )}
              onClick={() => onSelect(value)}
            >
              {format(value)}
            </button>
          );
        })}
      </div>
    </div>
  );
}
