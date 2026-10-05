import Link from "next/link";
import { SiteFooter } from "@/components/layout/SiteFooter";
import { SiteHeader } from "@/components/layout/SiteHeader";
import { Container } from "@/components/shared/Container";

export default function NotFound() {
  return (
    <>
      <SiteHeader />
      <main id="main" className="flex-1">
        <Container className="py-24 text-center">
          <p className="text-xs tracking-[0.22em] text-accent">404</p>
          <h1 className="mt-4 text-4xl font-semibold">الصفحة غير موجودة</h1>
          <p className="mx-auto mt-4 max-w-md text-muted">الرابط الذي طلبته غير متاح، أو نُقل إلى مكان آخر.</p>
          <Link
            href="/"
            className="mt-8 inline-flex border border-foreground bg-foreground px-5 py-3 text-sm text-primary-foreground"
          >
            العودة إلى الرئيسية
          </Link>
        </Container>
      </main>
      <SiteFooter settings={null} />
    </>
  );
}
