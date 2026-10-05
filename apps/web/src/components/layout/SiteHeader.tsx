import Link from "next/link";
import { cookies } from "next/headers";
import { navItems } from "@/lib/content/brand";
import { BrandWordmark } from "@/components/ui/clinic";
import { MobileNavigation } from "./MobileNavigation";

function SessionAction({ hasSession }: { hasSession: boolean }) {
  return (
    <Link
      href={hasSession ? "/dashboard" : "/login"}
      className="text-sm font-medium text-foreground/80 transition hover:text-accent"
    >
      {hasSession ? "لوحة التحكم" : "تسجيل الدخول"}
    </Link>
  );
}

export async function SiteHeader() {
  const jar = await cookies();
  const hasSession = jar.has("mr_access") || jar.has("mr_refresh");

  return (
    <header className="sticky top-0 z-40 border-b border-border/80 bg-surface/95 backdrop-blur-md">
      <div className="clinic-shell flex h-[4.25rem] items-center justify-between gap-4 lg:h-[4.75rem]">
        <div className="min-w-0 shrink-0">
          <BrandWordmark />
        </div>
        <nav
          className="hidden items-center gap-1 text-[13px] text-foreground/75 xl:flex xl:gap-0.5 xl:text-sm"
          aria-label="التنقل الرئيسي"
        >
          {navItems.map((item) => (
            <Link
              key={item.href}
              href={item.href}
              className="whitespace-nowrap rounded-xl px-3 py-2 transition hover:bg-surface-warm hover:text-foreground"
            >
              {item.label}
            </Link>
          ))}
        </nav>
        <div className="hidden shrink-0 items-center gap-4 lg:flex">
          <SessionAction hasSession={hasSession} />
        </div>
        <MobileNavigation hasSession={hasSession} />
      </div>
    </header>
  );
}
