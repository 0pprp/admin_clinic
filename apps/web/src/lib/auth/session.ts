import { apiFetch, parseJson } from "@/lib/api/client";
import type { UserSummary } from "@/lib/api/types";

export type AuthChallenge = {
  requiresEmailVerification: boolean;
  email: string;
  message: string;
  purpose: string;
};

export async function getCurrentUser(): Promise<UserSummary | null> {
  try {
    let response = await apiFetch("/api/auth/me");

    if (response.status === 401) {
      const refresh = await apiFetch("/api/auth/refresh", { method: "POST" });
      if (!refresh.ok) {
        return null;
      }

      response = await apiFetch("/api/auth/me");
    }

    if (!response.ok) {
      return null;
    }

    return (await response.json()) as UserSummary;
  } catch {
    return null;
  }
}

export async function login(input: {
  email: string;
  password: string;
  rememberMe: boolean;
}): Promise<UserSummary> {
  const response = await apiFetch("/api/auth/login", {
    method: "POST",
    body: JSON.stringify({
      email: input.email,
      password: input.password,
      rememberMe: input.rememberMe
    })
  });

  return parseJson<UserSummary>(response, "تعذر تسجيل الدخول.");
}

export async function googleLogin(input: {
  idToken: string;
  rememberMe?: boolean;
}): Promise<AuthChallenge> {
  const response = await apiFetch("/api/auth/google", {
    method: "POST",
    body: JSON.stringify({
      idToken: input.idToken,
      rememberMe: input.rememberMe ?? true
    })
  });

  return parseJson<AuthChallenge>(response, "تعذر تسجيل الدخول عبر Google.");
}

export async function register(input: {
  fullName: string;
  email: string;
  phoneNumber: string;
  whatsAppNumber: string;
  governorate: string;
  password: string;
  passwordConfirmation: string;
  termsAccepted: boolean;
}): Promise<UserSummary> {
  const response = await apiFetch("/api/auth/register", {
    method: "POST",
    body: JSON.stringify(input)
  });

  return parseJson<UserSummary>(response, "تعذر إنشاء الحساب.");
}

export async function logout(): Promise<void> {
  await apiFetch("/api/auth/logout", { method: "POST" });
}

export async function forgotPassword(email: string): Promise<string> {
  const response = await apiFetch("/api/auth/forgot-password", {
    method: "POST",
    body: JSON.stringify({ email })
  });
  const body = await parseJson<{ message: string }>(response, "تعذر إرسال طلب الاستعادة.");
  return body.message;
}

export async function resetPassword(input: {
  email: string;
  token: string;
  newPassword: string;
  confirmPassword: string;
}): Promise<string> {
  const response = await apiFetch("/api/auth/reset-password", {
    method: "POST",
    body: JSON.stringify(input)
  });
  const body = await parseJson<{ message: string }>(response, "تعذر تعيين كلمة المرور.");
  return body.message;
}

export async function verifyEmail(input: {
  email: string;
  token: string;
  purpose?: string;
  rememberMe?: boolean;
}): Promise<UserSummary> {
  const response = await apiFetch("/api/auth/verify-email", {
    method: "POST",
    body: JSON.stringify({
      email: input.email,
      token: input.token,
      purpose: input.purpose ?? "EmailVerification",
      rememberMe: input.rememberMe ?? true
    })
  });

  return parseJson<UserSummary>(response, "تعذر تأكيد الرمز.");
}

export async function resendVerification(email: string, purpose = "EmailVerification"): Promise<string> {
  const response = await apiFetch("/api/auth/resend-verification", {
    method: "POST",
    body: JSON.stringify({ email, purpose })
  });
  const body = await parseJson<{ message: string }>(response, "تعذر إعادة إرسال الرمز.");
  return body.message;
}
