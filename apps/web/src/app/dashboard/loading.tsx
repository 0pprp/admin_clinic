export default function DashboardLoading() {
  return (
    <div className="animate-pulse space-y-4">
      <div className="h-8 w-48 bg-surface-warm" />
      <div className="h-4 w-72 bg-surface-warm" />
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <div className="h-28 bg-surface-warm" />
        <div className="h-28 bg-surface-warm" />
        <div className="h-28 bg-surface-warm" />
        <div className="h-28 bg-surface-warm" />
      </div>
    </div>
  );
}
