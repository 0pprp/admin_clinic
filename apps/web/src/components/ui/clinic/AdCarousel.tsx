"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import type { AdSlide } from "@/lib/content/ads";
import { cn } from "./cn";

function Bars() {
  const bars = [
    { h: 34, accent: false },
    { h: 48, accent: false },
    { h: 72, accent: true },
    { h: 56, accent: false },
    { h: 40, accent: false }
  ];
  return (
    <div className="flex h-28 items-end gap-2.5 sm:h-32" aria-hidden="true">
      {bars.map((bar, index) => (
        <div
          key={index}
          className={cn("w-4 rounded-md sm:w-5", bar.accent ? "bg-accent" : "bg-white/20")}
          style={{ height: `${bar.h}%` }}
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

  return (
    <section className="clinic-shell pb-4 sm:pb-6" aria-roledescription="carousel" aria-label="إعلانات العيادة">
      <div className="overflow-hidden rounded-[1.5rem] border border-white/10 bg-surface-dark text-primary-foreground">
        <div className="grid items-center gap-8 px-6 py-8 sm:px-10 sm:py-10 lg:grid-cols-[1.15fr_0.85fr] lg:px-12">
          <div>
            <p className="text-sm font-bold text-accent">{slide.eyebrow}</p>
            <h2 className="mt-3 text-2xl font-extrabold leading-tight tracking-tight sm:text-3xl">{slide.title}</h2>
            <p className="mt-3 max-w-xl text-sm leading-8 text-primary-foreground/70">{slide.body}</p>
            <Link
              href={slide.ctaHref}
              className="mt-6 inline-flex rounded-xl bg-accent px-5 py-3 text-sm font-semibold text-primary-foreground transition hover:bg-accent-soft"
            >
              {slide.ctaLabel}
            </Link>
          </div>
          <div className="flex justify-start lg:justify-end">
            <Bars />
          </div>
        </div>
        {slides.length > 1 ? (
          <div className="flex items-center justify-center gap-2 pb-5">
            {slides.map((item, i) => (
              <button
                key={item.id}
                type="button"
                aria-label={`الإعلان ${i + 1}`}
                aria-current={i === index}
                className={cn("h-2 w-2 rounded-full transition", i === index ? "bg-accent" : "bg-white/30")}
                onClick={() => setIndex(i)}
              />
            ))}
          </div>
        ) : null}
      </div>
    </section>
  );
}
