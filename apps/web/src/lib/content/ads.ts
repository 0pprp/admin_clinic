export type AdSlide = {
  id: string;
  eyebrow: string;
  title: string;
  body: string;
  ctaLabel: string;
  ctaHref: string;
  tone: "navy" | "light";
};

/** محتوى كاروسيل الرئيسية — مطابق لنصوص إعلانات Figma */
export const homeAdSlides: AdSlide[] = [
  {
    id: "lead",
    eyebrow: "العيادة الإدارية",
    title: "قيادة تصنع الفرق",
    body: "طوّر أسلوب إدارتك بمسارات عملية من التشخيص إلى التنفيذ.",
    ctaLabel: "استكشف الكورسات",
    ctaHref: "/courses",
    tone: "navy"
  },
  {
    id: "diagnose",
    eyebrow: "ابدأ صح",
    title: "ابدأ بالتشخيص الصحيح",
    body: "قبل أي علاج إداري: افهم الخلل، رتّب الأولويات، وابنِ قراراً أوضح.",
    ctaLabel: "اطلب استشارة",
    ctaHref: "/consultation",
    tone: "light"
  },
  {
    id: "practice",
    eyebrow: "من المعرفة إلى التطبيق",
    title: "حوّل المعرفة إلى ممارسة",
    body: "كورسات وتطبيقات تساعدك تنقل الفكرة من الشاشة إلى يوم العمل.",
    ctaLabel: "تصفّح المحتوى",
    ctaHref: "/articles",
    tone: "navy"
  }
];
