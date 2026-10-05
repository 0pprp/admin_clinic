import { Container } from "@/components/shared/Container";

export default function SiteLoading() {
  return (
    <Container className="py-24" aria-busy="true" aria-live="polite">
      <div className="h-3 w-24 bg-border" />
      <div className="mt-6 h-10 w-2/3 max-w-md bg-border" />
      <div className="mt-8 h-24 w-full bg-surface" />
      <p className="sr-only">جاري تحميل الصفحة...</p>
    </Container>
  );
}
