/** مقدمة الصفحات الداخلية — مطابق لأسلوب Figma Clinic (eyebrow برتقالي + عنوان عريض + وصف مكتوم) */
export function PageIntro({
  eyebrow,
  title,
  description,
  embedded = false,
  className = ""
}: {
  eyebrow?: string;
  title: string;
  description?: string;
  /** true عند التضمين داخل clinic-shell موجود مسبقاً */
  embedded?: boolean;
  className?: string;
}) {
  const body = (
    <header className={`max-w-3xl ${className}`.trim()}>
      {eyebrow ? <p className="text-sm font-bold text-accent">{eyebrow}</p> : null}
      <h1 className="mt-3 text-3xl font-extrabold tracking-tight sm:text-4xl">{title}</h1>
      {description ? <p className="mt-3 text-sm leading-8 text-muted sm:text-base">{description}</p> : null}
    </header>
  );

  if (embedded) {
    return body;
  }

  return <div className="clinic-shell pt-10 sm:pt-14">{body}</div>;
}
