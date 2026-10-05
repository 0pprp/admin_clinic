"use client";

import { useRouter } from "next/navigation";
import { FormEvent } from "react";
import { Button } from "@/components/ui/clinic";
import { clinicInputClassName } from "@/components/ui/clinic/Field";
import { withListQuery } from "@/lib/admin/query";

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
    <form onSubmit={onSubmit} className="clinic-card mb-6 grid gap-4 p-5 sm:grid-cols-2 lg:grid-cols-4">
      {children}
      <div className="flex items-end gap-2 sm:col-span-2 lg:col-span-4">
        <Button type="submit" variant="accent" size="md">
          تطبيق التصفية
        </Button>
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
      <span className="mb-1.5 block font-medium text-foreground">{label}</span>
      <input className={clinicInputClassName} name={name} type={type} defaultValue={defaultValue} placeholder={placeholder} />
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
      <span className="mb-1.5 block font-medium text-foreground">{label}</span>
      <select className={clinicInputClassName} name={name} defaultValue={defaultValue}>
        {options.map((option) => (
          <option key={option.value || "all"} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    </label>
  );
}
