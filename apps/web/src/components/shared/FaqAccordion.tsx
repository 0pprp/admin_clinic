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
    <div className="clinic-panel divide-y divide-border overflow-hidden">
      {items.map((item, index) => {
        const panelId = `${baseId}-panel-${index}`;
        const buttonId = `${baseId}-button-${index}`;
        const open = openIds.includes(item.id);

        return (
          <div key={item.id} className="px-5 sm:px-6">
            <h3>
              <button
                id={buttonId}
                type="button"
                className="flex w-full items-start justify-between gap-6 py-5 text-right text-lg font-semibold transition hover:text-accent"
                aria-expanded={open}
                aria-controls={panelId}
                onClick={() => toggle(item.id)}
              >
                <span>{item.question}</span>
                <span aria-hidden="true" className="mt-1 shrink-0 text-accent">
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
