"use client";

import { useEffect } from "react";
import { Button } from "@/components/ui/clinic";

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
    <section className="clinic-card px-5 py-10 sm:px-8">
      <p className="text-sm font-bold text-accent">خطأ</p>
      <h1 className="mt-2 text-2xl font-extrabold tracking-tight">تعذر عرض الصفحة</h1>
      <p className="mt-3 text-sm leading-8 text-muted">حدث خطأ أثناء تحميل لوحة الإدارة. يمكنك المحاولة مرة أخرى.</p>
      <Button type="button" variant="accent" className="mt-8" onClick={reset}>
        إعادة المحاولة
      </Button>
    </section>
  );
}
