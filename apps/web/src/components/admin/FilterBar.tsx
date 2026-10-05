"use client";

import { useRouter } from "next/navigation";
import { FormEvent } from "react";
import { withListQuery } from "@/lib/admin/query";
import { inputClassName, secondaryButtonClassName } from "@/lib/admin/ui";

export function FilterBar({
  pathname,
  values,
  children
}: {
  pathname: string;
  values: Record<string, string>;
  children: React.ReactNode;
}) {
  const router = useRouter();

  function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    const next: Record<string, string> = { page: "1", pageSize: values.pageSize || "12" };
    for (const [key, value] of form.entries()) {
      next[key] = String(value);
    }
    router.push(withListQuery(pathname, next));
  }

  return (
    <form onSubmit={onSubmit} className="mb-6 grid gap-3 border border-border bg-surface p-4 sm:grid-cols-2 lg:grid-cols-4">
      {children}
      <div className="flex items-end gap-2 sm:col-span-2 lg:col-span-4">
        <button type="submit" className={secondaryButtonClassName}>
          تطبيق التصفية
        </button>
      </div>
    </form>
  );
}

export function FilterField({
  label,
  name,
  defaultValue = "",
  type = "text",
  placeholder
}: {
  label: string;
  name: string;
  defaultValue?: string;
  type?: string;
  placeholder?: string;
}) {
  return (
    <label className="block text-sm">
      <span className="mb-1.5 block text-muted">{label}</span>
      <input className={inputClassName} name={name} type={type} defaultValue={defaultValue} placeholder={placeholder} />
    </label>
  );
}

export function FilterSelect({
  label,
  name,
  defaultValue = "",
  options
}: {
  label: string;
  name: string;
  defaultValue?: string;
  options: Array<{ value: string; label: string }>;
}) {
  return (
    <label className="block text-sm">
      <span className="mb-1.5 block text-muted">{label}</span>
      <select className={inputClassName} name={name} defaultValue={defaultValue}>
        {options.map((option) => (
          <option key={option.value || "all"} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    </label>
  );
}
