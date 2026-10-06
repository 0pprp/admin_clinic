"use client";

import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { FormEvent, Suspense, useState } from "react";
import { AuthShell, Field, buttonClassName, inputClassName } from "@/components/auth/AuthShell";
import { GoogleSignInButton } from "@/components/auth/GoogleSignInButton";
import { ApiRequestError } from "@/lib/api/client";
import { googleLogin, login } from "@/lib/auth/session";
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
  const [error, setError] = useState("");
  const [pending, setPending] = useState(false);

  function destination() {
    const from = safeInternalPath(searchParams.get("from"));
    const intent = searchParams.get("intent");
    return intent === "enroll" && from.startsWith("/courses/") && !from.includes("/enroll")
      ? `${from}${from.includes("?") ? "&" : "?"}intent=enroll`
      : from;
  }

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
      await login({ email: email.trim(), password, rememberMe: false });
      router.replace(destination());
      router.refresh();
    } catch (caught) {
      const message = caught instanceof ApiRequestError ? caught.message : "تعذر تسجيل الدخول.";
      if (message.includes("تأكيد البريد")) {
        router.push(`/verify-email?email=${encodeURIComponent(email.trim())}&purpose=EmailVerification`);
        return;
      }
      setError(message);
    } finally {
      setPending(false);
    }
  }

  async function onGoogle(idToken: string) {
    setError("");
    setPending(true);
    try {
      const challenge = await googleLogin({ idToken, rememberMe: true });
      const from = searchParams.get("from");
      const qs = new URLSearchParams({
        email: challenge.email,
        purpose: challenge.purpose || "GoogleLogin"
      });
      if (from) qs.set("from", from);
      router.push(`/verify-email?${qs.toString()}`);
    } catch (caught) {
      setError(caught instanceof ApiRequestError ? caught.message : "تعذر تسجيل الدخول عبر Google.");
    } finally {
      setPending(false);
    }
  }

  return (
    <AuthShell title="أهلاً بعودتك" description="سجّل دخولك لمتابعة دوراتك واستشاراتك.">
      <form onSubmit={onSubmit} noValidate>
        <Field label="البريد الإلكتروني">
          <input
            className={inputClassName}
            type="email"
            autoComplete="email"
            placeholder="name@gmail.com"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
          />
        </Field>
        <Field label="كلمة المرور">
          <input
            className={inputClassName}
            type="password"
            autoComplete="current-password"
            placeholder="********"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />
        </Field>
        {error ? <p className="mb-4 text-sm text-red-700">{error}</p> : null}
        <button className={buttonClassName} type="submit" disabled={pending}>
          {pending ? "جاري تسجيل الدخول..." : "تسجيل الدخول"}
        </button>
      </form>

      <div className="my-5 flex items-center gap-3 text-xs text-muted">
        <span className="h-px flex-1 bg-border" />
        أو
        <span className="h-px flex-1 bg-border" />
      </div>

      <GoogleSignInButton onCredential={onGoogle} disabled={pending} />

      <div className="mt-5 space-y-2 text-start text-sm">
        <p>
          <Link className="text-foreground transition hover:text-accent" href="/forgot-password">
            نسيت كلمة المرور؟
          </Link>
        </p>
        <p>
          <Link
            className="font-bold text-foreground transition hover:text-accent"
            href={
              searchParams.get("from")
                ? `/register?from=${encodeURIComponent(searchParams.get("from") ?? "")}`
                : "/register"
            }
          >
            إنشاء حساب
          </Link>
        </p>
      </div>
    </AuthShell>
  );
}
