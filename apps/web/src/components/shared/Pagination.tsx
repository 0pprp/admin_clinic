import Link from "next/link";

export function Pagination({
  page,
  pageSize,
  totalCount,
  basePath
}: {
  page: number;
  pageSize: number;
  totalCount: number;
  basePath: string;
}) {
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  if (totalPages <= 1) {
    return null;
  }

  const previous = page > 1 ? `${basePath}?page=${page - 1}` : null;
  const next = page < totalPages ? `${basePath}?page=${page + 1}` : null;
  const linkClass =
    "inline-flex items-center rounded-xl border border-border bg-surface px-3 py-2 transition hover:border-accent hover:text-accent";
  const disabledClass = "inline-flex items-center rounded-xl border border-transparent px-3 py-2 text-muted";

  return (
    <nav className="clinic-card mt-12 flex items-center justify-between gap-4 px-4 py-4 text-sm sm:px-5" aria-label="ترقيم الصفحات">
      {previous ? (
        <Link href={previous} className={linkClass}>
          الصفحة السابقة
        </Link>
      ) : (
        <span className={disabledClass}>الصفحة السابقة</span>
      )}
      <p className="text-muted">
        صفحة {page} من {totalPages}
      </p>
      {next ? (
        <Link href={next} className={linkClass}>
          الصفحة التالية
        </Link>
      ) : (
        <span className={disabledClass}>الصفحة التالية</span>
      )}
    </nav>
  );
}
