"use client";

import { FormEvent, useState } from "react";
import { ApiRequestError, apiFetch, parseJson } from "@/lib/api/client";
import type { CreateContactResponse } from "@/lib/admin/types";

const fieldClass = "mt-2 w-full border border-border bg-background px-3 py-2 text-sm";

export function ContactForm() {
  const [name, setName] = useState("");
  const [phone, setPhone] = useState("");
  const [email, setEmail] = useState("");
  const [subject, setSubject] = useState("");
  const [message, setMessage] = useState("");
  const [website, setWebsite] = useState("");
  const [error, setError] = useState("");
  const [pending, setPending] = useState(false);
  const [success, setSuccess] = useState(false);

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");
    setPending(true);
    try {
      const response = await apiFetch("/api/contact", {
        method: "POST",
        body: JSON.stringify({
          name: name.trim(),
          phone: phone.trim() || null,
          email: email.trim(),
          subject: subject.trim(),
          message: message.trim(),
          website: website || null
        })
      });
      await parseJson<CreateContactResponse>(response, "تعذر إرسال الرسالة.");
      setSuccess(true);
    } catch (caught) {
      setError(caught instanceof ApiRequestError ? caught.message : "تعذر إرسال الرسالة.");
    } finally {
      setPending(false);
    }
  }

  if (success) {
    return (
      <section className="border border-border bg-surface px-6 py-8">
        <p className="text-lg font-semibold">تم استلام رسالتك بنجاح.</p>
        <p className="mt-3 max-w-xl text-sm leading-8 text-muted">سيتواصل معك الفريق عند الحاجة عبر البيانات التي أرسلتها.</p>
      </section>
    );
  }

  return (
    <form onSubmit={onSubmit} className="relative max-w-2xl space-y-5" noValidate>
      <div className="absolute h-0 w-0 overflow-hidden opacity-0" aria-hidden="true">
        <label>
          الموقع
          <input tabIndex={-1} autoComplete="off" value={website} onChange={(event) => setWebsite(event.target.value)} />
        </label>
      </div>
      <label className="block text-sm">
        الاسم
        <input className={fieldClass} value={name} onChange={(event) => setName(event.target.value)} required />
      </label>
      <label className="block text-sm">
        الهاتف
        <input className={fieldClass} inputMode="tel" value={phone} onChange={(event) => setPhone(event.target.value)} />
      </label>
      <label className="block text-sm">
        البريد الإلكتروني
        <input className={fieldClass} type="email" value={email} onChange={(event) => setEmail(event.target.value)} required />
      </label>
      <label className="block text-sm">
        الموضوع
        <input className={fieldClass} value={subject} onChange={(event) => setSubject(event.target.value)} required />
      </label>
      <label className="block text-sm">
        الرسالة
        <textarea className={fieldClass} rows={5} value={message} onChange={(event) => setMessage(event.target.value)} required />
      </label>
      {error ? <p className="text-sm text-red-700">{error}</p> : null}
      <button type="submit" disabled={pending} className="inline-flex border border-accent bg-accent px-5 py-3 text-sm text-surface-dark disabled:opacity-50">
        {pending ? "جاري الإرسال..." : "إرسال الرسالة"}
      </button>
    </form>
  );
}
