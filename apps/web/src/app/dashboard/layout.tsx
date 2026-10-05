import type { Metadata } from "next";
import { StudentPortal } from "@/components/dashboard/StudentPortal";

export const metadata: Metadata = {
  title: "لوحة التحكم",
  robots: { index: false, follow: false }
};

export default function DashboardLayout({ children }: LayoutProps<"/dashboard">) {
  return <StudentPortal>{children}</StudentPortal>;
}
