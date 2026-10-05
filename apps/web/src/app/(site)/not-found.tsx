import Link from "next/link";
import { BrandAccentLabel } from "@/components/brand/BrandAccentLabel";
import { Container } from "@/components/shared/Container";

export default function SiteNotFound() {
  return (
    <Container className="py-24 text-center">
      <BrandAccentLabel className="text-base font-bold tracking-wide">404</BrandAccentLabel>
      <h1 className="mt-4 text-4xl font-semibold">الصفحة غير موجودة</h1>
      <p className="mx-auto mt-4 max-w-md text-muted">الرابط الذي طلبته غير متاح، أو نُقل إلى مكان آخر.</p>
      <Link
        href="/"
        className="mt-8 inline-flex border border-foreground bg-foreground px-5 py-3 text-sm text-primary-foreground"
      >
        العودة إلى الرئيسية
      </Link>
    </Container>
  );
}
