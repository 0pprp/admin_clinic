import { cn } from "./cn";

const tones = ["bg-[#fff3df]", "bg-[#e6f6ef]", "bg-[#e5f2ff]", "bg-[#fde8ef]"] as const;

export function FeedbackCard({
  quote,
  name,
  role,
  index = 0
}: {
  quote: string;
  name: string;
  role?: string;
  index?: number;
}) {
  return (
    <article className={cn("rounded-xl p-6", tones[index % tones.length])}>
      <div className="flex gap-1 text-accent" aria-hidden="true">
        {Array.from({ length: 5 }).map((_, i) => (
          <span key={i}>★</span>
        ))}
      </div>
      <p className="mt-4 text-sm leading-8 text-foreground">{quote}</p>
      <div className="mt-6 flex items-center gap-3">
        <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary text-sm font-semibold text-primary-foreground">
          {name.slice(0, 1)}
        </div>
        <div>
          <p className="text-sm font-semibold">{name}</p>
          {role ? <p className="text-xs text-muted">{role}</p> : null}
        </div>
      </div>
    </article>
  );
}
