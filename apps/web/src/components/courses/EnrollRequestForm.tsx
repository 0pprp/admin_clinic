"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { FormEvent, useEffect, useState } from "react";
import { ApiRequestError, apiFetch, parseJson } from "@/lib/api/client";
import { getCurrentUser } from "@/lib/auth/session";
import type { UserSummary } from "@/lib/api/types";
import { courseAccessLabel, formatIqd } from "@/lib/format";
import type { PurchaseCreated } from "@/lib/purchases";

export function EnrollRequestForm({
  courseId,
  courseTitle,
  slug,
  priceIQD,
  accessType,
  accessDurationDays
}: {
  courseId: string;
  courseTitle: string;
  slug: string;
  priceIQD: number;
  accessType: string;
  accessDurationDays: number | null;
}) {
  const router = useRouter();
  const [user, setUser] = useState<UserSummary | null>(null);
  const [notes, setNotes] = useState("");
  const [accepted, setAccepted] = useState(false);
  const [error, setError] = useState("");
  const [pending, setPending] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    getCurrentUser().then((session) => {
      if (!session) {
        router.replace(`/login?from=${encodeURIComponent(`/courses/${slug}`)}&intent=enroll`);
        return;
      }

      setUser(session);
      setLoading(false);
    });
  }, [router, slug]);

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!accepted || pending) {
      return;
    }

    setError("");
    setPending(true);
    try {
      const response = await apiFetch("/api/purchase-requests", {
        method: "POST",
        body: JSON.stringify({
          courseId,
          customerNotes: notes.trim() || null
        })
      });
      const created = await parseJson<PurchaseCreated>(response, "تعذر إرسال طلب الاشتراك.");
      router.replace(`/dashboard/orders/${created.id}?created=1`);
    } catch (caught) {
      setError(caught instanceof ApiRequestError ? caught.message : "تعذر إرسال طلب الاشتراك.");
      setPending(false);
    }
  }

  if (loading || !user) {
    return <p className="text-sm text-muted">جاري تحميل بيانات الحساب...</p>;
  }

  const profileIncomplete = !user.fullName || !user.phoneNumber || !user.whatsAppNumber || !user.email || !user.governorate;

  return (
    <form onSubmit={onSubmit} className="space-y-10">
      <section>
        <h2 className="text-xl font-semibold">ملخص الدورة</h2>
        <dl className="mt-4 grid gap-4 text-sm sm:grid-cols-2">
          <div>
            <dt className="text-muted">الدورة</dt>
            <dd className="mt-1 font-medium">{courseTitle}</dd>
          </div>
          <div>
            <dt className="text-muted">السعر</dt>
            <dd className="mt-1 font-medium">{formatIqd(priceIQD)}</dd>
          </div>
          <div>
            <dt className="text-muted">الوصول</dt>
            <dd className="mt-1">{courseAccessLabel(accessType, accessDurationDays)}</dd>
          </div>
        </dl>
      </section>
      <section>
        <div className="flex items-center justify-between gap-4">
          <h2 className="text-xl font-semibold">بيانات التواصل</h2>
          <Link href="/dashboard" className="text-sm text-accent hover:underline">
            تعديل بيانات الحساب
          </Link>
        </div>
        <dl className="mt-4 grid gap-4 text-sm sm:grid-cols-2">
          <div>
            <dt className="text-muted">الاسم</dt>
            <dd className="mt-1">{user.fullName}</dd>
          </div>
          <div>
            <dt className="text-muted">الهاتف</dt>
            <dd className="mt-1">{user.phoneNumber}</dd>
          </div>
          <div>
            <dt className="text-muted">واتساب</dt>
            <dd className="mt-1">{user.whatsAppNumber}</dd>
          </div>
          <div>
            <dt className="text-muted">البريد</dt>
            <dd className="mt-1 break-all">{user.email}</dd>
          </div>
          <div>
            <dt className="text-muted">المحافظة</dt>
            <dd className="mt-1">{user.governorate}</dd>
          </div>
        </dl>
        {profileIncomplete ? (
          <p className="mt-4 text-sm text-red-700">يرجى إكمال بيانات حسابك قبل إرسال طلب الاشتراك.</p>
        ) : null}
      </section>
      <section>
        <label className="block text-sm">
          ملاحظة اختيارية
          <textarea
            className="mt-2 w-full border border-border bg-background px-3 py-2 text-sm"
            rows={4}
            value={notes}
            onChange={(event) => setNotes(event.target.value)}
            maxLength={2000}
          />
        </label>
      </section>
      <label className="flex items-start gap-3 text-sm leading-7">
        <input
          className="mt-1"
          type="checkbox"
          checked={accepted}
          onChange={(event) => setAccepted(event.target.checked)}
        />
        أفهم أن إرسال الطلب لا يعني تفعيل الدورة، وسيتواصل معي الفريق لتأكيد الدفع.
      </label>
      {error ? <p className="text-sm text-red-700">{error}</p> : null}
      <button
        type="submit"
        disabled={pending || !accepted || profileIncomplete}
        className="inline-flex border border-accent bg-accent px-5 py-3 text-sm text-surface-dark disabled:opacity-50"
      >
        {pending ? "جاري إرسال الطلب..." : "إرسال طلب الاشتراك"}
      </button>
    </form>
  );
}
