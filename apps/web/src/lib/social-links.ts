export type SocialLink = {
  label: string;
  href: string;
};

const LABELS: Record<string, string> = {
  instagram: "إنستغرام",
  twitter: "إكس",
  x: "إكس",
  linkedin: "لينكدإن",
  youtube: "يوتيوب",
  facebook: "فيسبوك",
  tiktok: "تيك توك",
  telegram: "تيليغرام"
};

function isSafeHttpUrl(value: string): boolean {
  try {
    const url = new URL(value);
    return url.protocol === "https:" || url.protocol === "http:";
  } catch {
    return false;
  }
}

export function parseSocialLinks(value: string | null | undefined): SocialLink[] {
  if (!value) {
    return [];
  }

  try {
    const parsed = JSON.parse(value) as unknown;

    if (Array.isArray(parsed)) {
      return parsed
        .map((item) => {
          if (typeof item === "string" && isSafeHttpUrl(item)) {
            return { label: "رابط", href: item };
          }

          if (item && typeof item === "object" && "href" in item) {
            const href = String((item as { href: unknown }).href);
            const label =
              "label" in item ? String((item as { label: unknown }).label) : "رابط";
            return isSafeHttpUrl(href) ? { label, href } : null;
          }

          return null;
        })
        .filter((item): item is SocialLink => item !== null);
    }

    if (parsed && typeof parsed === "object") {
      return Object.entries(parsed as Record<string, unknown>)
        .map(([key, href]) => {
          if (typeof href !== "string" || !isSafeHttpUrl(href)) {
            return null;
          }

          return { label: LABELS[key.toLowerCase()] ?? key, href };
        })
        .filter((item): item is SocialLink => item !== null);
    }
  } catch {
    return [];
  }

  return [];
}
