"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useId, useState } from "react";
import { ToastProvider } from "@/components/admin/ToastProvider";
import { BrandLoading, BrandMark, Button } from "@/components/ui/clinic";
import { brand } from "@/lib/content/brand";
import { getCurrentUser, logout } from "@/lib/auth/session";
import { adminNavGroups, isNavCurrent } from "@/lib/admin/nav";
import { isStaff } from "@/lib/admin/roles";
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

  useEffect(() => {
    setOpen(false);
  }, [pathname]);

  async function onLogout() {
    await logout();
    router.replace("/login");
    router.refresh();
  }

  if (!user) {
    return <BrandLoading label="جاري تحميل لوحة الإدارة..." />;
  }

  const groups = adminNavGroups
    .map((group) => ({ ...group, items: group.items.filter((item) => item.visible(user.roles)) }))
    .filter((group) => group.items.length > 0);

  return (
    <ToastProvider>
      <div className="min-h-full bg-background text-foreground">
        <div className="lg:grid lg:min-h-screen lg:grid-cols-[17rem_minmax(0,1fr)]">
          {/* A01 · شريط جانبي كحلي */}
          <aside
            id={menuId}
            className={`${
              open ? "fixed inset-y-0 inset-inline-start-0 z-40 flex w-[min(100%,17rem)] flex-col" : "hidden"
            } bg-surface-dark text-primary-foreground lg:static lg:flex lg:w-auto lg:flex-col`}
          >
            <div className="flex items-center justify-between gap-3 border-b border-white/10 px-4 py-4 lg:px-5">
              <Link href="/admin" className="inline-flex min-w-0 items-center gap-2.5" onClick={() => setOpen(false)}>
                <BrandMark className="h-8 w-8" />
                <span className="min-w-0">
                  <span className="block truncate text-sm font-extrabold tracking-tight">{brand.nameAr}</span>
                  <span className="mt-0.5 block text-[11px] font-semibold text-accent">لوحة الإدارة</span>
                </span>
              </Link>
              <button
                type="button"
                className="rounded-xl border border-white/15 px-3 py-1.5 text-sm lg:hidden"
                onClick={() => setOpen(false)}
              >
                إغلاق
              </button>
            </div>

            <nav aria-label="تنقل لوحة الإدارة" className="flex-1 overflow-y-auto px-3 py-5 lg:px-4">
              <div className="flex flex-col gap-6 text-sm">
                {groups.map((group) => (
                  <div key={group.title}>
                    <p className="px-3 text-[11px] font-bold tracking-wide text-accent">{group.title}</p>
                    <div className="mt-2 flex flex-col gap-0.5">
                      {group.items.map((item) => {
                        const current = isNavCurrent(pathname, item);
                        return (
                          <Link
                            key={item.href}
                            href={item.href}
                            aria-current={current ? "page" : undefined}
                            className={`rounded-xl px-3 py-2.5 transition ${
                              current
                                ? "bg-accent font-semibold text-primary-foreground shadow-[0_8px_20px_rgba(249,115,22,0.28)]"
                                : "text-primary-foreground/75 hover:bg-white/10 hover:text-primary-foreground"
                            }`}
                            onClick={() => setOpen(false)}
                          >
                            {item.label}
                          </Link>
                        );
                      })}
                    </div>
                  </div>
                ))}
              </div>
            </nav>

            <div className="border-t border-white/10 px-4 py-4 lg:px-5">
              <p className="truncate text-sm font-medium">{user.fullName}</p>
              <p className="mt-0.5 truncate text-xs text-primary-foreground/55">{user.email}</p>
              <div className="mt-4 flex flex-col gap-2">
                <Link
                  href="/dashboard"
                  className="rounded-xl border border-white/15 px-3 py-2 text-center text-sm transition hover:border-accent hover:text-accent"
                >
                  لوحة الطالب
                </Link>
                <Link
                  href="/"
                  className="rounded-xl border border-accent/40 px-3 py-2 text-center text-sm font-semibold text-accent transition hover:bg-accent hover:text-primary-foreground"
                >
                  الموقع
                </Link>
                <Button
                  variant="ghost"
                  className="justify-center text-primary-foreground/70 hover:bg-white/10 hover:text-primary-foreground"
                  onClick={onLogout}
                >
                  تسجيل الخروج
                </Button>
              </div>
            </div>
          </aside>

          {open ? (
            <button
              type="button"
              className="fixed inset-0 z-30 bg-surface-dark/45 backdrop-blur-[2px] lg:hidden"
              aria-label="إغلاق القائمة"
              onClick={() => setOpen(false)}
            />
          ) : null}

          <div className="flex min-w-0 flex-col">
            <header className="sticky top-0 z-20 border-b border-border bg-surface/95 backdrop-blur">
              <div className="flex items-center justify-between gap-2 px-3 py-3 sm:gap-3 sm:px-6 sm:py-3.5 lg:px-8">
                <div className="min-w-0 lg:hidden">
                  <Link href="/admin" className="inline-flex items-center gap-2">
                    <BrandMark className="h-7 w-7" />
                    <span className="truncate text-sm font-extrabold tracking-tight">{brand.nameAr}</span>
                  </Link>
                </div>
                <p className="hidden text-sm text-muted lg:block">مرحباً، {user.fullName}</p>
                <div className="flex shrink-0 items-center gap-2">
                  <Link
                    href="/"
                    className="hidden rounded-xl border border-border px-3 py-2 text-sm font-semibold transition hover:border-accent hover:text-accent md:inline-flex"
                  >
                    الموقع
                  </Link>
                  <button
                    type="button"
                    className="inline-flex h-10 w-10 items-center justify-center rounded-xl border border-border bg-surface lg:hidden"
                    aria-expanded={open}
                    aria-controls={menuId}
                    aria-label={open ? "إغلاق القائمة" : "فتح القائمة"}
                    onClick={() => setOpen(true)}
                  >
                    <span aria-hidden="true" className="relative block h-4 w-5">
                      <span className="absolute start-0 top-0 block h-0.5 w-5 rounded-full bg-current" />
                      <span className="absolute start-0 top-1.5 block h-0.5 w-5 rounded-full bg-current" />
                      <span className="absolute start-0 top-3 block h-0.5 w-5 rounded-full bg-current" />
                    </span>
                  </button>
                </div>
              </div>
            </header>

            <main id="main" className="flex-1 px-3 py-5 sm:px-6 sm:py-8 lg:px-8">
              <div className="mx-auto w-full max-w-6xl">{children}</div>
            </main>
          </div>
        </div>
      </div>
    </ToastProvider>
  );
}
