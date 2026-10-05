"use client";

import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { PageIntro } from "@/components/shared/PageIntro";
import { Button, ClinicField, ClinicInput } from "@/components/ui/clinic";
import { ApiRequestError, apiFetch, parseJson } from "@/lib/api/client";
import { getCurrentUser } from "@/lib/auth/session";
import type { UserSummary } from "@/lib/api/types";
import { safeInternalPath } from "@/lib/safe-path";

/** S08 · الملف الشخصي — نماذج ClinicField */
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
    return (
      <div>
        <PageIntro embedded eyebrow="مساحة المتعلم" title="الملف الشخصي" description="جاري تحميل الملف الشخصي..." />
      </div>
    );
  }

  return (
    <div className="max-w-xl">
      <PageIntro
        embedded
        eyebrow="مساحة المتعلم"
        title="الملف الشخصي"
        description="حدّث بيانات التواصل الخاصة بك. البريد الإلكتروني غير قابل للتعديل حالياً."
      />

      <form onSubmit={onSaveProfile} className="clinic-card mt-8 px-5 py-8 sm:px-8">
        <p className="mb-6 text-sm font-bold text-accent">بيانات التواصل</p>
        <ClinicField label="الاسم الكامل">
          <ClinicInput value={fullName} onChange={(event) => setFullName(event.target.value)} required />
        </ClinicField>
        <ClinicField label="البريد الإلكتروني" hint="لا يمكن تعديل البريد من هنا.">
          <ClinicInput value={user.email} readOnly className="opacity-70" />
        </ClinicField>
        <ClinicField label="رقم الهاتف">
          <ClinicInput inputMode="tel" value={phoneNumber} onChange={(event) => setPhoneNumber(event.target.value)} />
        </ClinicField>
        <ClinicField label="واتساب">
          <ClinicInput inputMode="tel" value={whatsAppNumber} onChange={(event) => setWhatsAppNumber(event.target.value)} />
        </ClinicField>
        <ClinicField label="المحافظة">
          <ClinicInput value={governorate} onChange={(event) => setGovernorate(event.target.value)} />
        </ClinicField>
        {profileError ? <p className="mb-4 text-sm text-red-600">{profileError}</p> : null}
        {profileSuccess ? <p className="mb-4 text-sm font-semibold text-accent">{profileSuccess}</p> : null}
        <Button type="submit" variant="accent" size="lg" className="w-full" disabled={profilePending}>
          {profilePending ? "جاري الحفظ..." : "حفظ التغييرات"}
        </Button>
      </form>

      <form onSubmit={onChangePassword} className="clinic-card mt-8 px-5 py-8 sm:px-8">
        <h2 className="text-xl font-extrabold tracking-tight">تغيير كلمة المرور</h2>
        <p className="mt-2 text-sm text-muted">بعد التحديث ستُعاد إلى صفحة تسجيل الدخول.</p>
        <div className="mt-6">
          <ClinicField label="كلمة المرور الحالية">
            <ClinicInput
              type="password"
              autoComplete="current-password"
              value={currentPassword}
              onChange={(event) => setCurrentPassword(event.target.value)}
            />
          </ClinicField>
          <ClinicField label="كلمة المرور الجديدة">
            <ClinicInput
              type="password"
              autoComplete="new-password"
              value={newPassword}
              onChange={(event) => setNewPassword(event.target.value)}
            />
          </ClinicField>
          <ClinicField label="تأكيد كلمة المرور">
            <ClinicInput
              type="password"
              autoComplete="new-password"
              value={confirmPassword}
              onChange={(event) => setConfirmPassword(event.target.value)}
            />
          </ClinicField>
        </div>
        {passwordError ? <p className="mb-4 text-sm text-red-600">{passwordError}</p> : null}
        {passwordSuccess ? <p className="mb-4 text-sm font-semibold text-accent">{passwordSuccess}</p> : null}
        <Button type="submit" variant="primary" size="lg" className="w-full" disabled={passwordPending}>
          {passwordPending ? "جاري التحديث..." : "تغيير كلمة المرور"}
        </Button>
      </form>
    </div>
  );
}
