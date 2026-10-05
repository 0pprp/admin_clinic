"use client";

import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { FormEvent, Suspense, useState } from "react";
import { AuthShell, Field, buttonClassName, inputClassName } from "@/components/auth/AuthShell";
import { ApiRequestError } from "@/lib/api/client";
import { login } from "@/lib/auth/session";
import { safeInternalPath } from "@/lib/safe-path";

export default function LoginPage() {
  return (
    <Suspense>
      <LoginForm />
    </Suspense>
  );
}

function LoginForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [rememberMe, setRememberMe] = useState(false);
  const [error, setError] = useState("");
  const [pending, setPending] = useState(false);

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");

    if (!email.trim()) {
      setError("البريد الإلكتروني مطلوب.");
      return;
    }

    if (!password) {
      setError("كلمة المرور مطلوبة.");
      return;
    }

    setPending(true);
    try {
      await login({ email: email.trim(), password, rememberMe });
      const from = safeInternalPath(searchParams.get("from"));
      const intent = searchParams.get("intent");
      const destination =
        intent === "enroll" && from.startsWith("/courses/") && !from.includes("/enroll")
          ? `${from}${from.includes("?") ? "&" : "?"}intent=enroll`
          : from;
      router.replace(destination);
      router.refresh();
    } catch (caught) {
      setError(caught instanceof ApiRequestError ? caught.message : "تعذر تسجيل الدخول.");
    } finally {
      setPending(false);
    }
  }

  return (
    <AuthShell title="تسجيل الدخول" description="أدخل بيانات حسابك للمتابعة.">
      <form onSubmit={onSubmit} noValidate>
        <Field label="البريد الإلكتروني">
          <input
            className={inputClassName}
            type="email"
            autoComplete="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
          />
        </Field>
        <Field label="كلمة المرور">
          <input
            className={inputClassName}
            type="password"
            autoComplete="current-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />
        </Field>
        <label className="mb-4 flex items-center gap-2 text-sm">
          <input
            type="checkbox"
            checked={rememberMe}
            onChange={(event) => setRememberMe(event.target.checked)}
          />
          تذكرني
        </label>
        {error ? <p className="mb-4 text-sm text-red-700">{error}</p> : null}
        <button className={buttonClassName} type="submit" disabled={pending}>
          {pending ? "جاري تسجيل الدخول..." : "تسجيل الدخول"}
        </button>
      </form>
      <p className="mt-4 text-sm">
        <Link className="underline" href="/forgot-password">
          نسيت كلمة المرور؟
        </Link>
      </p>
      <p className="mt-2 text-sm text-[#6B7280]">
        ليس لديك حساب؟{" "}
        <Link
          className="font-medium text-[#0E141B] underline"
          href={searchParams.get("from") ? `/register?from=${encodeURIComponent(searchParams.get("from") ?? "")}` : "/register"}
        >
          إنشاء حساب
        </Link>
      </p>
    </AuthShell>
  );
}
