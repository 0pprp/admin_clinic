"use client";

import Link from "next/link";
import { usePathname, useSearchParams } from "next/navigation";

export function AdminPagination({
  page,
  pageSize,
  totalCount
}: {
  page: number;
  pageSize: number;
  totalCount: number;
}) {
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

  if (totalPages <= 1) {
    return null;
  }

  function hrefFor(nextPage: number): string {
    const params = new URLSearchParams(searchParams.toString());
    params.set("page", String(nextPage));
    params.set("pageSize", String(pageSize));
    return `${pathname}?${params.toString()}`;
  }

  return (
    <nav className="mt-6 flex items-center justify-between border-t border-border pt-4 text-sm" aria-label="ترقيم الصفحات">
      {page > 1 ? (
        <Link href={hrefFor(page - 1)} className="text-foreground hover:text-accent">
          الصفحة السابقة
        </Link>
      ) : (
        <span className="text-muted">الصفحة السابقة</span>
      )}
      <p className="text-muted">
        صفحة {page} من {totalPages} · {totalCount} عنصر
      </p>
      {page < totalPages ? (
        <Link href={hrefFor(page + 1)} className="text-foreground hover:text-accent">
          الصفحة التالية
        </Link>
      ) : (
        <span className="text-muted">الصفحة التالية</span>
      )}
    </nav>
  );
}
