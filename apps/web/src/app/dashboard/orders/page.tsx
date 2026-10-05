"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { DashboardShell } from "@/components/dashboard/DashboardShell";
import { EmptyState } from "@/components/shared/EmptyState";
import { Badge, ButtonLink } from "@/components/ui/clinic";
import { apiFetch, parseJson } from "@/lib/api/client";
import { getCurrentUser } from "@/lib/auth/session";
import { formatBaghdadDateTime, formatIqd } from "@/lib/format";
import { purchaseStatusText, type StudentPurchaseSummary } from "@/lib/purchases";
import type { Paged } from "@/lib/api/public-types";

function purchaseTone(status: string): "orange" | "mint" | "sand" | "rose" | "navy" | "sky" {
  switch (status) {
    case "Completed":
      return "mint";
    case "ActivationCodeIssued":
      return "orange";
    case "AwaitingPayment":
    case "PaymentReceived":
      return "sky";
    case "Rejected":
    case "Cancelled":
      return "rose";
    case "Contacted":
      return "sand";
    default:
      return "navy";
  }
}

/** S05 · طلباتي */
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
      <DashboardShell title="طلباتي" description="جاري تحميل الطلبات...">
        <div className="h-40 animate-pulse rounded-2xl bg-surface-warm" />
      </DashboardShell>
    );
  }

  return (
    <DashboardShell title="طلباتي" description="تابع حالة طلبات الاشتراك والتعليمات المرتبطة بها.">
      {items.length === 0 ? (
        <div className="space-y-5">
          <EmptyState title="لا توجد طلبات بعد" description="يمكنك إرسال طلب اشتراك من صفحة الدورة." />
          <ButtonLink href="/courses" variant="accent" size="md">
            استكشف الدورات
          </ButtonLink>
        </div>
      ) : (
        <ul className="clinic-card divide-y divide-border overflow-hidden">
          {items.map((item) => (
            <li key={item.id} className="flex flex-col gap-3 px-5 py-5 sm:flex-row sm:items-center sm:justify-between sm:px-6">
              <div className="min-w-0">
                <p className="font-bold">{item.courseTitle}</p>
                <p className="mt-1 text-xs text-muted">
                  {item.requestNumber} · {formatBaghdadDateTime(item.createdAt)}
                </p>
              </div>
              <div className="flex flex-wrap items-center gap-3 sm:justify-end">
                <p className="text-sm font-semibold">{formatIqd(item.amountIQD)}</p>
                <Badge tone={purchaseTone(item.status)}>{purchaseStatusText(item.status)}</Badge>
                <Link href={`/dashboard/orders/${item.id}`} className="text-sm font-semibold text-accent hover:underline">
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
