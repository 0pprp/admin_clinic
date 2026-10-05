import type { Metadata } from "next";
import type { ReactNode } from "react";
import { AuthPageChrome } from "@/components/auth/AuthPageChrome";

export const metadata: Metadata = {
  title: "تسجيل الدخول",
  robots: { index: false, follow: false }
};

export default function AuthLayout({ children }: { children: ReactNode }) {
  return <AuthPageChrome>{children}</AuthPageChrome>;
}
