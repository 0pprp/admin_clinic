import type { Metadata } from "next";
import type { ReactNode } from "react";
import { AuthPageChrome } from "@/components/auth/AuthPageChrome";

export const metadata: Metadata = {
  title: "تعيين كلمة المرور",
  robots: { index: false, follow: false }
};

export default function ResetPasswordLayout({ children }: { children: ReactNode }) {
  return <AuthPageChrome>{children}</AuthPageChrome>;
}
