export default function DashboardLoading() {
  return (
    <div className="animate-pulse space-y-4">
      <div className="h-3 w-28 rounded bg-surface-warm" />
      <div className="h-8 w-48 rounded-lg bg-surface-warm" />
      <div className="h-4 w-72 rounded-lg bg-surface-warm" />
      <div className="grid gap-4 pt-4 sm:grid-cols-2 xl:grid-cols-3">
        <div className="h-28 rounded-2xl bg-surface-warm" />
        <div className="h-28 rounded-2xl bg-surface-warm" />
        <div className="h-28 rounded-2xl bg-surface-warm" />
      </div>
    </div>
  );
}
