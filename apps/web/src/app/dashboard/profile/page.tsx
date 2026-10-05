"use client";

import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { Field, buttonClassName, inputClassName } from "@/components/auth/AuthShell";
import { ApiRequestError, apiFetch, parseJson } from "@/lib/api/client";
import { getCurrentUser } from "@/lib/auth/session";
import type { UserSummary } from "@/lib/api/types";
import { safeInternalPath } from "@/lib/safe-path";

export default function DashboardProfilePage() {
  const router = useRouter();
  const [user, setUser] = useState<UserSummary | null>(null);
  const [fullName, setFullName] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [whatsAppNumber, setWhatsAppNumber] = useState("");
  const [governorate, setGovernorate] = useState("");
  const [profileError, setProfileError] = useState("");
  const [profileSuccess, setProfileSuccess] = useState("");
  const [profilePending, setProfilePending] = useState(false);
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [passwordError, setPasswordError] = useState("");
  const [passwordSuccess, setPasswordSuccess] = useState("");
  const [passwordPending, setPasswordPending] = useState(false);

  useEffect(() => {
    getCurrentUser().then(async (session) => {
      if (!session) {
        router.replace(`/login?from=${encodeURIComponent(safeInternalPath("/dashboard/profile"))}`);
        return;
      }

      const response = await apiFetch("/api/profile");
      if (response.status === 401) {
        router.replace(`/login?from=${encodeURIComponent(safeInternalPath("/dashboard/profile"))}`);
        return;
      }

      const profile = await parseJson<UserSummary>(response, "تعذر تحميل الملف الشخصي.");
      setUser(profile);
      setFullName(profile.fullName);
      setPhoneNumber(profile.phoneNumber ?? "");
      setWhatsAppNumber(profile.whatsAppNumber ?? "");
      setGovernorate(profile.governorate ?? "");
    });
  }, [router]);

  async function onSaveProfile(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setProfileError("");
    setProfileSuccess("");
    setProfilePending(true);
    try {
      const response = await apiFetch("/api/profile", {
        method: "PUT",
        body: JSON.stringify({
          fullName,
          phoneNumber,
          whatsAppNumber: whatsAppNumber.trim() || null,
          governorate
        })
      });
      const body = await parseJson<UserSummary>(response, "تعذر حفظ التغييرات.");
      setUser(body);
      setProfileSuccess("تم حفظ التغييرات.");
    } catch (caught) {
      setProfileError(caught instanceof ApiRequestError ? caught.message : "تعذر حفظ التغييرات.");
    } finally {
      setProfilePending(false);
    }
  }

  async function onChangePassword(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setPasswordError("");
    setPasswordSuccess("");
    setPasswordPending(true);
    try {
      const response = await apiFetch("/api/profile/password", {
        method: "PUT",
        body: JSON.stringify({
          currentPassword,
          newPassword,
          confirmPassword
        })
      });
      const body = await parseJson<{ message: string }>(response, "تعذر تغيير كلمة المرور.");
      setPasswordSuccess(body.message);
      setCurrentPassword("");
      setNewPassword("");
      setConfirmPassword("");
      router.replace("/login");
      router.refresh();
    } catch (caught) {
      setPasswordError(caught instanceof ApiRequestError ? caught.message : "تعذر تغيير كلمة المرور.");
    } finally {
      setPasswordPending(false);
    }
  }

  if (!user) {
    return <p className="text-sm text-muted">جاري تحميل الملف الشخصي...</p>;
  }

  return (
    <div className="max-w-xl">
      <h1 className="text-3xl font-extrabold tracking-tight">الملف الشخصي</h1>
      <p className="mt-3 max-w-2xl text-sm leading-8 text-muted">حدّث بيانات التواصل الخاصة بك. البريد الإلكتروني غير قابل للتعديل حالياً.</p>
      <form onSubmit={onSaveProfile} className="clinic-card mt-8 max-w-xl px-5 py-8 sm:px-8">
        <Field label="الاسم الكامل">
          <input className={inputClassName} value={fullName} onChange={(event) => setFullName(event.target.value)} required />
        </Field>
        <Field label="البريد الإلكتروني">
          <input className={`${inputClassName} opacity-70`} value={user.email} readOnly />
        </Field>
        <Field label="رقم الهاتف">
          <input className={inputClassName} value={phoneNumber} onChange={(event) => setPhoneNumber(event.target.value)} />
        </Field>
        <Field label="واتساب">
          <input className={inputClassName} value={whatsAppNumber} onChange={(event) => setWhatsAppNumber(event.target.value)} />
        </Field>
        <Field label="المحافظة">
          <input className={inputClassName} value={governorate} onChange={(event) => setGovernorate(event.target.value)} />
        </Field>
        {profileError ? <p className="mb-4 text-sm text-red-600">{profileError}</p> : null}
        {profileSuccess ? <p className="mb-4 text-sm text-accent">{profileSuccess}</p> : null}
        <button type="submit" disabled={profilePending} className={buttonClassName}>
          {profilePending ? "جاري الحفظ..." : "حفظ التغييرات"}
        </button>
      </form>
      <form onSubmit={onChangePassword} className="clinic-card mt-8 max-w-xl px-5 py-8 sm:px-8">
        <h2 className="text-xl font-semibold">تغيير كلمة المرور</h2>
        <div className="mt-6">
          <Field label="كلمة المرور الحالية">
            <input
              className={inputClassName}
              type="password"
              autoComplete="current-password"
              value={currentPassword}
              onChange={(event) => setCurrentPassword(event.target.value)}
            />
          </Field>
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
        </div>
        {passwordError ? <p className="mb-4 text-sm text-red-600">{passwordError}</p> : null}
        {passwordSuccess ? <p className="mb-4 text-sm text-accent">{passwordSuccess}</p> : null}
        <button type="submit" disabled={passwordPending} className={buttonClassName}>
          {passwordPending ? "جاري التحديث..." : "تغيير كلمة المرور"}
        </button>
      </form>
    </div>
  );
}
