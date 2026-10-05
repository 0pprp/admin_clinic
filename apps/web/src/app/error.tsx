"use client";

import Link from "next/link";
import { Container } from "@/components/shared/Container";

export default function GlobalError({ reset }: { error: Error & { digest?: string }; reset: () => void }) {
  return (
    <Container className="py-24">
      <h1 className="text-3xl font-semibold">تعذر تحميل الموقع</h1>
      <p className="mt-4 text-muted">حدث خطأ غير متوقع. يمكنك المحاولة مرة أخرى.</p>
      <div className="mt-8 flex gap-4">
        <button type="button" className="border border-foreground px-5 py-3 text-sm" onClick={() => reset()}>
          إعادة المحاولة
        </button>
        <Link href="/" className="px-5 py-3 text-sm">
          العودة إلى الرئيسية
        </Link>
      </div>
    </Container>
  );
}
