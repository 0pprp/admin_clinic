"use client";

import { useId, useState } from "react";
import type { FaqItem } from "@/lib/api/public-types";

export function FaqAccordion({ items }: { items: FaqItem[] }) {
  const baseId = useId();
  const [openIds, setOpenIds] = useState<string[]>([]);

  function toggle(id: string) {
    setOpenIds((current) => (current.includes(id) ? current.filter((item) => item !== id) : [...current, id]));
  }

  return (
    <div className="divide-y divide-border border-y border-border">
      {items.map((item, index) => {
        const panelId = `${baseId}-panel-${index}`;
        const buttonId = `${baseId}-button-${index}`;
        const open = openIds.includes(item.id);

        return (
          <div key={item.id}>
            <h3>
              <button
                id={buttonId}
                type="button"
                className="flex w-full items-start justify-between gap-6 py-5 text-right text-lg font-semibold"
                aria-expanded={open}
                aria-controls={panelId}
                onClick={() => toggle(item.id)}
              >
                <span>{item.question}</span>
                <span aria-hidden="true" className="mt-1 text-accent">
                  {open ? "−" : "+"}
                </span>
              </button>
            </h3>
            <div id={panelId} role="region" aria-labelledby={buttonId} hidden={!open} className="pb-5">
              <p className="max-w-3xl text-sm leading-8 text-muted">{item.answer}</p>
            </div>
          </div>
        );
      })}
    </div>
  );
}
