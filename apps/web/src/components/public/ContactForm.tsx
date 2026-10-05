"use client";

import { FormEvent, useState } from "react";
import { ApiRequestError, apiFetch, parseJson } from "@/lib/api/client";
import type { CreateContactResponse } from "@/lib/admin/types";
import { Button } from "@/components/ui/clinic";
import { ClinicField, ClinicInput, ClinicTextarea } from "@/components/ui/clinic/Field";

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
      <div className="text-center">
        <p className="text-lg font-extrabold text-foreground">تم استلام رسالتك بنجاح.</p>
        <p className="mt-3 text-sm leading-8 text-muted">سيتواصل معك الفريق عند الحاجة عبر البيانات التي أرسلتها.</p>
      </div>
    );
  }

  return (
    <form onSubmit={onSubmit} className="relative space-y-1" noValidate>
      <div className="absolute h-0 w-0 overflow-hidden opacity-0" aria-hidden="true">
        <label>
          الموقع
          <input tabIndex={-1} autoComplete="off" value={website} onChange={(event) => setWebsite(event.target.value)} />
        </label>
      </div>

      <ClinicField label="الاسم">
        <ClinicInput value={name} onChange={(event) => setName(event.target.value)} required />
      </ClinicField>
      <ClinicField label="الهاتف">
        <ClinicInput inputMode="tel" value={phone} onChange={(event) => setPhone(event.target.value)} />
      </ClinicField>
      <ClinicField label="البريد الإلكتروني">
        <ClinicInput type="email" value={email} onChange={(event) => setEmail(event.target.value)} required />
      </ClinicField>
      <ClinicField label="الموضوع">
        <ClinicInput value={subject} onChange={(event) => setSubject(event.target.value)} required />
      </ClinicField>
      <ClinicField label="الرسالة">
        <ClinicTextarea rows={5} value={message} onChange={(event) => setMessage(event.target.value)} required />
      </ClinicField>

      {error ? <p className="mb-4 text-sm text-red-700">{error}</p> : null}

      <Button type="submit" variant="accent" size="lg" className="mt-2 w-full" disabled={pending}>
        {pending ? "جاري الإرسال..." : "إرسال الرسالة"}
      </Button>
    </form>
  );
}
