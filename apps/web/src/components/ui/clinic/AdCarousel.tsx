"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import type { AdSlide } from "@/lib/content/ads";
import { cn } from "./cn";

function Bars({ light }: { light?: boolean }) {
  const bars = [28, 44, 62, 84];
  return (
    <div className="flex h-28 items-end gap-2 sm:h-36 sm:gap-3" aria-hidden="true">
      {bars.map((h, i) => (
        <div
          key={h}
          className={cn(
            "w-4 rounded-sm sm:w-5",
            i === bars.length - 1 ? "bg-accent" : light ? "bg-primary/20" : "bg-white/25"
          )}
          style={{ height: `${h}%` }}
        />
      ))}
    </div>
  );
}

export function AdCarousel({ slides }: { slides: AdSlide[] }) {
  const [index, setIndex] = useState(0);

  useEffect(() => {
    if (slides.length < 2) {
      return;
    }
    const id = window.setInterval(() => {
      setIndex((current) => (current + 1) % slides.length);
    }, 5500);
    return () => window.clearInterval(id);
  }, [slides.length]);

  if (slides.length === 0) {
    return null;
  }

  const slide = slides[index] ?? slides[0];
  const navy = slide.tone === "navy";

  return (
    <section className="relative overflow-hidden" aria-roledescription="carousel" aria-label="إعلانات العيادة">
      <div
        className={cn(
          "relative mx-auto max-w-6xl overflow-hidden rounded-2xl border",
          navy ? "border-white/10 bg-surface-dark text-primary-foreground" : "border-border bg-surface text-foreground"
        )}
      >
        <div className="grid items-center gap-8 px-6 py-8 sm:px-10 sm:py-10 lg:grid-cols-[1.2fr_0.8fr] lg:px-12 lg:py-12">
          <div>
            <p className={cn("font-naskh text-base font-bold sm:text-lg", navy ? "text-accent-soft" : "text-accent")}>
              {slide.eyebrow}
            </p>
            <h2 className="mt-3 text-2xl font-semibold leading-tight tracking-tight sm:text-3xl lg:text-4xl">
              {slide.title}
            </h2>
            <p className={cn("mt-4 max-w-xl text-sm leading-8 sm:text-base", navy ? "text-white/75" : "text-muted")}>
              {slide.body}
            </p>
            <Link
              href={slide.ctaHref}
              className={cn(
                "mt-6 inline-flex rounded-md px-5 py-3 text-sm font-medium transition",
                navy
                  ? "bg-accent text-primary-foreground hover:bg-accent-soft"
                  : "bg-primary text-primary-foreground hover:bg-[#0a2744]"
              )}
            >
              {slide.ctaLabel}
            </Link>
          </div>
          <div className="flex justify-start lg:justify-end">
            <Bars light={!navy} />
          </div>
        </div>
        {slides.length > 1 ? (
          <div className="absolute inset-x-0 bottom-3 flex justify-center gap-2">
            {slides.map((item, i) => (
              <button
                key={item.id}
                type="button"
                aria-label={`الإعلان ${i + 1}`}
                aria-current={i === index}
                className={cn(
                  "h-2 w-2 rounded-full transition",
                  i === index ? "bg-accent" : navy ? "bg-white/35" : "bg-primary/25"
                )}
                onClick={() => setIndex(i)}
              />
            ))}
          </div>
        ) : null}
      </div>
    </section>
  );
}
