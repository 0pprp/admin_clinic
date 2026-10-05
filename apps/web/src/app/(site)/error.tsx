"use client";

import { Container } from "@/components/shared/Container";

export default function SiteError({ reset }: { error: Error & { digest?: string }; reset: () => void }) {
  return (
    <Container className="py-24">
      <h1 className="text-3xl font-semibold">تعذر عرض هذه الصفحة</h1>
      <p className="mt-4 max-w-xl text-muted">حدث خطأ غير متوقع. يمكنك المحاولة مرة أخرى أو العودة لاحقاً.</p>
      <button
        type="button"
        className="mt-8 border border-foreground px-5 py-3 text-sm"
        onClick={() => reset()}
      >
        إعادة المحاولة
      </button>
    </Container>
  );
}
