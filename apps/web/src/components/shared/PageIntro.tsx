import { BrandAccentLabel } from "@/components/brand/BrandAccentLabel";

export function PageIntro({
  eyebrow,
  title,
  description
}: {
  eyebrow?: string;
  title: string;
  description?: string;
}) {
  return (
    <section className="border-b border-border bg-surface">
      <div className="clinic-shell py-12 sm:py-16">
        {eyebrow ? <BrandAccentLabel className="text-sm font-bold sm:text-base">{eyebrow}</BrandAccentLabel> : null}
        <h1 className="mt-3 max-w-3xl text-3xl font-extrabold leading-tight tracking-tight sm:text-5xl">{title}</h1>
        {description ? <p className="mt-4 max-w-2xl text-base leading-8 text-muted">{description}</p> : null}
      </div>
    </section>
  );
}
