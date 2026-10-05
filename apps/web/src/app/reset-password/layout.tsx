import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "تعيين كلمة المرور",
  robots: { index: false, follow: false }
};

export default function ResetPasswordLayout({ children }: LayoutProps<"/reset-password">) {
  return children;
}
