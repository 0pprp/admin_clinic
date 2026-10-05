"use client";

import { FormEvent, useState } from "react";
import { ApiRequestError, apiFetch, parseJson } from "@/lib/api/client";
import type { CreateConsultationResponse } from "@/lib/admin/types";
import { Button } from "@/components/ui/clinic";
import { ClinicField, ClinicInput, ClinicTextarea, clinicInputClassName } from "@/components/ui/clinic/Field";

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
      <div className="text-center">
        <p className="text-lg font-extrabold text-foreground">تم استلام طلب الاستشارة بنجاح.</p>
        <p className="mt-3 text-sm font-semibold text-accent">رقم الطلب: {success.requestNumber}</p>
        <p className="mt-4 text-sm leading-8 text-muted">
          سيتواصل معك فريق العيادة الإدارية لتأكيد الموعد والتفاصيل.
        </p>
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

      <ClinicField label="الاسم الكامل">
        <ClinicInput value={fullName} onChange={(event) => setFullName(event.target.value)} required />
      </ClinicField>
      <ClinicField label="رقم الهاتف">
        <ClinicInput inputMode="tel" value={phoneNumber} onChange={(event) => setPhoneNumber(event.target.value)} required />
      </ClinicField>
      <ClinicField label="رقم واتساب">
        <ClinicInput inputMode="tel" value={whatsAppNumber} onChange={(event) => setWhatsAppNumber(event.target.value)} />
      </ClinicField>
      <ClinicField label="البريد الإلكتروني">
        <ClinicInput type="email" value={email} onChange={(event) => setEmail(event.target.value)} />
      </ClinicField>
      <ClinicField label="نوع الاستشارة">
        <select
          className={clinicInputClassName}
          value={consultationType}
          onChange={(event) => setConsultationType(event.target.value)}
        >
          {TYPES.map((item) => (
            <option key={item.value} value={item.value}>
              {item.label}
            </option>
          ))}
        </select>
      </ClinicField>
      <ClinicField label="اسم الشركة">
        <ClinicInput value={companyName} onChange={(event) => setCompanyName(event.target.value)} />
      </ClinicField>
      <div className="grid gap-1 sm:grid-cols-2 sm:gap-4">
        <ClinicField label="التاريخ المفضل">
          <ClinicInput type="date" value={preferredDate} onChange={(event) => setPreferredDate(event.target.value)} />
        </ClinicField>
        <ClinicField label="الوقت المفضل">
          <ClinicInput type="time" value={preferredTime} onChange={(event) => setPreferredTime(event.target.value)} />
        </ClinicField>
      </div>
      <ClinicField label="الموضوع">
        <ClinicInput value={topic} onChange={(event) => setTopic(event.target.value)} required />
      </ClinicField>
      <ClinicField label="الرسالة">
        <ClinicTextarea rows={5} value={message} onChange={(event) => setMessage(event.target.value)} required />
      </ClinicField>
      <ClinicField label="طريقة التواصل المفضلة">
        <select
          className={clinicInputClassName}
          value={preferredCommunicationMethod}
          onChange={(event) => setPreferredCommunicationMethod(event.target.value)}
        >
          {METHODS.map((item) => (
            <option key={item.value} value={item.value}>
              {item.label}
            </option>
          ))}
        </select>
      </ClinicField>

      {error ? <p className="mb-4 text-sm text-red-700">{error}</p> : null}

      <Button type="submit" variant="accent" size="lg" className="mt-2 w-full" disabled={pending}>
        {pending ? "جاري الإرسال..." : "إرسال طلب الاستشارة"}
      </Button>
    </form>
  );
}
