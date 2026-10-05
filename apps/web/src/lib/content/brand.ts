export const brand = {
  nameAr: "العيادة الإدارية",
  nameEn: "THE MANAGEMENT CLINIC",
  taglineAr: "عالم أصغر... أفق أوسع",
  sloganAr: "شخّص . عالج . طوّر",
  siteTitle: "العيادة الإدارية",
  siteDescription:
    "استشارات وكورسات عملية تساعدك على فهم تحديات الإدارة وبناء فريق أقوى وخطوات قابلة للتطبيق."
} as const;

export const heroCopy = {
  eyebrow: brand.sloganAr,
  title: "قرارات أوضح. إدارة تصنع أثرًا.",
  description:
    "استشارات وكورسات عملية تساعدك على فهم تحديات الإدارة وبناء فريق أقوى وخطوات قابلة للتطبيق."
} as const;

export const homeSteps = [
  { code: "01", title: "شخّص", body: "افهم التحدي قبل اتخاذ القرار" },
  { code: "02", title: "عالج", body: "ضع خطة تناسب واقع فريقك" },
  { code: "03", title: "طوّر", body: "حوّل الحل إلى عادة مستمرة" }
] as const;

export const aboutPreviewCopy = {
  eyebrow: "عن العيادة",
  title: "من التشخيص إلى التطوير",
  body: "العيادة الإدارية مساحة عملية لمعالجة تحديات الإدارة والقيادة: نفهم الواقع، نحدد الخلل، ونضع مساراً أوضح للتطوير داخل المؤسسة أو المشروع.",
  note: "الهدف ليس التنظير، بل حلول إدارية يمكن تنفيذها."
} as const;

export const whyItems = [
  {
    title: "تشخيص دقيق",
    body: "نبدأ من فهم الواقع الإداري كما هو، لا من وصفات عامة جاهزة."
  },
  {
    title: "علاج عملي",
    body: "نركّز على معالجة الخلل الإداري بخطوات واضحة يمكن تطبيقها."
  },
  {
    title: "تطوير مستمر",
    body: "نبني قدرات الفريق والمدير على اتخاذ قرارات أنضج مع الوقت."
  },
  {
    title: "لغة المؤسسة",
    body: "المحتوى والاستشارات مصممة لواقع الأعمال والإدارة اليومية."
  }
] as const;

export const positioningAreas = [
  "الإدارة والقيادة",
  "بناء الفرق",
  "اتخاذ القرار",
  "تحسين الأداء",
  "الاستشارات الإدارية",
  "تطوير المديرين"
] as const;

export const consultationCopy = {
  title: "تحتاج تشخيصاً أوضح لمشكلتك الإدارية؟",
  body: "الاستشارة مساحة لفهم واقع مؤسستك أو فريقك، وتحديد ما يستحق العلاج قبل أي توسع أو تغيير.",
  cta: "اطلب استشارة"
} as const;

export const finalCtaCopy = {
  title: "ابدأ من التشخيص الصحيح.",
  body: "تصفّح الكورسات، أو اطلب استشارة إذا كنت تحتاج نقاشاً أعمق حول تحدٍ إداري محدد."
} as const;

export const featuredCoursesCopy = {
  title: "ابدأ من التحدي الذي تواجهه",
  description: "كورسات مصممة لواقع العمل والإدارة اليومية"
} as const;

export const navItems = [
  { href: "/", label: "الرئيسية" },
  { href: "/about", label: "عن العيادة" },
  { href: "/courses", label: "الكورسات" },
  { href: "/articles", label: "المقالات" },
  { href: "/consultation", label: "الاستشارات" },
  { href: "/contact", label: "تواصل معنا" }
] as const;
