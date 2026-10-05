import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "تسجيل الدخول",
  robots: { index: false, follow: false }
};

export default function AuthLayout({ children }: LayoutProps<"/login">) {
  return children;
}
