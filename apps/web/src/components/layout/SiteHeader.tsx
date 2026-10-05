import Link from "next/link";
import { cookies } from "next/headers";
import { navItems } from "@/lib/content/brand";
import { Container } from "@/components/shared/Container";
import { MobileNavigation } from "./MobileNavigation";
import { Wordmark } from "./Wordmark";

function SessionAction({ hasSession }: { hasSession: boolean }) {
  if (hasSession) {
    return (
      <Link href="/dashboard" className="text-sm text-primary-foreground/90 transition hover:text-accent-soft">
        لوحة التحكم
      </Link>
    );
  }

  return (
    <Link href="/login" className="text-sm text-primary-foreground/90 transition hover:text-accent-soft">
      تسجيل الدخول
    </Link>
  );
}

export async function SiteHeader() {
  const jar = await cookies();
  const hasSession = jar.has("mr_access") || jar.has("mr_refresh");

  return (
    <header className="sticky top-0 z-40 border-b border-white/10 bg-surface-dark/95 backdrop-blur-md">
      <Container className="flex h-16 items-center justify-between gap-3 sm:gap-4 lg:h-[4.5rem]">
        <div className="min-w-0 flex-1">
          <Wordmark inverted />
        </div>
        <nav
          className="hidden items-center gap-3 text-[13px] text-primary-foreground/80 lg:flex xl:gap-5 xl:text-sm"
          aria-label="التنقل الرئيسي"
        >
          {navItems.map((item) => (
            <Link key={item.href} href={item.href} className="whitespace-nowrap transition hover:text-accent-soft">
              {item.label}
            </Link>
          ))}
        </nav>
        <div className="hidden shrink-0 items-center gap-4 lg:flex">
          <SessionAction hasSession={hasSession} />
          <Link
            href="/courses"
            className="border border-accent px-4 py-2 text-sm text-accent transition hover:bg-accent hover:text-primary-foreground"
          >
            استكشف الكورسات
          </Link>
        </div>
        <MobileNavigation hasSession={hasSession} />
      </Container>
    </header>
  );
}
