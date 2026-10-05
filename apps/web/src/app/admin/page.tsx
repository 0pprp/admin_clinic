"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { EmptyState } from "@/components/shared/EmptyState";
import { ForbiddenState } from "@/components/admin/ForbiddenState";
import { LoadingState, ErrorState, PageHeader } from "@/components/admin/PageHeader";
import { StatusBadge } from "@/components/admin/StatusBadge";
import { adminJson, errorMessage, isForbidden, isUnauthorized } from "@/lib/admin/http";
import {
  canManageConsultations,
  canManageCourses,
  canManagePayments,
  canManageStudents,
  canViewAuditLogs
} from "@/lib/admin/roles";
import { consultationStatusLabel, labelOrRaw, purchaseLabel, statusTone, activationStatusLabel } from "@/lib/admin/labels";
import type { AdminDashboardSummary } from "@/lib/admin/types";
import { getCurrentUser } from "@/lib/auth/session";
import { formatBaghdadDateTime } from "@/lib/format";
import { safeInternalPath } from "@/lib/safe-path";

export default function AdminDashboardPage() {
  const router = useRouter();
  const [roles, setRoles] = useState<string[] | null>(null);
  const [summary, setSummary] = useState<AdminDashboardSummary | null>(null);
  const [error, setError] = useState("");
  const [forbidden, setForbidden] = useState(false);

  useEffect(() => {
    getCurrentUser().then(async (session) => {
      if (!session) {
        router.replace(`/login?from=${encodeURIComponent(safeInternalPath("/admin"))}`);
        return;
      }

      setRoles(session.roles);
      try {
        const data = await adminJson<AdminDashboardSummary>("/api/admin/dashboard/summary", "تعذر تحميل النظرة العامة.");
        setSummary(data);
      } catch (caught) {
        if (isUnauthorized(caught)) {
          router.replace(`/login?from=${encodeURIComponent(safeInternalPath("/admin"))}`);
          return;
        }
        if (isForbidden(caught)) {
          setForbidden(true);
          return;
        }
        setError(errorMessage(caught, "تعذر تحميل النظرة العامة."));
      }
    });
  }, [router]);

  if (forbidden) {
    return <ForbiddenState />;
  }

  if (error) {
    return (
      <>
        <PageHeader title="نظرة عامة" />
        <ErrorState message={error} />
      </>
    );
  }

  if (!summary || !roles) {
    return (
      <>
        <PageHeader title="نظرة عامة" />
        <LoadingState />
      </>
    );
  }

  const cards = [
    canManagePayments(roles)
      ? { label: "طلبات بانتظار التواصل", value: summary.pendingPurchaseRequests, href: "/admin/purchase-requests?status=Pending" }
      : null,
    canManagePayments(roles)
      ? { label: "بانتظار الدفع", value: summary.awaitingPaymentRequests, href: "/admin/purchase-requests?status=AwaitingPayment" }
      : null,
    canManagePayments(roles)
      ? { label: "تم استلام الدفع", value: summary.paymentReceivedRequests, href: "/admin/purchase-requests?status=PaymentReceived" }
      : null,
    canManageStudents(roles)
      ? { label: "الطلاب النشطون", value: summary.activeStudents, href: "/admin/students?status=Active" }
      : null,
    canManageStudents(roles)
      ? { label: "اشتراكات نشطة", value: summary.activeEnrollments, href: "/admin/enrollments?status=Active" }
      : null,
    canManageCourses(roles)
      ? { label: "دورات منشورة", value: summary.publishedCourses, href: "/admin/courses?status=Published" }
      : null,
    canManageConsultations(roles)
      ? { label: "استشارات جديدة", value: summary.newConsultations, href: "/admin/consultations?status=New" }
      : null,
    canManageConsultations(roles)
      ? { label: "رسائل غير مقروءة", value: summary.unreadContactMessages, href: "/admin/contact-messages?status=New" }
      : null
  ].filter((item): item is { label: string; value: number; href: string } => item !== null);

  return (
    <>
      <PageHeader title="نظرة عامة" description="ملخص حالة المنصة حسب صلاحياتك." />
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {cards.map((card) => (
          <Link key={card.label} href={card.href} className="border border-border bg-surface px-5 py-5 hover:border-accent">
            <p className="text-xs text-muted">{card.label}</p>
            <p className="mt-3 text-2xl font-semibold">{card.value}</p>
          </Link>
        ))}
      </div>
      <div className="mt-10 grid gap-8 xl:grid-cols-2">
        {canManagePayments(roles) ? (
          <DashboardList
            title="آخر طلبات الاشتراك"
            empty="لا توجد طلبات حديثة."
            href="/admin/purchase-requests"
            items={summary.recentPurchaseRequests.map((item) => ({
              id: item.id,
              href: `/admin/purchase-requests/${item.id}`,
              title: item.fullName,
              meta: `${item.requestNumber} · ${item.courseTitle}`,
              status: purchaseLabel(item.status),
              toneStatus: item.status,
              date: item.createdAt
            }))}
          />
        ) : null}
        {canManagePayments(roles) ? (
          <DashboardList
            title="آخر التفعيلات"
            empty="لا توجد تفعيلات حديثة."
            href="/admin/activation-codes"
            items={summary.recentActivations.map((item) => ({
              id: item.id,
              href: "/admin/activation-codes",
              title: item.userFullName,
              meta: item.courseTitle,
              status: labelOrRaw(activationStatusLabel, item.status),
              toneStatus: item.status,
              date: item.createdAt
            }))}
          />
        ) : null}
        {canManageConsultations(roles) ? (
          <DashboardList
            title="استشارات جديدة"
            empty="لا توجد استشارات حديثة."
            href="/admin/consultations"
            items={summary.recentConsultations.map((item) => ({
              id: item.id,
              href: `/admin/consultations/${item.id}`,
              title: item.fullName,
              meta: item.requestNumber,
              status: labelOrRaw(consultationStatusLabel, item.status),
              toneStatus: item.status,
              date: item.createdAt
            }))}
          />
        ) : null}
        {canViewAuditLogs(roles) ? (
          <DashboardList
            title="النشاط الأخير"
            empty="لا يوجد نشاط حديث."
            href="/admin/audit-logs"
            items={summary.recentActivity.map((item) => ({
              id: item.id,
              href: "/admin/audit-logs",
              title: item.action,
              meta: `${item.actorName} · ${item.entityType}`,
              status: item.description,
              toneStatus: "neutral",
              date: item.createdAt
            }))}
          />
        ) : null}
      </div>
    </>
  );
}

function DashboardList({
  title,
  empty,
  href,
  items
}: {
  title: string;
  empty: string;
  href: string;
  items: Array<{
    id: string;
    href: string;
    title: string;
    meta: string;
    status: string;
    toneStatus: string;
    date: string;
  }>;
}) {
  return (
    <section className="border border-border bg-surface p-5">
      <div className="flex items-center justify-between gap-3">
        <h2 className="text-lg font-semibold">{title}</h2>
        <Link href={href} className="text-sm text-accent hover:underline">
          عرض الكل
        </Link>
      </div>
      {items.length === 0 ? (
        <div className="mt-4">
          <EmptyState title={empty} />
        </div>
      ) : (
        <ul className="mt-4 divide-y divide-border">
          {items.map((item) => (
            <li key={item.id} className="py-3">
              <Link href={item.href} className="block hover:bg-surface-warm/40">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <p className="font-medium">{item.title}</p>
                  <StatusBadge label={item.status} tone={statusTone(item.toneStatus)} />
                </div>
                <p className="mt-1 text-xs text-muted">
                  {item.meta} · {formatBaghdadDateTime(item.date)}
                </p>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
