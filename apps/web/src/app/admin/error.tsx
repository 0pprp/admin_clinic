"use client";

import { useEffect } from "react";

export default function AdminError({
  error,
  reset
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <section className="border border-border bg-surface px-5 py-10">
      <h1 className="text-2xl font-semibold">تعذر عرض الصفحة</h1>
      <p className="mt-3 text-sm leading-8 text-muted">حدث خطأ أثناء تحميل لوحة الإدارة. يمكنك المحاولة مرة أخرى.</p>
      <button type="button" className="mt-8 border border-foreground px-4 py-2 text-sm" onClick={reset}>
        إعادة المحاولة
      </button>
    </section>
  );
}
