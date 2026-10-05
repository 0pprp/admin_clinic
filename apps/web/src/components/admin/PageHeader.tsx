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
        <h1 className="text-2xl font-semibold sm:text-3xl">{title}</h1>
        {description ? <p className="mt-2 max-w-2xl text-sm leading-7 text-muted">{description}</p> : null}
      </div>
      {actions ? <div className="flex flex-wrap gap-2">{actions}</div> : null}
    </div>
  );
}

export function LoadingState({ label = "جاري التحميل..." }: { label?: string }) {
  return <p className="text-sm text-muted">{label}</p>;
}

export function ErrorState({ message }: { message: string }) {
  return <p className="text-sm text-red-700">{message}</p>;
}

export function AdminTable({ children }: { children: React.ReactNode }) {
  return <div className="overflow-x-auto border border-border bg-surface">{children}</div>;
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
