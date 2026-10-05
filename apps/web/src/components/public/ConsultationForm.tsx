"use client";

import { FormEvent, useState } from "react";
import { ApiRequestError, apiFetch, parseJson } from "@/lib/api/client";
import type { CreateConsultationResponse } from "@/lib/admin/types";

const TYPES = [
  { value: "Business", label: "أعمال" },
  { value: "Marketing", label: "تسويق" },
  { value: "Management", label: "إدارة" },
  { value: "Personal", label: "شخصي" },
  { value: "Other", label: "أخرى" }
] as const;

const METHODS = [
  { value: "Phone", label: "هاتف" },
  { value: "WhatsApp", label: "واتساب" },
  { value: "Email", label: "بريد" }
] as const;

const fieldClass = "mt-2 w-full border border-border bg-background px-3 py-2 text-sm";

export function ConsultationForm() {
  const [fullName, setFullName] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [whatsAppNumber, setWhatsAppNumber] = useState("");
  const [email, setEmail] = useState("");
  const [consultationType, setConsultationType] = useState("Business");
  const [companyName, setCompanyName] = useState("");
  const [preferredDate, setPreferredDate] = useState("");
  const [preferredTime, setPreferredTime] = useState("");
  const [topic, setTopic] = useState("");
  const [message, setMessage] = useState("");
  const [preferredCommunicationMethod, setPreferredCommunicationMethod] = useState("WhatsApp");
  const [website, setWebsite] = useState("");
  const [error, setError] = useState("");
  const [pending, setPending] = useState(false);
  const [success, setSuccess] = useState<CreateConsultationResponse | null>(null);

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");
    setPending(true);
    try {
      const response = await apiFetch("/api/consultations", {
        method: "POST",
        body: JSON.stringify({
          fullName: fullName.trim(),
          phoneNumber: phoneNumber.trim(),
          whatsAppNumber: whatsAppNumber.trim() || null,
          email: email.trim() || null,
          consultationType,
          companyName: companyName.trim() || null,
          preferredDate: preferredDate || null,
          preferredTime: preferredTime ? (preferredTime.length === 5 ? `${preferredTime}:00` : preferredTime) : null,
          topic: topic.trim(),
          message: message.trim(),
          preferredCommunicationMethod,
          website: website || null
        })
      });
      const created = await parseJson<CreateConsultationResponse>(response, "تعذر إرسال طلب الاستشارة.");
      setSuccess(created);
    } catch (caught) {
      setError(caught instanceof ApiRequestError ? caught.message : "تعذر إرسال طلب الاستشارة.");
    } finally {
      setPending(false);
    }
  }

  if (success) {
    return (
      <section className="border border-border bg-surface px-6 py-8">
        <p className="text-lg font-semibold">تم استلام طلب الاستشارة بنجاح.</p>
        <p className="mt-3 text-sm">رقم الطلب: {success.requestNumber}</p>
        <p className="mt-4 max-w-xl text-sm leading-8 text-muted">
          سيتواصل معك فريق العيادة الإدارية لتأكيد الموعد والتفاصيل.
        </p>
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
        الاسم الكامل
        <input className={fieldClass} value={fullName} onChange={(event) => setFullName(event.target.value)} required />
      </label>
      <label className="block text-sm">
        رقم الهاتف
        <input className={fieldClass} inputMode="tel" value={phoneNumber} onChange={(event) => setPhoneNumber(event.target.value)} required />
      </label>
      <label className="block text-sm">
        رقم واتساب
        <input className={fieldClass} inputMode="tel" value={whatsAppNumber} onChange={(event) => setWhatsAppNumber(event.target.value)} />
      </label>
      <label className="block text-sm">
        البريد الإلكتروني
        <input className={fieldClass} type="email" value={email} onChange={(event) => setEmail(event.target.value)} />
      </label>
      <label className="block text-sm">
        نوع الاستشارة
        <select className={fieldClass} value={consultationType} onChange={(event) => setConsultationType(event.target.value)}>
          {TYPES.map((item) => (
            <option key={item.value} value={item.value}>
              {item.label}
            </option>
          ))}
        </select>
      </label>
      <label className="block text-sm">
        اسم الشركة
        <input className={fieldClass} value={companyName} onChange={(event) => setCompanyName(event.target.value)} />
      </label>
      <div className="grid gap-5 sm:grid-cols-2">
        <label className="block text-sm">
          التاريخ المفضل
          <input className={fieldClass} type="date" value={preferredDate} onChange={(event) => setPreferredDate(event.target.value)} />
        </label>
        <label className="block text-sm">
          الوقت المفضل
          <input className={fieldClass} type="time" value={preferredTime} onChange={(event) => setPreferredTime(event.target.value)} />
        </label>
      </div>
      <label className="block text-sm">
        الموضوع
        <input className={fieldClass} value={topic} onChange={(event) => setTopic(event.target.value)} required />
      </label>
      <label className="block text-sm">
        الرسالة
        <textarea className={fieldClass} rows={5} value={message} onChange={(event) => setMessage(event.target.value)} required />
      </label>
      <label className="block text-sm">
        طريقة التواصل المفضلة
        <select className={fieldClass} value={preferredCommunicationMethod} onChange={(event) => setPreferredCommunicationMethod(event.target.value)}>
          {METHODS.map((item) => (
            <option key={item.value} value={item.value}>
              {item.label}
            </option>
          ))}
        </select>
      </label>
      {error ? <p className="text-sm text-red-700">{error}</p> : null}
      <button type="submit" disabled={pending} className="inline-flex border border-accent bg-accent px-5 py-3 text-sm text-surface-dark disabled:opacity-50">
        {pending ? "جاري الإرسال..." : "إرسال طلب الاستشارة"}
      </button>
    </form>
  );
}
