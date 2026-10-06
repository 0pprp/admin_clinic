"use client";

import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { FormEvent, Suspense, useMemo, useState } from "react";
import { AuthShell, buttonClassName } from "@/components/auth/AuthShell";
import { OtpInput } from "@/components/auth/OtpInput";
import { ApiRequestError } from "@/lib/api/client";
import { resendVerification, verifyEmail } from "@/lib/auth/session";
import { safeInternalPath } from "@/lib/safe-path";

export default function VerifyEmailPage() {
  return (
    <Suspense>
      <VerifyEmailForm />
    </Suspense>
  );
}

function VerifyEmailForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const email = useMemo(() => searchParams.get("email")?.trim() ?? "", [searchParams]);
  const purpose = searchParams.get("purpose")?.trim() || "EmailVerification";
  const [code, setCode] = useState("");
  const [error, setError] = useState("");
  const [message, setMessage] = useState("");
  const [pending, setPending] = useState(false);
  const [resending, setResending] = useState(false);

  const title =
    purpose === "GoogleLogin" ? "رمز تحقق Google" : purpose === "PasswordReset" ? "رمز الاستعادة" : "تأكيد البريد";

  const description =
    purpose === "GoogleLogin"
      ? "أدخل الرمز الذي وصل إلى Gmail لإكمال تسجيل الدخول عبر Google."
      : "أدخل رمز التحقق المكوّن من 6 أرقام الذي أرسلناه إلى بريدك.";

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");
    setMessage("");

    if (!email) {
      setError("البريد الإلكتروني مفقود. ارجع لصفحة التسجيل أو تسجيل الدخول.");
      return;
    }

    if (code.trim().length < 4) {
      setError("أدخل رمز التحقق كاملاً.");
      return;
    }

    setPending(true);
    try {
      await verifyEmail({
        email,
        token: code.trim(),
        purpose,
        rememberMe: true
      });
      const from = safeInternalPath(searchParams.get("from"));
      router.replace(from || "/");
      router.refresh();
    } catch (caught) {
      setError(caught instanceof ApiRequestError ? caught.message : "تعذر تأكيد الرمز.");
    } finally {
      setPending(false);
    }
  }

  async function onResend() {
    if (!email) return;
    setResending(true);
    setError("");
    setMessage("");
    try {
      setMessage(await resendVerification(email, purpose));
    } catch (caught) {
      setError(caught instanceof ApiRequestError ? caught.message : "تعذر إعادة الإرسال.");
    } finally {
      setResending(false);
    }
  }

  return (
    <AuthShell title={title} description={description}>
      <form onSubmit={onSubmit} noValidate className="space-y-5">
        <p className="text-sm text-muted">
          الرمز مُرسل إلى: <span className="font-bold text-foreground">{email || "—"}</span>
        </p>
        <OtpInput value={code} onChange={setCode} disabled={pending} />
        {error ? <p className="text-sm text-red-700">{error}</p> : null}
        {message ? <p className="text-sm text-emerald-800">{message}</p> : null}
        <button className={buttonClassName} type="submit" disabled={pending || !email}>
          {pending ? "جاري التحقق..." : "تأكيد الرمز"}
        </button>
      </form>
      <div className="mt-5 flex flex-col gap-2 text-sm">
        <button
          type="button"
          className="text-foreground underline disabled:opacity-50"
          onClick={onResend}
          disabled={resending || !email}
        >
          {resending ? "جاري الإرسال..." : "إعادة إرسال الرمز"}
        </button>
        <Link className="underline" href="/login">
          العودة لتسجيل الدخول
        </Link>
      </div>
    </AuthShell>
  );
}
