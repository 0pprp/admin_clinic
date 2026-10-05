"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useId, useState } from "react";
import { brand } from "@/lib/content/brand";
import { isStaff } from "@/lib/admin/roles";
import { getCurrentUser, logout } from "@/lib/auth/session";
import { shortDisplayName } from "@/lib/learning";
import { safeInternalPath } from "@/lib/safe-path";
import type { UserSummary } from "@/lib/api/types";

const navItems = [
  { href: "/dashboard", label: "نظرة عامة", exact: true },
  { href: "/dashboard/courses", label: "دوراتي", exact: false },
  { href: "/dashboard/orders", label: "طلبات الاشتراك", exact: false },
  { href: "/dashboard/activate", label: "تفعيل دورة", exact: false },
  { href: "/dashboard/profile", label: "الملف الشخصي", exact: false }
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
    return (
      <div className="min-h-full bg-background px-4 py-16 text-foreground">
        <p className="text-sm text-muted">جاري تحميل لوحة الطالب...</p>
      </div>
    );
  }

  return (
    <div className="min-h-full bg-background text-foreground">
      <header className="border-b border-border bg-surface-dark text-primary-foreground">
        <div className="mx-auto flex max-w-6xl items-center justify-between gap-3 px-4 py-4">
          <div>
            <p className="text-base font-semibold">{brand.nameAr}</p>
            <p className="mt-1 text-xs text-accent-soft">لوحة التعلّم</p>
          </div>
          <div className="flex items-center gap-3 text-sm">
            <p className="hidden sm:block text-primary-foreground/80">{shortDisplayName(user.fullName)}</p>
            {isStaff(user.roles) ? (
              <Link href="/admin" className="border border-accent px-3 py-1.5 text-accent hover:bg-accent hover:text-surface-dark">
                لوحة الإدارة
              </Link>
            ) : null}
            <Link href="/" className="border border-accent px-3 py-1.5 text-accent hover:bg-accent hover:text-surface-dark">
              العودة للموقع
            </Link>
            <button
              type="button"
              className="border border-white/20 px-3 py-1.5 lg:hidden"
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
                    className={`px-3 py-2 ${current ? "bg-surface-warm text-foreground" : "text-muted hover:text-foreground"}`}
                    onClick={() => setOpen(false)}
                  >
                  {item.label}
                </Link>
              );
            })}
            {isStaff(user.roles) ? (
              <Link href="/admin" className="mt-4 px-3 py-2 text-muted hover:text-foreground" onClick={() => setOpen(false)}>
                لوحة الإدارة
              </Link>
            ) : null}
            <button type="button" className="mt-4 px-3 py-2 text-start text-muted hover:text-foreground" onClick={onLogout}>
              تسجيل الخروج
            </button>
          </nav>
        </aside>
        <main id="main" className="px-4 py-8 sm:py-10 lg:px-8">
          {children}
        </main>
      </div>
    </div>
  );
}
