export type AdSlide = {
  id: string;
  eyebrow: string;
  title: string;
  body: string;
  ctaLabel?: string;
  ctaHref?: string;
  tone: "navy" | "light";
};

export const homeAdSlides: AdSlide[] = [
  {
    id: "lead",
    eyebrow: "العيادة الإدارية / إعلان",
    title: "قيادة تصنع الفرق",
    body: "كورسات عملية للمديرين وقادة الفرق",
    tone: "navy"
  },
  {
    id: "diagnose",
    eyebrow: "العيادة الإدارية / إعلان",
    title: "ابدأ بالتشخيص الصحيح",
    body: "استشارات إدارية لفهم واقع مؤسستك قبل أي علاج أو توسع.",
    ctaLabel: "اطلب استشارة",
    ctaHref: "/consultation",
    tone: "navy"
  },
  {
    id: "practice",
    eyebrow: "العيادة الإدارية / إعلان",
    title: "حوّل المعرفة إلى ممارسة",
    body: "كورسات وتطبيقات تساعدك تنقل الفكرة من الشاشة إلى يوم العمل.",
    ctaLabel: "تصفّح الكورسات",
    ctaHref: "/courses",
    tone: "navy"
  }
];
