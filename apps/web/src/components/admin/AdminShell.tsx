"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useId, useState } from "react";
import { ToastProvider } from "@/components/admin/ToastProvider";
import { getCurrentUser, logout } from "@/lib/auth/session";
import { adminNavGroups, isNavCurrent, pageTitleForPath } from "@/lib/admin/nav";
import { isStaff } from "@/lib/admin/roles";
import { brand } from "@/lib/content/brand";
import { safeInternalPath } from "@/lib/safe-path";
import type { UserSummary } from "@/lib/api/types";

export function AdminShell({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const router = useRouter();
  const menuId = useId();
  const [user, setUser] = useState<UserSummary | null | undefined>(undefined);
  const [open, setOpen] = useState(false);

  useEffect(() => {
    getCurrentUser().then((session) => {
      if (!session) {
        router.replace(`/login?from=${encodeURIComponent(safeInternalPath(pathname, "/admin"))}`);
        return;
      }

      if (!isStaff(session.roles)) {
        router.replace("/dashboard");
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
        <p className="text-sm text-muted">جاري تحميل لوحة الإدارة...</p>
      </div>
    );
  }

  const groups = adminNavGroups
    .map((group) => ({ ...group, items: group.items.filter((item) => item.visible(user.roles)) }))
    .filter((group) => group.items.length > 0);
  const title = pageTitleForPath(pathname, user.roles);

  return (
    <ToastProvider>
      <div className="min-h-full bg-background text-foreground">
        <header className="border-b border-border bg-surface-dark text-primary-foreground">
          <div className="flex items-center justify-between gap-3 px-4 py-4 lg:px-6">
            <div>
              <p className="text-base font-semibold">{brand.nameAr}</p>
              <p className="mt-1 text-xs text-accent-soft">لوحة الإدارة · {title}</p>
            </div>
            <div className="flex items-center gap-3 text-sm">
              <p className="hidden sm:block text-primary-foreground/80">{user.fullName}</p>
              <Link href="/dashboard" className="hidden border border-white/20 px-3 py-1.5 sm:inline-flex">
                لوحة الطالب
              </Link>
              <Link href="/" className="border border-accent px-3 py-1.5 text-accent hover:bg-accent hover:text-surface-dark">
                الموقع
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
        <div className="lg:grid lg:grid-cols-[16rem_minmax(0,1fr)]">
          <aside
            id={menuId}
            className={`${open ? "block" : "hidden"} border-b border-border bg-surface px-3 py-4 lg:block lg:min-h-[calc(100vh-4.5rem)] lg:border-b-0 lg:border-l lg:px-4 lg:py-6`}
          >
            <nav aria-label="تنقل لوحة الإدارة" className="flex flex-col gap-5 text-sm">
              {groups.map((group) => (
                <div key={group.title}>
                  <p className="px-3 text-[11px] font-semibold tracking-[0.14em] text-accent">{group.title}</p>
                  <div className="mt-2 flex flex-col gap-1">
                    {group.items.map((item) => {
                      const current = isNavCurrent(pathname, item);
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
                  </div>
                </div>
              ))}
              <button type="button" className="px-3 py-2 text-start text-muted hover:text-foreground" onClick={onLogout}>
                تسجيل الخروج
              </button>
            </nav>
          </aside>
          <main id="main" className="px-4 py-8 sm:py-10 lg:px-8">
            {children}
          </main>
        </div>
      </div>
    </ToastProvider>
  );
}
