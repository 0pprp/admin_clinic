"use client";

import { useRef } from "react";
import { inputClassName } from "@/components/auth/AuthShell";

type Props = {
  length?: number;
  value: string;
  onChange: (value: string) => void;
  disabled?: boolean;
};

export function OtpInput({ length = 6, value, onChange, disabled }: Props) {
  const inputsRef = useRef<Array<HTMLInputElement | null>>([]);
  const digits = value.padEnd(length, " ").slice(0, length).split("");

  function setDigit(index: number, digit: string) {
    const next = value.split("");
    while (next.length < length) next.push("");
    next[index] = digit;
    onChange(next.join("").replace(/\s/g, "").slice(0, length));
  }

  return (
    <div className="flex justify-center gap-2" dir="ltr">
      {Array.from({ length }, (_, index) => (
        <input
          key={index}
          ref={(node) => {
            inputsRef.current[index] = node;
          }}
          className={`${inputClassName} h-12 w-11 px-0 text-center text-lg font-extrabold tracking-widest`}
          inputMode="numeric"
          autoComplete={index === 0 ? "one-time-code" : "off"}
          maxLength={1}
          disabled={disabled}
          value={digits[index]?.trim() ?? ""}
          onChange={(event) => {
            const digit = event.target.value.replace(/\D/g, "").slice(-1);
            setDigit(index, digit);
            if (digit && index < length - 1) {
              inputsRef.current[index + 1]?.focus();
            }
          }}
          onKeyDown={(event) => {
            if (event.key === "Backspace" && !digits[index]?.trim() && index > 0) {
              inputsRef.current[index - 1]?.focus();
            }
          }}
          onPaste={(event) => {
            event.preventDefault();
            const pasted = event.clipboardData.getData("text").replace(/\D/g, "").slice(0, length);
            if (pasted) {
              onChange(pasted);
              inputsRef.current[Math.min(pasted.length, length) - 1]?.focus();
            }
          }}
        />
      ))}
    </div>
  );
}
