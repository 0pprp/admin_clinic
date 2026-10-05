import {
  canManageActivations,
  canManageConsultations,
  canManageContent,
  canManageCourses,
  canManagePayments,
  canManageStudents,
  canManageUsers,
  canViewAuditLogs
} from "./roles";

export type AdminNavItem = {
  href: string;
  label: string;
  exact?: boolean;
  visible: (roles: string[]) => boolean;
};

export type AdminNavGroup = {
  title: string;
  items: AdminNavItem[];
};

export const adminNavGroups: AdminNavGroup[] = [
  {
    title: "الرئيسية",
    items: [{ href: "/admin", label: "نظرة عامة", exact: true, visible: () => true }]
  },
  {
    title: "التشغيل اليومي",
    items: [
      { href: "/admin/purchase-requests", label: "طلبات الاشتراك والدفع", visible: canManagePayments },
      { href: "/admin/students", label: "الطلاب", visible: canManageStudents },
      { href: "/admin/enrollments", label: "الاشتراكات المفعّلة", visible: canManageStudents },
      { href: "/admin/activation-codes", label: "أكواد التفعيل", visible: canManageActivations },
      { href: "/admin/consultations", label: "طلبات الاستشارة", visible: canManageConsultations },
      { href: "/admin/contact-messages", label: "رسائل التواصل", visible: canManageConsultations }
    ]
  },
  {
    title: "المحتوى التعليمي",
    items: [
      { href: "/admin/courses", label: "الكورسات والدروس والفيديو", visible: canManageCourses },
      { href: "/admin/articles", label: "المقالات", visible: canManageContent },
      { href: "/admin/faq", label: "الأسئلة الشائعة", visible: canManageContent }
    ]
  },
  {
    title: "هوية الموقع والنصوص",
    items: [{ href: "/admin/settings", label: "النصوص والإعدادات الظاهرة", visible: () => true }]
  },
  {
    title: "النظام",
    items: [
      { href: "/admin/users", label: "المستخدمون والصلاحيات", visible: canManageUsers },
      { href: "/admin/audit-logs", label: "سجل العمليات", visible: canViewAuditLogs }
    ]
  }
];

export const adminNavItems: AdminNavItem[] = adminNavGroups.flatMap((group) => group.items);

export function isNavCurrent(pathname: string, item: AdminNavItem): boolean {
  if (item.exact) {
    return pathname === item.href;
  }

  return pathname === item.href || pathname.startsWith(`${item.href}/`);
}

export function pageTitleForPath(pathname: string, roles: string[]): string {
  const visible = adminNavItems.filter((item) => item.visible(roles));
  const match = visible
    .filter((item) => isNavCurrent(pathname, item))
    .sort((a, b) => b.href.length - a.href.length)[0];
  return match?.label ?? "لوحة الإدارة";
}
