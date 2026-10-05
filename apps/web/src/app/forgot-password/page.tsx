"use client";

import Link from "next/link";
import { FormEvent, useState } from "react";
import { AuthShell, Field, buttonClassName, inputClassName } from "@/components/auth/AuthShell";
import { ApiRequestError } from "@/lib/api/client";
import { forgotPassword } from "@/lib/auth/session";

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [pending, setPending] = useState(false);

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");
    setMessage("");

    if (!email.trim()) {
      setError("البريد الإلكتروني مطلوب.");
      return;
    }

    setPending(true);
    try {
      setMessage(await forgotPassword(email.trim()));
    } catch (caught) {
      setError(caught instanceof ApiRequestError ? caught.message : "تعذر إرسال الطلب.");
    } finally {
      setPending(false);
    }
  }

  return (
    <AuthShell title="استعادة كلمة المرور" description="سنرسل التعليمات إذا كان البريد مسجلاً لدينا.">
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
        {error ? <p className="mb-4 text-sm text-red-700">{error}</p> : null}
        {message ? <p className="mb-4 text-sm text-emerald-800">{message}</p> : null}
        <button className={buttonClassName} type="submit" disabled={pending}>
          {pending ? "جاري الإرسال..." : "إرسال التعليمات"}
        </button>
      </form>
      <p className="mt-4 text-sm">
        <Link className="underline" href="/login">
          العودة لتسجيل الدخول
        </Link>
      </p>
    </AuthShell>
  );
}
