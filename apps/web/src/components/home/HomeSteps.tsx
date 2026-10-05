import { homeSteps } from "@/lib/content/brand";

export function HomeSteps() {
  return (
    <section className="clinic-shell py-8 sm:py-10" aria-label="مسار العمل">
      <ul className="grid gap-4 sm:grid-cols-3">
        {homeSteps.map((step) => (
          <li key={step.code} className="clinic-card px-5 py-7 text-center sm:px-6">
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
