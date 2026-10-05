"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { FormEvent, useEffect, useState } from "react";
import { DashboardShell } from "@/components/dashboard/DashboardShell";
import { ApiRequestError, apiFetch, parseJson } from "@/lib/api/client";
import { getCurrentUser } from "@/lib/auth/session";
import { normalizeActivationInput, type RedeemActivationResponse } from "@/lib/activation";

export default function DashboardActivatePage() {
  const router = useRouter();
  const [ready, setReady] = useState(false);
  const [code, setCode] = useState("");
  const [error, setError] = useState("");
  const [pending, setPending] = useState(false);
  const [result, setResult] = useState<RedeemActivationResponse | null>(null);

  useEffect(() => {
    getCurrentUser().then((session) => {
      if (!session) {
        router.replace("/login?from=/dashboard/activate");
        return;
      }

      setReady(true);
    });
  }, [router]);

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (pending) {
      return;
    }

    setError("");
    setPending(true);
    try {
      const response = await apiFetch("/api/activation/redeem", {
        method: "POST",
        body: JSON.stringify({ code: normalizeActivationInput(code) })
      });
      const body = await parseJson<RedeemActivationResponse>(response, "كود التفعيل غير صحيح أو غير متاح للاستخدام.");
      setResult(body);
      router.refresh();
    } catch (caught) {
      setError(
        caught instanceof ApiRequestError
          ? caught.message
          : "كود التفعيل غير صحيح أو غير متاح للاستخدام."
      );
    } finally {
      setPending(false);
    }
  }

  if (!ready) {
    return (
      <DashboardShell title="تفعيل دورة">
        <p className="text-sm text-muted">جاري التحقق من الجلسة...</p>
      </DashboardShell>
    );
  }

  if (result) {
    return (
      <DashboardShell title="تم تفعيل الدورة بنجاح">
        <section className="max-w-2xl border border-border bg-surface px-5 py-8 sm:px-8">
          <p className="text-sm leading-8 text-muted">أصبحت الدورة جزءاً من حسابك، ويمكنك البدء متى شئت.</p>
          <h2 className="mt-6 text-2xl font-semibold">{result.courseTitle}</h2>
          <Link
            href={`/dashboard/courses/${result.courseSlug}`}
            className="mt-8 inline-flex border border-accent px-5 py-2.5 text-sm text-accent"
          >
            ابدأ التعلم
          </Link>
        </section>
      </DashboardShell>
    );
  }

  return (
    <DashboardShell title="تفعيل دورة">
      <p className="max-w-2xl text-sm leading-8 text-muted">
        تم استلام دفعتك؟ أدخل كود التفعيل الذي استلمته من فريق العيادة الإدارية.
      </p>
      <form onSubmit={onSubmit} className="mt-10 max-w-xl border border-border bg-surface px-5 py-8 sm:px-8">
        <p className="text-xs tracking-[0.2em] text-accent">لديك كود تفعيل؟</p>
        <label className="mt-6 block text-sm" htmlFor="activation-code">
          كود التفعيل
        </label>
        <input
          id="activation-code"
          name="code"
          value={code}
          onChange={(event) => setCode(event.target.value)}
          autoComplete="off"
          spellCheck={false}
          dir="ltr"
          placeholder="MR-8K2P-7X4M"
          className="mt-2 w-full border border-border bg-background px-3 py-3 text-left tracking-[0.18em] outline-none focus:border-accent"
        />
        {error ? <p className="mt-4 text-sm text-red-400">{error}</p> : null}
        <button
          type="submit"
          disabled={pending || code.trim().length === 0}
          className="mt-8 border border-accent bg-accent px-5 py-2.5 text-sm text-background disabled:opacity-60"
        >
          {pending ? "جاري التفعيل..." : "تفعيل الدورة"}
        </button>
      </form>
    </DashboardShell>
  );
}
