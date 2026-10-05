import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "استعادة كلمة المرور",
  robots: { index: false, follow: false }
};

export default function ForgotPasswordLayout({ children }: LayoutProps<"/forgot-password">) {
  return children;
}
