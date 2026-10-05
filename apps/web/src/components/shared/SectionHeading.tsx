import { BrandAccentLabel } from "@/components/brand/BrandAccentLabel";

export function SectionHeading({
  eyebrow,
  title,
  description
}: {
  eyebrow?: string;
  title: string;
  description?: string;
}) {
  return (
    <div className="max-w-2xl">
      {eyebrow ? (
        <BrandAccentLabel className="text-sm font-bold tracking-wide sm:text-base">
          {eyebrow}
        </BrandAccentLabel>
      ) : null}
      <h2 className="mt-3 text-3xl font-semibold leading-tight tracking-tight sm:text-4xl">{title}</h2>
      {description ? <p className="mt-4 text-base leading-8 text-muted">{description}</p> : null}
    </div>
  );
}
