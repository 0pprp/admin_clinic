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

  return (
    <nav className="mt-12 flex items-center justify-between border-t border-border pt-6 text-sm" aria-label="ترقيم الصفحات">
      {previous ? (
        <Link href={previous} className="text-foreground hover:text-accent">
          الصفحة السابقة
        </Link>
      ) : (
        <span className="text-muted">الصفحة السابقة</span>
      )}
      <p className="text-muted">
        صفحة {page} من {totalPages}
      </p>
      {next ? (
        <Link href={next} className="text-foreground hover:text-accent">
          الصفحة التالية
        </Link>
      ) : (
        <span className="text-muted">الصفحة التالية</span>
      )}
    </nav>
  );
}
