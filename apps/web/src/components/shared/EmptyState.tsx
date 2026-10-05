export function EmptyState({ title, description }: { title: string; description?: string }) {
  return (
    <div className="clinic-card border-dashed px-6 py-12 text-center sm:px-10 sm:py-14">
      <p className="text-lg font-semibold">{title}</p>
      {description ? <p className="mx-auto mt-3 max-w-md text-sm leading-7 text-muted">{description}</p> : null}
    </div>
  );
}
