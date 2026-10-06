"use client";

import Link from "next/link";
import { FormEvent, useState } from "react";
import { AuthShell, Field, buttonClassName, inputClassName } from "@/components/auth/AuthShell";
import { OtpInput } from "@/components/auth/OtpInput";
import { ApiRequestError } from "@/lib/api/client";
import { resetPassword } from "@/lib/auth/session";

export function ResetPasswordForm({ email, token }: { email: string; token: string }) {
  const [currentEmail, setCurrentEmail] = useState(email);
  const [otp, setOtp] = useState(token && /^\d{4,8}$/.test(token) ? token : "");
  const [legacyToken] = useState(token && !/^\d{4,8}$/.test(token) ? token : "");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [pending, setPending] = useState(false);

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");
    setMessage("");

    const resetToken = legacyToken || otp.trim();
    if (!currentEmail.trim() || !resetToken) {
      setError("أدخل البريد ورمز الاستعادة.");
      return;
    }

    if (newPassword.length < 8) {
      setError("كلمة المرور يجب أن تكون 8 أحرف على الأقل.");
      return;
    }

    if (newPassword !== confirmPassword) {
      setError("تأكيد كلمة المرور غير مطابق.");
      return;
    }

    setPending(true);
    try {
      setMessage(
        await resetPassword({
          email: currentEmail.trim(),
          token: resetToken,
          newPassword,
          confirmPassword
        })
      );
    } catch (caught) {
      setError(caught instanceof ApiRequestError ? caught.message : "تعذر تعيين كلمة المرور.");
    } finally {
      setPending(false);
    }
  }

  return (
    <AuthShell title="تعيين كلمة مرور جديدة" description="أدخل رمز التحقق الذي وصلك ثم اختر كلمة مرور جديدة.">
      <form onSubmit={onSubmit} noValidate>
        <Field label="البريد الإلكتروني">
          <input
            className={inputClassName}
            type="email"
            value={currentEmail}
            onChange={(event) => setCurrentEmail(event.target.value)}
          />
        </Field>
        {!legacyToken ? (
          <div className="mb-4">
            <span className="mb-1.5 block text-sm font-bold text-foreground">رمز الاستعادة</span>
            <OtpInput value={otp} onChange={setOtp} disabled={pending} />
          </div>
        ) : null}
        <Field label="كلمة المرور الجديدة">
          <input
            className={inputClassName}
            type="password"
            autoComplete="new-password"
            value={newPassword}
            onChange={(event) => setNewPassword(event.target.value)}
          />
        </Field>
        <Field label="تأكيد كلمة المرور">
          <input
            className={inputClassName}
            type="password"
            autoComplete="new-password"
            value={confirmPassword}
            onChange={(event) => setConfirmPassword(event.target.value)}
          />
        </Field>
        {error ? <p className="mb-4 text-sm text-red-700">{error}</p> : null}
        {message ? <p className="mb-4 text-sm text-emerald-800">{message}</p> : null}
        <button className={buttonClassName} type="submit" disabled={pending}>
          {pending ? "جاري الحفظ..." : "تعيين كلمة المرور"}
        </button>
      </form>
      <p className="mt-4 text-sm">
        <Link className="underline" href="/login">
          تسجيل الدخول
        </Link>
      </p>
    </AuthShell>
  );
}
