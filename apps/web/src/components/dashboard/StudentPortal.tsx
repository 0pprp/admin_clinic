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

  async function onLogout() {
    await logout();
    router.replace("/login");
    router.refresh();
  }

  if (!user) {
    return <BrandLoading label="جاري تحميل مساحة التعلّم..." />;
  }

  return (
    <div className="min-h-full bg-background text-foreground">
      <header className="border-b border-border bg-surface">
        <div className="mx-auto flex max-w-6xl items-center justify-between gap-3 px-4 py-3.5">
          <div className="min-w-0">
            <BrandWordmark compact href="/dashboard" />
            <p className="mt-1.5 text-xs font-semibold text-accent">مساحة المتعلم</p>
          </div>
          <div className="flex items-center gap-2 text-sm sm:gap-3">
            <p className="hidden text-muted sm:block">{shortDisplayName(user.fullName)}</p>
            {isStaff(user.roles) ? (
              <Link href="/admin" className="rounded-xl border border-accent px-3 py-1.5 font-semibold text-accent">
                لوحة الإدارة
              </Link>
            ) : null}
            <Link href="/" className="rounded-xl border border-border px-3 py-1.5">
              الموقع
            </Link>
            <button
              type="button"
              className="rounded-xl border border-border px-3 py-1.5 lg:hidden"
              aria-expanded={open}
              aria-controls={menuId}
              onClick={() => setOpen((value) => !value)}
            >
              القائمة
            </button>
          </div>
        </div>
      </header>
      <div className="mx-auto grid max-w-6xl lg:grid-cols-[15rem_minmax(0,1fr)]">
        <aside
          id={menuId}
          className={`${open ? "block" : "hidden"} border-b border-border bg-surface px-4 py-5 lg:block lg:border-b-0 lg:border-l lg:px-5 lg:py-8`}
        >
          <nav aria-label="تنقل لوحة الطالب" className="flex flex-col gap-1 text-sm">
            {navItems.map((item) => {
              const current = item.exact
                ? pathname === item.href
                : pathname === item.href || pathname.startsWith(`${item.href}/`);
              return (
                <Link
                  key={item.href}
                  href={item.href}
                  aria-current={current ? "page" : undefined}
                  className={`rounded-xl px-3 py-2 transition ${
                    current ? "bg-surface-warm font-semibold text-foreground" : "text-muted hover:bg-surface-warm/70 hover:text-foreground"
                  }`}
                  onClick={() => setOpen(false)}
                >
                  {item.label}
                </Link>
              );
            })}
            {isStaff(user.roles) ? (
              <Link href="/admin" className="mt-4 rounded-xl px-3 py-2 text-muted hover:text-foreground" onClick={() => setOpen(false)}>
                لوحة الإدارة
              </Link>
            ) : null}
            <Button variant="ghost" className="mt-4 justify-start px-3 text-muted" onClick={onLogout}>
              تسجيل الخروج
            </Button>
          </nav>
        </aside>
        <main id="main" className="px-4 py-8 sm:py-10 lg:px-8">
          {children}
        </main>
      </div>
    </div>
  );
}
