import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "إنشاء حساب",
  robots: { index: false, follow: false }
};

export default function RegisterLayout({ children }: LayoutProps<"/register">) {
  return children;
}
