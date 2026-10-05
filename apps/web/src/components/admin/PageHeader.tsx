import { BrandLoading } from "@/components/ui/clinic";

export function PageHeader({
  title,
  description,
  actions
}: {
  title: string;
  description?: string;
  actions?: React.ReactNode;
}) {
  return (
    <div className="mb-8 flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight sm:text-3xl">{title}</h1>
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
  return <p className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{message}</p>;
}

export function AdminTable({ children }: { children: React.ReactNode }) {
  return <div className="clinic-panel overflow-x-auto">{children}</div>;
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
      <span className="mb-1.5 block font-medium">{label}</span>
      {children}
    </label>
  );
}
