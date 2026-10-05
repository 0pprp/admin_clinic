"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { DashboardShell } from "@/components/dashboard/DashboardShell";
import { EmptyState } from "@/components/shared/EmptyState";
import { apiFetch, parseJson } from "@/lib/api/client";
import { getCurrentUser } from "@/lib/auth/session";
import { formatBaghdadDateTime, formatIqd } from "@/lib/format";
import { purchaseStatusText, type StudentPurchaseSummary } from "@/lib/purchases";
import type { Paged } from "@/lib/api/public-types";

export default function DashboardOrdersPage() {
  const router = useRouter();
  const [items, setItems] = useState<StudentPurchaseSummary[] | null>(null);

  useEffect(() => {
    getCurrentUser().then(async (session) => {
      if (!session) {
        router.replace("/login?from=/dashboard/orders");
        return;
      }

      const response = await apiFetch("/api/purchase-requests?page=1&pageSize=20");
      const data = await parseJson<Paged<StudentPurchaseSummary>>(response, "تعذر تحميل الطلبات.");
      setItems(data.items);
    });
  }, [router]);

  if (!items) {
    return (
      <DashboardShell title="طلباتي">
        <p className="text-sm text-muted">جاري تحميل الطلبات...</p>
      </DashboardShell>
    );
  }

  return (
    <DashboardShell title="طلباتي">
      {items.length === 0 ? (
        <EmptyState title="لا توجد طلبات بعد" description="يمكنك إرسال طلب اشتراك من صفحة الدورة." />
      ) : (
        <ul className="divide-y divide-border border-y border-border">
          {items.map((item) => (
            <li key={item.id} className="flex flex-col gap-2 py-5 sm:flex-row sm:items-center sm:justify-between">
              <div>
                <p className="font-medium">{item.courseTitle}</p>
                <p className="mt-1 text-xs text-muted">
                  {item.requestNumber} · {formatBaghdadDateTime(item.createdAt)}
                </p>
              </div>
              <div className="flex items-center justify-between gap-4 sm:justify-end">
                <p className="text-sm">{formatIqd(item.amountIQD)}</p>
                <p className="text-sm text-muted">{purchaseStatusText(item.status)}</p>
                <Link href={`/dashboard/orders/${item.id}`} className="text-sm text-accent hover:underline">
                  التفاصيل
                </Link>
              </div>
            </li>
          ))}
        </ul>
      )}
    </DashboardShell>
  );
}
