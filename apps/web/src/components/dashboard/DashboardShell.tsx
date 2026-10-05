export function DashboardShell({
  title,
  description,
  children
}: {
  title: string;
  description?: string;
  children: React.ReactNode;
}) {
  return (
    <div>
      <h1 className="text-3xl font-semibold">{title}</h1>
      {description ? <p className="mt-3 max-w-2xl text-sm leading-8 text-muted">{description}</p> : null}
      <div className="mt-8">{children}</div>
    </div>
  );
}
