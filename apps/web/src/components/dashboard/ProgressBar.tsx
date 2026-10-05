export function ProgressBar({
  value,
  label = "نسبة التقدم"
}: {
  value: number;
  label?: string;
}) {
  const clamped = Math.min(100, Math.max(0, Math.round(value)));

  return (
    <div>
      <div className="mb-2 flex items-center justify-between gap-3 text-sm">
        <span className="text-muted">{label}</span>
        <span className="font-medium">{clamped}%</span>
      </div>
      <div
        role="progressbar"
        aria-label={label}
        aria-valuenow={clamped}
        aria-valuemin={0}
        aria-valuemax={100}
        className="h-2 overflow-hidden bg-surface-warm"
      >
        <div className="h-full bg-accent transition-[width]" style={{ width: `${clamped}%` }} />
      </div>
    </div>
  );
}
