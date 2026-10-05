"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { FormEvent, useEffect, useState } from "react";
import { ApiRequestError, apiFetch, parseJson } from "@/lib/api/client";
import { getCurrentUser } from "@/lib/auth/session";
import type { UserSummary } from "@/lib/api/types";
import { courseAccessLabel, formatIqd } from "@/lib/format";
import type { PurchaseCreated } from "@/lib/purchases";
import { Button } from "@/components/ui/clinic";
import { ClinicField, ClinicTextarea } from "@/components/ui/clinic/Field";

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
    <form
      onSubmit={onSubmit}
      className="clinic-card space-y-6 rounded-[1.25rem] p-6 sm:p-8"
    >
      <section>
        <h2 className="text-lg font-extrabold">ملخص الدورة</h2>
        <dl className="mt-4 grid gap-4 text-sm sm:grid-cols-2">
          <div>
            <dt className="text-muted">الدورة</dt>
            <dd className="mt-1 font-semibold">{courseTitle}</dd>
          </div>
          <div>
            <dt className="text-muted">السعر</dt>
            <dd className="mt-1 text-lg font-extrabold text-accent">{formatIqd(priceIQD)}</dd>
          </div>
          <div className="sm:col-span-2">
            <dt className="text-muted">الوصول</dt>
            <dd className="mt-1 font-medium">{courseAccessLabel(accessType, accessDurationDays)}</dd>
          </div>
        </dl>
      </section>

      <section className="border-t border-border pt-6">
        <div className="flex items-center justify-between gap-4">
          <h2 className="text-lg font-extrabold">بيانات التواصل</h2>
          <Link href="/dashboard" className="text-sm font-semibold text-accent hover:underline">
            تعديل الحساب
          </Link>
        </div>
        <dl className="mt-4 grid gap-4 text-sm sm:grid-cols-2">
          <div>
            <dt className="text-muted">الاسم</dt>
            <dd className="mt-1 font-medium">{user.fullName}</dd>
          </div>
          <div>
            <dt className="text-muted">الهاتف</dt>
            <dd className="mt-1 font-medium">{user.phoneNumber}</dd>
          </div>
          <div>
            <dt className="text-muted">واتساب</dt>
            <dd className="mt-1 font-medium">{user.whatsAppNumber}</dd>
          </div>
          <div>
            <dt className="text-muted">البريد</dt>
            <dd className="mt-1 break-all font-medium">{user.email}</dd>
          </div>
          <div>
            <dt className="text-muted">المحافظة</dt>
            <dd className="mt-1 font-medium">{user.governorate}</dd>
          </div>
        </dl>
        {profileIncomplete ? (
          <p className="mt-4 text-sm text-red-700">يرجى إكمال بيانات حسابك قبل إرسال طلب الاشتراك.</p>
        ) : null}
      </section>

      <section className="border-t border-border pt-6">
        <ClinicField label="ملاحظة اختيارية">
          <ClinicTextarea
            rows={4}
            value={notes}
            onChange={(event) => setNotes(event.target.value)}
            maxLength={2000}
            placeholder="أي تفاصيل تريد إضافتها للطلب..."
          />
        </ClinicField>
      </section>

      <label className="flex items-start gap-3 text-sm leading-7 text-muted">
        <input
          className="mt-1 size-4 accent-[var(--accent)]"
          type="checkbox"
          checked={accepted}
          onChange={(event) => setAccepted(event.target.checked)}
        />
        أفهم أن إرسال الطلب لا يعني تفعيل الدورة، وسيتواصل معي الفريق لتأكيد الدفع.
      </label>

      {error ? <p className="text-sm text-red-700">{error}</p> : null}

      <Button
        type="submit"
        variant="accent"
        size="lg"
        className="w-full"
        disabled={pending || !accepted || profileIncomplete}
      >
        {pending ? "جاري إرسال الطلب..." : "إرسال طلب الاشتراك"}
      </Button>
    </form>
  );
}
