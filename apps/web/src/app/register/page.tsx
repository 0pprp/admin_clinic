"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { FormEvent, useState } from "react";
import { AuthShell, Field, buttonClassName, inputClassName } from "@/components/auth/AuthShell";
import { ClinicSelect } from "@/components/ui/clinic";
import { ApiRequestError } from "@/lib/api/client";
import { register } from "@/lib/auth/session";
import { IRAQ_GOVERNORATES } from "@/lib/data/governorates";

export default function RegisterPage() {
  const router = useRouter();
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [whatsAppNumber, setWhatsAppNumber] = useState("");
  const [governorate, setGovernorate] = useState("");
  const [password, setPassword] = useState("");
  const [passwordConfirmation, setPasswordConfirmation] = useState("");
  const [termsAccepted, setTermsAccepted] = useState(false);
  const [error, setError] = useState("");
  const [pending, setPending] = useState(false);

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");

    if (!fullName.trim()) {
      setError("الاسم الكامل مطلوب.");
      return;
    }

    if (!email.trim()) {
      setError("البريد الإلكتروني مطلوب.");
      return;
    }

    if (!phoneNumber.trim()) {
      setError("رقم الهاتف مطلوب.");
      return;
    }

    if (!governorate) {
      setError("المحافظة مطلوبة.");
      return;
    }

    if (password.length < 8) {
      setError("كلمة المرور يجب أن تكون 8 أحرف على الأقل.");
      return;
    }

    if (password !== passwordConfirmation) {
      setError("تأكيد كلمة المرور غير مطابق.");
      return;
    }

    if (!termsAccepted) {
      setError("يجب قبول الشروط والمتابعة.");
      return;
    }

    setPending(true);
    try {
      await register({
        fullName: fullName.trim(),
        email: email.trim(),
        phoneNumber: phoneNumber.trim(),
        whatsAppNumber: whatsAppNumber.trim(),
        governorate,
        password,
        passwordConfirmation,
        termsAccepted
      });
      router.replace("/login");
      router.refresh();
    } catch (caught) {
      setError(caught instanceof ApiRequestError ? caught.message : "تعذر إنشاء الحساب.");
    } finally {
      setPending(false);
    }
  }

  return (
    <AuthShell title="إنشاء حساب" description="التسجيل متاح الآن بدون تأكيد بريد إلكتروني إلزامي.">
      <form onSubmit={onSubmit} noValidate>
        <Field label="الاسم الكامل">
          <input className={inputClassName} value={fullName} onChange={(event) => setFullName(event.target.value)} />
        </Field>
        <Field label="البريد الإلكتروني">
          <input
            className={inputClassName}
            type="email"
            autoComplete="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
          />
        </Field>
        <Field label="رقم الهاتف">
          <input
            className={inputClassName}
            inputMode="tel"
            value={phoneNumber}
            onChange={(event) => setPhoneNumber(event.target.value)}
          />
        </Field>
        <Field label="رقم واتساب">
          <input
            className={inputClassName}
            inputMode="tel"
            value={whatsAppNumber}
            onChange={(event) => setWhatsAppNumber(event.target.value)}
          />
        </Field>
        <Field label="المحافظة">
          <ClinicSelect
            value={governorate}
            onChange={setGovernorate}
            placeholder="اختر المحافظة"
            options={IRAQ_GOVERNORATES.map((item) => ({ value: item, label: item }))}
          />
        </Field>
        <Field label="كلمة المرور">
          <input
            className={inputClassName}
            type="password"
            autoComplete="new-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />
        </Field>
        <Field label="تأكيد كلمة المرور">
          <input
            className={inputClassName}
            type="password"
            autoComplete="new-password"
            value={passwordConfirmation}
            onChange={(event) => setPasswordConfirmation(event.target.value)}
          />
        </Field>
        <label className="mb-4 flex items-start gap-2 text-sm">
          <input
            className="mt-1"
            type="checkbox"
            checked={termsAccepted}
            onChange={(event) => setTermsAccepted(event.target.checked)}
          />
          قبول الشروط
        </label>
        {error ? <p className="mb-4 text-sm text-red-700">{error}</p> : null}
        <button className={buttonClassName} type="submit" disabled={pending}>
          {pending ? "جاري إنشاء الحساب..." : "إنشاء حساب"}
        </button>
      </form>
      <p className="mt-4 text-sm text-[#6B7280]">
        لديك حساب؟{" "}
        <Link className="font-medium text-[#0E141B] underline" href="/login">
          تسجيل الدخول
        </Link>
      </p>
    </AuthShell>
  );
}
