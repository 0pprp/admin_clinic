import type { Metadata } from "next";
import type { ReactNode } from "react";
import { AuthPageChrome } from "@/components/auth/AuthPageChrome";

export const metadata: Metadata = {
  title: "إنشاء حساب",
  robots: { index: false, follow: false }
};

export default function RegisterLayout({ children }: { children: ReactNode }) {
  return <AuthPageChrome>{children}</AuthPageChrome>;
}
