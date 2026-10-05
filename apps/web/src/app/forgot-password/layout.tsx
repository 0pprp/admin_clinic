import type { Metadata } from "next";
import type { ReactNode } from "react";
import { AuthPageChrome } from "@/components/auth/AuthPageChrome";

export const metadata: Metadata = {
  title: "استعادة كلمة المرور",
  robots: { index: false, follow: false }
};

export default function ForgotPasswordLayout({ children }: { children: ReactNode }) {
  return <AuthPageChrome>{children}</AuthPageChrome>;
}
