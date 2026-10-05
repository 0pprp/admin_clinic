import { homeSteps } from "@/lib/content/brand";

export function HomeSteps() {
  return (
    <section className="clinic-shell py-6 sm:py-8 md:py-10" aria-label="مسار العمل">
      {/* Mobile + tablet: single column; desktop: 3-up */}
      <ul className="grid gap-3 sm:gap-4 lg:grid-cols-3">
        {homeSteps.map((step) => (
          <li key={step.code} className="clinic-card px-4 py-6 text-center sm:px-6 sm:py-7">
            <p className="text-sm font-extrabold text-foreground">
              {step.code} / {step.title}
            </p>
            <p className="mt-3 text-sm leading-7 text-muted sm:text-[0.95rem]">{step.body}</p>
          </li>
        ))}
      </ul>
    </section>
  );
}
