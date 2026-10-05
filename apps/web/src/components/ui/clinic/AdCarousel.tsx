"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import type { AdSlide } from "@/lib/content/ads";
import { cn } from "./cn";

function Bars() {
  const bars = [
    { h: 28, accent: false },
    { h: 44, accent: true },
    { h: 58, accent: false },
    { h: 72, accent: false },
    { h: 88, accent: false }
  ];
  return (
    <div className="flex h-24 items-end gap-2 sm:h-28 sm:gap-2.5 md:h-32" aria-hidden="true">
      {bars.map((bar, index) => (
        <div
          key={index}
          className={cn("w-3.5 rounded-md sm:w-4 md:w-5", bar.accent ? "bg-accent" : "bg-white/20")}
          style={{ height: `${bar.h}%` }}
        />
      ))}
    </div>
  );
}

export function AdCarousel({
  slides,
  embedded = false
}: {
  slides: AdSlide[];
  /** عند التضمين داخل لوحة الإدارة دون clinic-shell */
  embedded?: boolean;
}) {
  const [index, setIndex] = useState(0);
  const [paused, setPaused] = useState(false);

  useEffect(() => {
    if (slides.length < 2 || paused) {
      return;
    }
    const id = window.setInterval(() => {
      setIndex((current) => (current + 1) % slides.length);
    }, 5500);
    return () => window.clearInterval(id);
  }, [slides.length, paused]);

  if (slides.length === 0) {
    return null;
  }

  const slide = slides[index] ?? slides[0];

  function go(delta: number) {
    setIndex((current) => (current + delta + slides.length) % slides.length);
  }

  return (
    <section
      className={embedded ? undefined : "clinic-shell-bleed pb-4 sm:pb-6"}
      aria-roledescription="carousel"
      aria-label="إعلانات العيادة"
    >
      {embedded ? null : (
        <p className="mb-3 px-3 text-sm text-muted sm:px-0">إعلان من العيادة</p>
      )}
      <div
        className={cn(
          "overflow-hidden bg-surface-dark text-primary-foreground",
          embedded ? "rounded-[1.75rem]" : "rounded-none sm:rounded-[1.75rem]"
        )}
      >
        {/* Mobile: stacked full-bleed; tablet+: keep single column until lg split */}
        <div className="grid items-center gap-6 px-4 py-7 sm:gap-8 sm:px-8 sm:py-9 md:px-10 md:py-10 lg:grid-cols-[1.15fr_0.85fr] lg:px-12">
          <div>
            <p className="text-sm font-medium text-primary-foreground/55">{slide.eyebrow}</p>
            <h2 className="mt-2 text-xl font-extrabold leading-tight tracking-tight sm:mt-3 sm:text-2xl md:text-3xl">
              {slide.title}
            </h2>
            <p className="mt-3 max-w-xl text-sm leading-7 text-primary-foreground/70 sm:leading-8">{slide.body}</p>
            {slide.ctaHref && slide.ctaLabel ? (
              <Link
                href={slide.ctaHref}
                className="mt-5 inline-flex w-full items-center justify-center rounded-xl bg-accent px-5 py-3 text-sm font-semibold text-primary-foreground transition hover:bg-accent-soft sm:mt-6 sm:w-auto"
              >
                {slide.ctaLabel}
              </Link>
            ) : null}
          </div>
          <div className="flex justify-start lg:justify-end">
            <Bars />
          </div>
        </div>
        {slides.length > 1 ? (
          <div className="flex flex-wrap items-center justify-between gap-3 px-4 pb-4 sm:px-8 sm:pb-5 md:px-10 lg:px-12">
            <div className="flex items-center gap-2">
              {slides.map((item, i) => (
                <button
                  key={item.id}
                  type="button"
                  aria-label={`الإعلان ${i + 1}`}
                  aria-current={i === index}
                  className={cn(
                    "h-2 w-2 rounded-full border border-white/70 transition",
                    i === index ? "bg-white" : "bg-transparent"
                  )}
                  onClick={() => setIndex(i)}
                />
              ))}
              <button
                type="button"
                className="ms-3 inline-flex items-center gap-1.5 text-xs text-primary-foreground/70 transition hover:text-primary-foreground"
                onClick={() => setPaused((value) => !value)}
              >
                <span aria-hidden="true">{paused ? "▶" : "⏸"}</span>
                {paused ? "تشغيل" : "إيقاف"}
              </button>
            </div>
            <div className="inline-flex overflow-hidden rounded-full bg-white/10">
              <button
                type="button"
                aria-label="السابق"
                className="px-3 py-1.5 text-sm text-primary-foreground/80 transition hover:bg-white/10"
                onClick={() => go(-1)}
              >
                ‹
              </button>
              <button
                type="button"
                aria-label="التالي"
                className="px-3 py-1.5 text-sm text-primary-foreground/80 transition hover:bg-white/10"
                onClick={() => go(1)}
              >
                ›
              </button>
            </div>
          </div>
        ) : null}
      </div>
    </section>
  );
}
