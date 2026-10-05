"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useId, useState } from "react";
import { BrandLoading, BrandWordmark, Button } from "@/components/ui/clinic";
import { isStaff } from "@/lib/admin/roles";
import { getCurrentUser, logout } from "@/lib/auth/session";
import { shortDisplayName } from "@/lib/learning";
import { safeInternalPath } from "@/lib/safe-path";
import type { UserSummary } from "@/lib/api/types";

const navItems = [
  { href: "/dashboard", label: "نظرة عامة", exact: true },
  { href: "/dashboard/courses", label: "كورساتي", exact: false },
  { href: "/dashboard/orders", label: "طلباتي", exact: false },
  { href: "/dashboard/activate", label: "تفعيل كود", exact: false },
  { href: "/dashboard/profile", label: "ملفي الشخصي", exact: false }
] as const;

/** S01 · مساحة المتعلم — شريط علوي + شريط جانبي بلمسات برتقالية */
export function StudentPortal({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const router = useRouter();
  const menuId = useId();
  const [user, setUser] = useState<UserSummary | null | undefined>(undefined);
  const [open, setOpen] = useState(false);

  useEffect(() => {
    getCurrentUser().then((session) => {
      if (!session) {
        router.replace(`/login?from=${encodeURIComponent(safeInternalPath(pathname))}`);
        return;
      }

      setUser(session);
    });
  }, [pathname, router]);

  useEffect(() => {
    setOpen(false);
  }, [pathname]);

  useEffect(() => {
    if (!open) {
      return;
    }
    const previousOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    return () => {
      document.body.style.overflow = previousOverflow;
    };
  }, [open]);

  async function onLogout() {
    await logout();
    router.replace("/login");
    router.refresh();
  }

  if (!user) {
    return <BrandLoading label="جاري تحميل مساحة التعلّم..." />;
  }

  const nav = (
    <nav aria-label="تنقل لوحة الطالب" className="flex flex-col gap-1 text-sm">
      <p className="mb-2 px-3 text-[12px] font-bold text-accent">القائمة</p>
      {navItems.map((item) => {
        const current = item.exact
          ? pathname === item.href
          : pathname === item.href || pathname.startsWith(`${item.href}/`);
        return (
          <Link
            key={item.href}
            href={item.href}
            aria-current={current ? "page" : undefined}
            className={`rounded-xl px-3 py-2.5 transition ${
              current
                ? "bg-accent/10 font-semibold text-accent"
                : "text-muted hover:bg-surface-warm hover:text-foreground"
            }`}
            onClick={() => setOpen(false)}
          >
            {item.label}
          </Link>
        );
      })}
      {isStaff(user.roles) ? (
        <Link
          href="/admin"
          className="mt-4 rounded-xl px-3 py-2.5 text-muted transition hover:bg-surface-warm hover:text-foreground"
          onClick={() => setOpen(false)}
        >
          لوحة الإدارة
        </Link>
      ) : null}
      <Button variant="ghost" className="mt-4 justify-start px-3 text-muted" onClick={onLogout}>
        تسجيل الخروج
      </Button>
    </nav>
  );

  return (
    <div className="min-h-full bg-background text-foreground">
      <header className="border-b border-border bg-surface">
        <div className="mx-auto flex max-w-6xl items-center justify-between gap-2 px-3 py-3 sm:gap-3 sm:px-6 sm:py-3.5">
          <div className="min-w-0">
            <BrandWordmark compact href="/dashboard" />
            <p className="mt-1 text-[11px] font-bold text-accent sm:mt-1.5 sm:text-xs">مساحة المتعلم</p>
          </div>
          <div className="flex items-center gap-1.5 text-sm sm:gap-3">
            <p className="hidden max-w-[8rem] truncate text-muted md:block lg:max-w-none">
              {shortDisplayName(user.fullName)}
            </p>
            {isStaff(user.roles) ? (
              <Link
                href="/admin"
                className="hidden rounded-xl border border-accent px-2.5 py-1.5 text-xs font-semibold text-accent transition hover:bg-accent/5 sm:inline-flex sm:px-3 sm:text-sm"
              >
                لوحة الإدارة
              </Link>
            ) : null}
            <Link
              href="/"
              className="hidden rounded-xl border border-border px-2.5 py-1.5 text-xs font-medium transition hover:border-accent hover:text-accent sm:inline-flex sm:px-3 sm:text-sm"
            >
              الموقع
            </Link>
            <button
              type="button"
              className="inline-flex h-10 w-10 items-center justify-center rounded-xl border border-border bg-surface lg:hidden"
              aria-expanded={open}
              aria-controls={menuId}
              aria-label={open ? "إغلاق القائمة" : "فتح القائمة"}
              onClick={() => setOpen((value) => !value)}
            >
              <span aria-hidden="true" className="relative block h-4 w-5">
                <span
                  className={`absolute start-0 top-0 block h-0.5 w-5 rounded-full bg-current transition ${open ? "top-1.5 rotate-45" : ""}`}
                />
                <span
                  className={`absolute start-0 top-1.5 block h-0.5 w-5 rounded-full bg-current transition ${open ? "opacity-0" : ""}`}
                />
                <span
                  className={`absolute start-0 top-3 block h-0.5 w-5 rounded-full bg-current transition ${open ? "top-1.5 -rotate-45" : ""}`}
                />
              </span>
            </button>
          </div>
        </div>
      </header>

      <div className="mx-auto grid max-w-6xl lg:grid-cols-[15.5rem_minmax(0,1fr)]">
        {/* Desktop sidebar */}
        <aside className="hidden border-border bg-surface lg:block lg:min-h-[calc(100vh-4.75rem)] lg:border-l lg:px-5 lg:py-8">
          {nav}
        </aside>

        {/* Mobile / tablet drawer */}
        {open ? (
          <div className="fixed inset-0 z-40 lg:hidden">
            <button
              type="button"
              className="absolute inset-0 bg-surface-dark/45 backdrop-blur-[2px]"
              aria-label="إغلاق القائمة"
              onClick={() => setOpen(false)}
            />
            <aside
              id={menuId}
              className="absolute inset-y-0 start-0 flex w-[min(100%,17rem)] flex-col border-e border-border bg-surface px-4 py-5 shadow-[0_0_40px_rgba(15,23,42,0.14)]"
            >
              {nav}
            </aside>
          </div>
        ) : (
          <aside id={menuId} className="hidden" hidden />
        )}

        <main id="main" className="px-3 py-6 sm:px-6 sm:py-8 lg:px-8 lg:py-10">
          {children}
        </main>
      </div>
    </div>
  );
}
