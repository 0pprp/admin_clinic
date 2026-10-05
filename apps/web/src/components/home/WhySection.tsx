import { whyItems } from "@/lib/content/brand";
import { BrandAccentLabel } from "@/components/brand/BrandAccentLabel";
import { Container } from "@/components/shared/Container";

export function WhySection() {
  return (
    <section className="bg-surface-dark text-primary-foreground">
      <Container className="py-20">
        <div className="max-w-2xl">
          <BrandAccentLabel className="text-sm font-bold tracking-wide sm:text-base">المنصة</BrandAccentLabel>
          <h2 className="mt-3 text-3xl font-semibold leading-tight tracking-tight sm:text-4xl">لماذا هذه المنصة؟</h2>
          <p className="mt-4 text-base leading-8 text-primary-foreground/70">
            نقاط عامة عن أسلوب العمل في المنصة. ليست ادعاءات شخصية غير موثقة.
          </p>
        </div>
        <ul className="mt-14 grid gap-10 sm:grid-cols-2">
          {whyItems.map((item, index) => (
            <li key={item.title} className="border-s border-accent/40 ps-6">
              <BrandAccentLabel className="text-sm font-bold tracking-wide">
                {String(index + 1).padStart(2, "0")}
              </BrandAccentLabel>
              <h3 className="mt-3 text-2xl font-semibold">{item.title}</h3>
              <p className="mt-3 text-sm leading-8 text-primary-foreground/70">{item.body}</p>
            </li>
          ))}
        </ul>
      </Container>
    </section>
  );
}
