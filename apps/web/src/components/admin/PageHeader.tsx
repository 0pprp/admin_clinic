import { BrandLoading } from "@/components/ui/clinic";

export function PageHeader({
  title,
  description,
  actions,
  eyebrow = "لوحة الإدارة"
}: {
  title: string;
  description?: string;
  actions?: React.ReactNode;
  eyebrow?: string;
}) {
  return (
    <div className="mb-7 flex flex-col gap-4 sm:mb-8 sm:flex-row sm:items-start sm:justify-between">
      <div className="min-w-0">
        {eyebrow ? <p className="text-sm font-bold text-accent">{eyebrow}</p> : null}
        <h1 className={`text-2xl font-extrabold tracking-tight sm:text-3xl ${eyebrow ? "mt-2" : ""}`}>{title}</h1>
        {description ? <p className="mt-2 max-w-2xl text-sm leading-7 text-muted">{description}</p> : null}
      </div>
      {actions ? <div className="flex flex-wrap gap-2">{actions}</div> : null}
    </div>
  );
}

export function LoadingState({ label = "جاري التحميل..." }: { label?: string }) {
  return <BrandLoading label={label} />;
}

export function ErrorState({ message }: { message: string }) {
  return (
    <p className="clinic-card border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-800" role="alert">
      {message}
    </p>
  );
}

export function AdminTable({ children }: { children: React.ReactNode }) {
  return <div className="clinic-card overflow-x-auto">{children}</div>;
}

export function Field({
  label,
  children
}: {
  label: string;
  children: React.ReactNode;
}) {
  return (
    <label className="block text-sm">
      <span className="mb-1.5 block font-medium text-foreground">{label}</span>
      {children}
    </label>
  );
}
